using StockIntel.Domain.Common;

namespace StockIntel.Application.Abstractions.Filings;

public interface ICompanyDirectory
{
  //Resolve a ticker to its SEC identity. null = unknown ticker
  Task<CompanyIdentity?> ResolveAsync(Ticker ticker, CancellationToken cancellationToken);
}