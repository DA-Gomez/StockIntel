using FluentAssertions;
using StockIntel.Infrastructure.Filings;

namespace StockIntel.Infrastructure.UnitTests.Filings;

public class Form4ParserTests
{
  private static string Fixture(string name) =>
    File.ReadAllText(Path.Combine("Filings", "Fixtures", name));

  [Fact]
  public void Parses_officer_sale_with_footnoted_price()
  {
    var parsed = Form4Parser.Parse(Fixture("officer-sale.xml"));

    parsed.IssuerCik.Should().Be("0000320193");
    parsed.InsiderName.Should().Be("COOK TIMOTHY D");
    parsed.IsOfficer.Should().BeTrue();
    parsed.IsTenPercentOwner.Should().BeFalse();
    parsed.OfficerTitle.Should().Be("Chief Executive Officer");
    parsed.Transactions.Should().HaveCount(2);

    var first = parsed.Transactions[0];
    first.Code.Should().Be("S");
    first.Shares.Should().Be(100000m);
    first.PricePerShare.Should().Be(171.9412m);// footnote sibling didn't corrupt it
    first.IsAcquisition.Should().BeFalse();
    first.SharesOwnedAfter.Should().Be(3380180m);
    first.IsDirectOwnership.Should().BeTrue();
  }

  [Fact]
  public void Parses_gift_without_price_and_ignores_holdings()
  {
    var parsed = Form4Parser.Parse(Fixture("gift-with-holding.xml"));

    parsed.IsDirector.Should().BeTrue();
    parsed.OfficerTitle.Should().BeNull();
    parsed.Transactions.Should().HaveCount(1);// the holding was ignored

    var gift = parsed.Transactions.Single();
    gift.Code.Should().Be("G");
    gift.PricePerShare.Should().BeNull();// no price element at all
    gift.IsDirectOwnership.Should().BeFalse();
  }

  [Fact]
  public void Real_world_filing_parses_without_suprises()
  {
    var parsed = Form4Parser.Parse(Fixture("real-form4.xml"));

    parsed.InsiderName.Should().NotBeNullOrWhiteSpace();
    parsed.IssuerCik.Should().MatchRegex("^[0-9]{10}$");// made loose intentionally: this test's job is "reality doesn't throw"
  }

  [Fact]
  public void Non_ownership_document_is_rejected_loudly()
  {
    FluentActions.Invoking(() => Form4Parser.Parse("<html><body>EDGAR error page</body></html>"))
      .Should().Throw<FormatException>();
  }
}