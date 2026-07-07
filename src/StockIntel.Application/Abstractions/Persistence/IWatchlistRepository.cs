using StockIntel.Domain.Users.Watchlists;

namespace StockIntel.Application.Abstractions.Persistence;

public interface IWatchlistRepository
{
  Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<IReadOnlyList<Watchlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
  Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken);

  //Distinct ticker symbols across all users' watchlists, the ingestion working set
  Task<IReadOnlyList<string>> GetAllWatchedSymbolsAsync(CancellationToken cancellationToken);
}