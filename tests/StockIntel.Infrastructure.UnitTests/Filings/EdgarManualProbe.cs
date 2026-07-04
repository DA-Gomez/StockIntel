using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Common;
using Xunit.Abstractions;

namespace StockIntel.Infrastructure.UnitTests.Filings;

//test that proves the assumption about EDGAR (check docs)
public class EdgarManualProbe(ITestOutputHelper output)
{
  // [Fact(Skip = "Manual: hits the real EDGAR API. remove Skip, run locally, restore Skip.")]
  [Fact]
  public async Task Pull_recent_form4_references_for_APPL()
  {
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
          ["Edgar:UserAgent"] = "StockIntel/0.2 (diego.a.gomez.ruiz@example.com)",
          ["Edgar:RequestsPerSecond"] = "5"
      })
      .Build());
    services.AddLogging();
    services.AddEdgar();
    await using var provider = services.BuildServiceProvider();

    var dir = provider.GetRequiredService<ICompanyDirectory>();
    var identity = await dir.ResolveAsync(Ticker.Create("AAPL"), CancellationToken.None);
    output.WriteLine($"AAPL -> CIK {identity!.Cik} ({identity.Name})");

    var source = provider.GetRequiredService<IInsiderFilingSource>();
    var refs = await source.GetRecentForm4ReferencesAsync(identity.Cik, CancellationToken.None);
    output.WriteLine($"Recent Form 4 filings: {refs.Count}");
    foreach (var r in refs.Take(5))
      output.WriteLine($"{r.FilingDate} {r.AccessionNumber} {r.PrimaryDocument}");
    }
}