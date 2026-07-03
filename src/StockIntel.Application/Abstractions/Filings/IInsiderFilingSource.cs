using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Application.Abstractions.Filings;

public interface IInsiderFilingSource
{
  //
  Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken);
}