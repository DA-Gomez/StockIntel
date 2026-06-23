using FluentAssertions;
using StockIntel.Domain.Common;

namespace StockIntel.Domain.UnitTests.Common;

public class TickerTests
{
  [Theory]
  [InlineData("aapl", "AAPL")]
  [InlineData("  msft  ", "MSFT")]
  [InlineData("NVDA", "NVDA")]
  public void Create_NormalizesToUppercaseAndTrims(string input, string expected)
  {
    var ticker = Ticker.Create(input);

    ticker.Symbol.Should().Be(expected);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("LOOOONG")]
  [InlineData("123")]
  [InlineData("A1")]
  public void Create_WithInvalidSymbol_Throws(string symbol)
  {
    var act = () => Ticker.Create(symbol);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Equality_TwoTickersWithSameSymbol_AreEqual()
  {
    var a = Ticker.Create("AAPL");
    var b = Ticker.Create("aapl");

    a.Should().Be(b);
    (a==b).Should().BeTrue();
  }
}