using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Application.Abstractions.Filings;

public interface IInsiderFilingSource
{
  //Fetch and parse one filing. Throws on unfetchable/unparseable 
  Task<ParsedForm4> GetFilingAsync(FilingReference reference, CancellationToken cancellationToken);
  
  Task<IReadOnlyList<FilingReference>> GetRecentForm4ReferencesAsync(string cik, CancellationToken cancellationToken);
}