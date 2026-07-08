using FluentAssertions;
using StockIntel.Application.Filings.InsiderActivity;

namespace StockIntel.Application.UnitTests.Filings;

public class ActivityCursorTests
{
  [Fact]
  public void Round_trips_date_and_id()
  {
    var date = new DateOnly(2026, 4, 2);
    var id = Guid.NewGuid();

    var cursor = ActivityCursor.Encode(date, id);
    var ok = ActivityCursor.TryDecode(cursor, out var decodeDate, out var decodedId);

    ok.Should().BeTrue();
    decodeDate.Should().Be(date);
    decodedId.Should().Be(id);
    cursor.Should().NotContainAny("+", "/", "=");
  }

  [Theory]
  [InlineData("")]
  [InlineData("not-a-cursor")]
  [InlineData("aGVsbG8")]// valid base64url, wrong payload
  public void Garbase_is_rejected_not_thrown(string cursor)
    => ActivityCursor.TryDecode(cursor, out _, out _).Should().BeFalse(); 
}