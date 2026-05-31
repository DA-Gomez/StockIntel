using StockIntel.Domain.Entities;

namespace StockIntel.Application.Abstractions.Persistence;

public interface IWatchlistRepository
{
  Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<IReadOnlyList<Watchlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
  Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken);
}