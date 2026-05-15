using StockIntel.Domain.ValueObjects;

namespace StockIntel.Domain.Entities;

public class WatchlistItem
{
  public Guid Id { get; private set; }
  public Guid WatchlistId { get; private set; }
  public Ticker Ticker { get; private set; } = null!;
  public DateTime AddedAt { get; private set; }

  private WatchlistItem() { } //ef core

  //intenal means that this function can only be called from within the domain 
  internal WatchlistItem(Guid watchlistId, Ticker ticker)
  {
    Id = Guid.NewGuid();
    WatchlistId = watchlistId;
    Ticker = ticker;
    AddedAt = DateTime.UtcNow;
  }
}