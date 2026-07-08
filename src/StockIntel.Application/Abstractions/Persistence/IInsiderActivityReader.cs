using StockIntel.Application.Filings.InsiderActivity;

namespace StockIntel.Application.Abstractions.Persistence;

public interface IInsiderActivityReader
{
  Task<IReadOnlyList<InsiderActivityItem>> GetActivityAsync(
    IReadOnlyCollection<string> symbols,
    DateOnly? afterTransactionDate,
    Guid? afterTransactionId,
    int limit,
    CancellationToken cancellationToken);

  
}