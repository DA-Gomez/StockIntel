using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Application.Abstractions.Filings;

public interface IInsiderFilingSource
{
  //Fetch and parse one filing. Throws on unfetchable/unparseable 
  Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken);
}