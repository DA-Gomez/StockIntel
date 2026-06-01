using FluentAssertions;
using StockIntel.Domain.Entities;
using StockIntel.Domain.ValueObjects;

namespace StockIntel.Domain.UnitTests.Entities;

public class WatchlistTest
{
  private readonly string WATCHLIST_NAME = "My watchlist";

  [Fact]
  public void Create_WithValidInput_CreatesEmptyWatchlist()
  {
    var userId = Guid.NewGuid();

    var watchlist = Watchlist.Create(userId, WATCHLIST_NAME);

    watchlist.Id.Should().NotBeEmpty();
    watchlist.UserId.Should().Be(userId);
    watchlist.Name.Should().Be(WATCHLIST_NAME);
    watchlist.Items.Should().BeEmpty();
  }

  [Fact]
  public void AddTicker_AddsItemToCollection()
  {
    var watchlist = Watchlist.Create(Guid.NewGuid(), WATCHLIST_NAME);
    var ticker = Ticker.Create("AAPL");

    watchlist.AddTicker(ticker);

    watchlist.Items.Should().HaveCount(1);
    watchlist.Items.First().Ticker.Should().Be(ticker);
  }

  [Fact]
  public void AddTicker_WithDuplicate_IsIdempotent()
  {
    var watchlist = Watchlist.Create(Guid.NewGuid(), WATCHLIST_NAME);
    var ticker = Ticker.Create("AAPL");

    watchlist.AddTicker(ticker);
    watchlist.AddTicker(ticker);

    watchlist.Items.Should().HaveCount(1);
  }

  [Fact]
  public void RemoveTicker_RemovesMatchingItem()
  {
    var watchlist = Watchlist.Create(Guid.NewGuid(), WATCHLIST_NAME);
    watchlist.AddTicker(Ticker.Create("AAPL"));
    watchlist.AddTicker(Ticker.Create("TSLA"));

    watchlist.RemoveTicker(Ticker.Create("AAPL"));

    watchlist.Items.Should().HaveCount(1);
    watchlist.Items.First().Ticker.Symbol.Should().Be("TSLA");
  }
}