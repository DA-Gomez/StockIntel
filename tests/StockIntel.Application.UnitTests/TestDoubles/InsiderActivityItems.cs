using StockIntel.Application.Filings.InsiderActivity;

namespace StockIntel.Application.UnitTests.TestDoubles;

// Builds InsiderActivityItem fixtures. Every field has an Apple/Tim-Cook default so a test
// only names what it cares about; transactionId defaults to a fresh Guid and filingDate to the
// day after the transaction.
public static class InsiderActivityItems
{
  public static InsiderActivityItem Create(
    DateOnly transactionDate,
    Guid? transactionId = null,
    string symbol = "AAPL",
    string companyName = "Apple Inc.",
    string insiderName = "COOK TIMOTHY D",
    string? officerTitle = "Chief Executive Officer",
    bool isDirector = true,
    bool isOfficer = true,
    bool isTenPercentOwner = false,
    string code = "S",
    string activityType = "Sale",
    decimal shares = 100000m,
    decimal? pricePerShare = 171.9412m,
    decimal? totalValue = 17194120m,
    decimal? sharesOwnedAfter = 3380180m,
    DateOnly? filingDate = null,
    string accessionNumber = "0001214156-26-000043")
    => new(
      transactionId ?? Guid.NewGuid(),
      symbol,
      companyName,
      insiderName,
      officerTitle,
      isDirector,
      isOfficer,
      isTenPercentOwner,
      transactionDate,
      code,
      activityType,
      shares,
      pricePerShare,
      totalValue,
      sharesOwnedAfter,
      filingDate ?? transactionDate.AddDays(1),
      accessionNumber);
}
