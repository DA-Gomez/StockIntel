using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Infrastructure.UnitTests.TestDoubles;

namespace StockIntel.Infrastructure.UnitTests.Filings;

public class EdgarComplianceTests
{
  [Fact]
  public async Task Every_edgar_request_carries_the_declared_user_agent()
  {
    var stub = StubHttpMessageHandler.RespondingWithJson("{}");

    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Edgar:UserAgent"] = "StockIntelTests test@example.com",
        ["Edgar:RequestsPerSecond"] = "10"
      })
      .Build());
    services.AddLogging();
    services.AddEdgar();
    // Re-open the named client's config and swap the wire for stub.
    // stub wins, the resilience + rate-limit handlers stay
    services.AddHttpClient(EdgarHttp.DirectoryClientName).ConfigurePrimaryHttpMessageHandler(() => stub);
    
    await using var provider = services.BuildServiceProvider();
    var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(EdgarHttp.DirectoryClientName);

    await client.GetAsync("https://www.sec.gov/files/company_tickers.json");

    stub.Requests.Should().ContainSingle()
      .Which.Headers.TryGetValues("User-Agent", out var values).Should().BeTrue();
    stub.Requests.Single().Headers.GetValues("User-Agent").Single().Should().Be("StockIntelTests test@example.com");

  }
}