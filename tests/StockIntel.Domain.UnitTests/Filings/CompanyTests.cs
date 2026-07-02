using FluentAssertions;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Domain.UnitTests.Filings;

public class CompanyTests
{
  [Theory]
  [InlineData("320193", "0000320193")]
  [InlineData("0000320193", "0000320193")]
  [InlineData("   320193 ", "0000320193")]
  public void NormalizeCik_produces_cononical_ten_digit_form(string input, string expected)
    => Company.NormalizeCik(input).Should().Be(expected);
  
  [Theory]
  [InlineData("")]
  [InlineData("12345678901")] // digits
  [InlineData("32O193")] //fixes copy paste
  public void NormalizeCik_rejects_invalid_input(string input)
    => FluentActions.Invoking(() => Company.NormalizeCik(input)).Should().Throw<ArgumentException>();

  [Fact]
  public void AddTickerListening_is_idempotent()
  {
    var company = Company.Create("320193", "Apple Inc.");
    var ticker = Ticker.Create("AAPL");

    company.AddTickerListening(ticker);
    company.AddTickerListening(ticker);

    company.Tickers.Should().HaveCount(1);
  }
}