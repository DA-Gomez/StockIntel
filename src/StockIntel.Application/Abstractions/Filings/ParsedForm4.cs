namespace StockIntel.Application.Abstractions.Filings;
// source agnostic (doesnt matter where) parse. filng date come from discovery (FilingReference)

public record ParsedForm4(
  string IssuerCik,
  string InsiderName,
  bool IsDirector,
  bool IsOfficer,
  bool IsTenPercentOwner,
  string? OfficerTitle,
  IReadOnlyList<ParsedForm4Transaction> Transactions);

public record ParsedForm4Transaction(
  DateOnly TransactionDate,
  string Code,
  decimal Shares,
  decimal? PricePerShare,
  bool IsAcquisition,
  decimal? SharesOwnedAfter,
  bool IsDirectOwnership);