using Microsoft.EntityFrameworkCore;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Domain.Entities;

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

}

//.Include() tells EF to eagerly load the items collection in the same query