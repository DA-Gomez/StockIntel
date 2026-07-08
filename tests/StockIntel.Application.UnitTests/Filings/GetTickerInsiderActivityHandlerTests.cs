using FluentAssertions;
using NSubstitute;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Filings.InsiderActivity;
using StockIntel.Application.UnitTests.TestDoubles;

namespace StockIntel.Application.UnitTests.Filings;

public class GetTickerInsiderActivityHandlerTests
{
  private readonly IInsiderActivityReader _reader = Substitute.For<IInsiderActivityReader>();
  private readonly GetTickerInsiderActivityHandler _handler;

  public GetTickerInsiderActivityHandlerTests() => _handler = new(_reader);

  [Fact]
  public async Task Full_page_plus_one_yields_trimmed_page_and_cursor_from_last_kept_item()
  {
    var items = Enumerable.Range(0, 3)
      .Select(i => InsiderActivityItems.Create(new DateOnly(2026, 4, 3 - i)))
      .ToList();
    _reader.GetActivityAsync(Arg.Any<IReadOnlyCollection<string>>(),
      null, null, 3, Arg.Any<CancellationToken>())
      .Returns(items);
    
    var page = await _handler.HandleAsync(new GetTickerInsiderActivityQuery("AAPL", Cursor: null, PageSize: 2), default);

    page.Items.Should().HaveCount(2);
    page.NextCursor.Should().Be(ActivityCursor.Encode(items[1].TransactionDate, items[1].TransactionId));
  }

  [Fact]
  public async Task Invalid_cursor_is_a_400_shaped_error()
    => await FluentActions.Invoking(() => _handler.HandleAsync(
      new GetTickerInsiderActivityQuery("AAPL", "garbage", null), default
    )).Should().ThrowAsync<ArgumentException>();
}