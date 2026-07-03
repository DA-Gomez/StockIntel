using FluentAssertions;
using StockIntel.Domain.Filings;

namespace StockIntel.Domain.UnitTests.Filings;

public class InsiderFilingsTests
{
  [Theory]
  [InlineData("0001214156-26-000043", "0001214156-26-000043")]
  [InlineData("000121415626000043",   "0001214156-26-000043")] // undashed normalizes
  public void NormalizeAccessionNumber_produces_canonical_dashed_form(string input, string expected)
   => InsiderFilings.NormalizeAccessionNumber(input).Should().Be(expected);
}