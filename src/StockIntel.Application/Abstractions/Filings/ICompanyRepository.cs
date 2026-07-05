using StockIntel.Domain.Filings;

namespace StockIntel.Application.Abstractions.Filings;

public interface ICompanyRepository
{
  Task<Company?> GetByCikAsync(string cik, CancellationToken cancellationToken);
  Task AddAsync(Company company, CancellationToken cancellationToken);
}