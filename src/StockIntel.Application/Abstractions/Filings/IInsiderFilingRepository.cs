using StockIntel.Domain.Filings;

namespace StockIntel.Application.Abstractions.Filings;

public interface IInsiderFilingRepository
{
  Task<bool> ExistsAsync(string accessionNumber, CancellationToken cancellationToken);

  //adds the filing and saves immediately
  //exception to the unit-of-work convention because its contract is "insert idempotently":
  //returns false (instead of throwing) when the accession number already exists. 
  //The duplicate can only be detected at save time, so the save must live where the database's answer can be translated
  Task<bool> TryAddAsync(InsiderFiling filing, CancellationToken cancellationToken);
}