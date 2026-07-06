using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestTickerFilings;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;
using StockIntel.Infrastructure.Persistence;

namespace StockIntel.Api.IntegrationTests;

//read handler test against real test containers (Postgres)
//only edgar edge by overriding the two App ports in the test hosts dep inj
[Collection("Postgres")]
public class IngestTickerFilingsTests
{
  private readonly WebApplicationFactory<Program> _factory;

  public IngestTickerFilingsTests(PostgresFixture postgres)
  {
    //StockIntelApiFactory needs the test-container conn string, so it can't be an
    //IClassFixture (no parameterless ctor) - build it from the shared PostgresFixture,
    //then layer the edgar-port stubs on top.
    _factory = new StockIntelApiFactory(postgres.ConnectionString)
      .WithWebHostBuilder(builder =>
        builder.ConfigureTestServices(services =>
        {
          services.RemoveAll<ICompanyDirectory>();
          services.RemoveAll<IInsiderFilingSource>();
          services.AddSingleton<ICompanyDirectory>(new StubDirectory());
          services.AddSingleton<IInsiderFilingSource>(new StubSource());
        }));
  }

  [Fact]
  public async Task Running_ingestion_twice_adds_zero_new_rows()
  {
    var first = await IngestAsync();
    var second = await IngestAsync();

    first.Should().Be(1);
    second.Should().Be(0);

    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    (await db.InsiderFilings.CountAsync()).Should().Be(1);
    (await db.Set<InsiderTransaction>().CountAsync()).Should().Be(1);
    (await db.Companies.CountAsync()).Should().Be(1);
  }

  private async Task<int> IngestAsync()
  {
    //fresh scope per run
    using var scope = _factory.Services.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<IngestTickerFilingsCommand, int>>();
    return await handler.HandleAsync(new IngestTickerFilingsCommand("AAPL"), CancellationToken.None);
  }

  private sealed class StubDirectory : ICompanyDirectory
  {
    public Task<CompanyIdentity?> ResolveAsync(Ticker ticker, CancellationToken cancellationToken)
      => Task.FromResult<CompanyIdentity?>(new CompanyIdentity("0000320193", "Apple Inc."));
  }

  private sealed class StubSource : IInsiderFilingSource
  {
    public Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken)
      => Task.FromResult<IReadOnlyList<FilingReference>>(new[]
      {
        new FilingReference("0000320193", "0001214156-26-000043", new DateOnly(2026, 4, 3), "wk-form4_1.xml")
      }
    );

    public Task<ParsedForm4> GetFilingAsync(FilingReference reference, CancellationToken cancellationToken)
      => Task.FromResult(new ParsedForm4("0000320193", "0001214156", "COOK TIMOTHY D",
        IsDirector: true, IsOfficer: true, IsTenPercentOwner: false, OfficerTitle: "Chief Executive Officer",
        Transactions: new[]
        {
          new ParsedForm4Transaction(new DateOnly(2026, 4, 2), "S", 100000m, 171.9412m,
            IsAcquisition: false, SharesOwnedAfter: 3380180m, IsDirectOwnership: true)
        })
    );
  }
}