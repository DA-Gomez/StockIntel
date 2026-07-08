using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Api.IntegrationTests.TestDoubles;

// Serves <paramref name="filingCount"/> Form 4 filings for whichever issuer CIK it is asked
// about, on consecutive descending dates, each with a single "Sale" transaction the day before
// its filing date. Accession numbers are derived from that CIK, so two tests pointed at
// different companies never collide on the (globally unique) accession index in the shared DB.
// filingCount: 1 (the default) is the single-filing fixture; bump it to give pagination
// something to page through.
public sealed class StubSource : IInsiderFilingSource
{
  private const string InsiderCik = "0001214156";
  private const string InsiderName = "COOK TIMOTHY D";
  private const string OfficerTitle = "Chief Executive Officer";
  private static readonly DateOnly BaseFilingDate = new(2026, 4, 3);

  private readonly int _filingCount;

  public StubSource(int filingCount = 1) => _filingCount = filingCount;

  public Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken)
    => Task.FromResult<IReadOnlyList<FilingReference>>(
      Enumerable.Range(0, _filingCount)
        .Select(i => new FilingReference(
          cik,
          $"{cik}-26-{43 + i:D6}",
          BaseFilingDate.AddDays(-i),
          $"wk-form4_{i + 1}.xml"))
        .ToList());

  public Task<ParsedForm4> GetFilingAsync(FilingReference reference, CancellationToken cancellationToken)
    => Task.FromResult(new ParsedForm4(
      reference.Cik, InsiderCik, InsiderName,
      IsDirector: true, IsOfficer: true, IsTenPercentOwner: false, OfficerTitle: OfficerTitle,
      Transactions: new[]
      {
        new ParsedForm4Transaction(reference.FilingDate.AddDays(-1), "S", 100000m, 171.9412m,
          IsAcquisition: false, SharesOwnedAfter: 3380180m, IsDirectOwnership: true)
      }));
}
