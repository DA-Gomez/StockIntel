using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockIntel.Api.IntegrationTests.TestDoubles;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestTickerFilings;
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

    // Scope the row counts to this test's own company: sibling classes share the (never-reset)
    // Postgres collection fixture, so a global CountAsync() would see their data too.
    var company = await db.Companies.SingleAsync(c => c.Cik == "0000320193");
    var filing = await db.InsiderFilings.SingleAsync(f => f.CompanyId == company.Id);
    (await db.Set<InsiderTransaction>().CountAsync(t => t.FilingId == filing.Id)).Should().Be(1);
  }

  private async Task<int> IngestAsync()
  {
    //fresh scope per run
    using var scope = _factory.Services.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<IngestTickerFilingsCommand, int>>();
    return await handler.HandleAsync(new IngestTickerFilingsCommand("AAPL"), CancellationToken.None);
  }
}