using Microsoft.EntityFrameworkCore;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Domain.Users.Watchlists;

namespace StockIntel.Infrastructure.Persistence.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
  private readonly AppDbContext _db;

  public WatchlistRepository (AppDbContext db)
  {
    _db = db;
  }

  public Task<Watchlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return _db.Watchlists.Include(w => w.Items).FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyList<Watchlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _db.Watchlists.Include(w => w.Items).Where(w => w.UserId == userId).ToListAsync(cancellationToken);
  }

  public async Task AddAsync(Watchlist watchlist, CancellationToken cancellationToken)
  {
    await _db.Watchlists.AddAsync(watchlist, cancellationToken);
  }

  public async Task<IReadOnlyList<string>> GetAllWatchedSymbolsAsync(CancellationToken cancellationToken)
  {
    var tickers = await _db.Set<WatchlistItem>()
      .Select(i => i.Ticker)
      .Distinct()
      .ToListAsync(cancellationToken);

    return tickers.Select(t => t.Symbol).ToList();
  }

}

//.Include() tells EF to eagerly load the items collection in the same query