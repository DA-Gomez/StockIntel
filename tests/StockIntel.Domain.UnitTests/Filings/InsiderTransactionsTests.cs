using FluentAssertions;
using StockIntel.Domain.Filings;

namespace StockIntel.Domain.UnitTests.Filings;

public class InsiderTransactionsTests
{
  [Fact]
  public void Open_market_purchase_is_classified_from_code_and_direction()
  {
    var filing = CreateFiling();
    filing.AddTransaction(new DateOnly(2026, 4, 2), "P", 1000m, 171.94m,
      isAcquisition: true, sharesOwnedAfter: 5000m, isDirectOwnership: true);

      filing.Transactions.Single().IsOpenMarketPurchase.Should().BeTrue();
      filing.Transactions.Single().IsOpenMarketSale.Should().BeFalse();
  }

  [Fact]
  public void Unknown_transaction_codes_are_accepted_not_rejected()
  {
    var filing = CreateFiling();
    filing.AddTransaction(new DateOnly(2026, 4, 2), "Z9", 1000m, null,
      isAcquisition: false, sharesOwnedAfter: null, isDirectOwnership: true);

      filing.Transactions.Should().HaveCount(1);
  }    
  
  // a valid filing with realistic defaults. Transactions have internal constructor
  private static InsiderFiling CreateFiling() =>
    InsiderFiling.Create(
      companyId: Guid.NewGuid(),
      accessionNumber: "0001214156-26-000043",
      filingDate: new DateOnly(2026, 4, 3),
      insiderCik: "1214156",
      insiderName: "COOK TIMOTHY D",
      isDirector: true,
      isOfficer: true,
      isTenPercentOwner: false,
      officerTitle: "Chief Executive Officer");
}