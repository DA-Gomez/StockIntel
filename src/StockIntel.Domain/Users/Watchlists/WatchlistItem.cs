using StockIntel.Domain.Common;

namespace StockIntel.Domain.Users.Watchlists;

public class WatchlistItem
{
  public Guid Id { get; private set; }
  public Guid WatchlistId { get; private set; }
  public Ticker Ticker { get; private set; } = null!; //nullable but telling compiler it wont be
  public DateTime AddedAt { get; private set; }

  private WatchlistItem() { } //ef core

  //intenal means that this function can only be called from within Domain 
  internal WatchlistItem(Guid watchlistId, Ticker ticker)
  {
    Id = Guid.NewGuid();
    WatchlistId = watchlistId;
    Ticker = ticker;
    AddedAt = DateTime.UtcNow;
  }
}