using StockIntel.Domain.Common;

namespace StockIntel.Domain.Users.Watchlists;

public class Watchlist {
  private readonly List<WatchlistItem> _items = new();

  public Guid Id { get; private set; }
  public Guid UserId { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }

  public IReadOnlyCollection<WatchlistItem> Items => _items;

  private Watchlist() { } //entity framework core

  public static Watchlist Create(Guid userId, string name) {
    if (userId == Guid.Empty)
      throw new ArgumentException("UserId is required", nameof(userId));
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Watchlist name is required", nameof(name));
    if (name.Length > 100)
      throw new ArgumentException("Watchlist name connot exceed 100 characters", nameof(name));

    return new Watchlist {
      Id = Guid.NewGuid(),
      UserId = userId,
      Name = name.Trim(),
      CreatedAt = DateTime.UtcNow
    };
  }

  public void AddTicker(Ticker ticker) {
    if (_items.Any(i => i.Ticker == ticker)) return;

    _items.Add(new WatchlistItem(Id, ticker));
  }

  public void RemoveTicker(Ticker ticker) {
    _items.RemoveAll(i => i.Ticker == ticker);
  }
}