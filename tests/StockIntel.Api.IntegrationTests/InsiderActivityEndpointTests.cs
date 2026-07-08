using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockIntel.Api.IntegrationTests.TestDoubles;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestTickerFilings;
using StockIntel.Application.Filings.InsiderActivity;

namespace StockIntel.Api.IntegrationTests;

//register -> login -> watch -> ingest -> read -> paginate, over
// HTTP, against a real Postgres, with only the EDGAR edge stubbed. The stub source
// now serves three filings on consecutive dates so pagination has something to paginate
[Collection("Postgres")]
public class InsiderActivityEndpointTests
{
  private readonly WebApplicationFactory<Program> _factory;

  public InsiderActivityEndpointTests(PostgresFixture postgres)
  {
    _factory = new StockIntelApiFactory(postgres.ConnectionString)
      .WithWebHostBuilder(builder =>
        builder.ConfigureTestServices(services =>
        {
          services.RemoveAll<ICompanyDirectory>();
          services.RemoveAll<IInsiderFilingSource>();
          // Own company + ticker so this test's feed is isolated from sibling classes that
          // share the (never-reset) Postgres collection fixture.
          services.AddSingleton<ICompanyDirectory>(new StubDirectory(cik: "0000789019", name: "Microsoft Corp."));
          services.AddSingleton<IInsiderFilingSource>(new StubSource(filingCount: 3));
        }));
  }

  [Fact]
  public async Task Register_watch_ingest_read_paginate()
  {
    var client = _factory.CreateClient();

    //register + login
    var email = $"feed-{Guid.NewGuid():N}@test.local";
    (await client.PostAsJsonAsync("/api/users/register", new { email, password = "itsasecret" }))
      .EnsureSuccessStatusCode();
    var login = await client.PostAsJsonAsync("/api/users/login", new { email, password = "itsasecret" });
    var token = (await login.Content.ReadFromJsonAsync<JsonElement>())
      .GetProperty("accessToken").GetString();
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);

    //Watch MSFT
    var created = await client.PostAsJsonAsync("/api/watchlists", new { name = "TechWatchlist" });
    var watchlistId = (await created.Content.ReadFromJsonAsync<JsonElement>())
      .GetProperty("watchlistId")
      .GetGuid();
    (await client.PostAsJsonAsync($"/api/watchlists/{watchlistId}/tickers", new { symbol = "MSFT" }))
      .EnsureSuccessStatusCode();

    // Ingest (scope-per-run defined by worker)
    using (var scope = _factory.Services.CreateScope())
    {
      var ingest = scope.ServiceProvider.GetRequiredService<ICommandHandler<IngestTickerFilingsCommand, int>>();
      (await ingest.HandleAsync(new IngestTickerFilingsCommand("MSFT"), default))
        .Should()
        .Be(3);
    }

    // Page 1: newest two, cursor present
    var page1 = await client.GetFromJsonAsync<InsiderActivityPage>($"/api/tickers/MSFT/insider-activity?pageSize=2");
    page1!.Items.Should().HaveCount(2);
    page1.Items.Should().BeInDescendingOrder(i => i.TransactionDate);
    page1.Items[0].ActivityType.Should().Be("Sale");
    page1.NextCursor.Should().NotBeNull();

    // Page 2 via cursor: the remaining one, no cursor, no overlap
    var page2 = await client.GetFromJsonAsync<InsiderActivityPage>($"/api/tickers/MSFT/insider-activity?pageSize=2&cursor={page1.NextCursor}");
    page2!.Items.Should().HaveCount(1);
    page2.NextCursor.Should().BeNull();
    page2.Items.Select(i => i.TransactionId).Should().NotIntersectWith(page1.Items.Select(i => i.TransactionId));

    // The watchlist feed sees the same world
    var wlPage = await client.GetFromJsonAsync<InsiderActivityPage>($"/api/watchlists/{watchlistId}/insider-activity?pageSize=10");
    wlPage!.Items.Should().HaveCount(3);

    //None of it exists for the unauthenticated
    var anonymous = _factory.CreateClient();
    (await anonymous.GetAsync("/api/tickers/MSFT/insider-activity")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}