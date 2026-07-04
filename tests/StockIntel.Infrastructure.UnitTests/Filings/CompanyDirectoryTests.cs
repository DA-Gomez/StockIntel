using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StockIntel.Domain.Common;
using StockIntel.Infrastructure.Filings;
using StockIntel.Infrastructure.UnitTests.TestDoubles;

namespace StockIntel.Infrastructure.UnitTests.Filings;

public class CompanyDirectoryTests
{
  private const string DirectoryJson = 
    """{"0":{"cik_str":320193,"ticker":"AAPL","title":"Apple Inc."},"1":{"cik_str":789019,"ticker":"MSFT","title":"MICROSOFT CORP"}}""";

  private static (CompanyDirectory Directory, StubHttpMessageHandler Stub) CreateSut()
  {
    var stub = StubHttpMessageHandler.RespondingWithJson(DirectoryJson);
    var dir = new CompanyDirectory(new FakeHttpClientFactory(
      new HttpClient(stub)), NullLogger<CompanyDirectory>.Instance);
      return (dir, stub);
  }

  [Fact]
  public async Task Resolves_known_ticker_to_normalized_cik()
  {
    var (dir, _) = CreateSut();

    var identity = await dir.ResolveAsync(Ticker.Create("aapl"), CancellationToken.None);

    identity.Should().NotBeNull();
    identity!.Cik.Should().Be("0000320193");
    identity.Name.Should().Be("Apple Inc.");
  }

  [Fact]
  public async Task Unknown_ticker_resolves_to_null_not_an_exception()
  {
    var (dir, _) = CreateSut();

    var identity = await dir.ResolveAsync(Ticker.Create("ZZZZZ"), CancellationToken.None);

    identity.Should().BeNull();
  }

  [Fact]
  public async Task Second_resolve_is_served_from_cache_not_a_second_download()
  {
    var (dir, stub) = CreateSut();

    await dir.ResolveAsync(Ticker.Create("AAPL"), CancellationToken.None);
    await dir.ResolveAsync(Ticker.Create("MSFT"), CancellationToken.None);

    stub.Requests.Should().HaveCount(1);
  }
}
