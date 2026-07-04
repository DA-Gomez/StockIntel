using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Filings;

public sealed class EdgarInsiderFilingSource : IInsiderFilingSource
{
  private readonly HttpClient _http; //typed client (configured in AddEdgar)

  public EdgarInsiderFilingSource(HttpClient http) => _http = http;

  public async Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken)
  {
    cik = Company.NormalizeCik(cik);

    using var res = await _http.GetAsync(EdgarHttp.SubmissionsUrl(cik), cancellationToken);

    //unknown cik (a company can disappear from Edgar)
    if (res.StatusCode == HttpStatusCode.NotFound) return Array.Empty<FilingReference>();

    res.EnsureSuccessStatusCode();

    var payload = await res.Content.ReadFromJsonAsync<SubmissionsResponse>(cancellationToken: cancellationToken);
    var recent = payload?.Filings?.Recent;
    if (recent is null) return Array.Empty<FilingReference>();

    //columnar data

    var results = new List<FilingReference>();
    for (var i = 0; i < recent.Forms.Length; i++)
    {
      if (recent.Forms[i] != "4") continue; // excludes "4/A" amendments (ledger)

      results.Add(new FilingReference(
        cik,
        InsiderFiling.NormalizeAccessionNumber(recent.AccessionNumbers[i]),
        DateOnly.Parse(recent.FilingDates[i], CultureInfo.InvariantCulture),
        recent.PrimaryDocuments[i]));
    }
    return results;
  }

  // Deserialization targets for the slice of the response we actually read.
  // Modeling only what you consume is deliberate: unknown JSON properties are
  // ignored by default, so the SEC adding fields never breaks us. Postel again.
  private sealed record SubmissionsResponse(
      [property: JsonPropertyName("filings")] FilingsBlock? Filings);

  private sealed record FilingsBlock(
      [property: JsonPropertyName("recent")] RecentFilings? Recent);

  private sealed record RecentFilings(
      [property: JsonPropertyName("accessionNumber")] string[] AccessionNumbers,
      [property: JsonPropertyName("filingDate")] string[] FilingDates,
      [property: JsonPropertyName("form")] string[] Forms,
      [property: JsonPropertyName("primaryDocument")] string[] PrimaryDocuments);
}