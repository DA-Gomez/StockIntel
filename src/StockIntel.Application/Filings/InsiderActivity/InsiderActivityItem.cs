namespace StockIntel.Application.Filings.InsiderActivity;

public record InsiderActivityItem(
  Guid TransactionId,
  string Symbol,
  string CompanyName,
  string InsiderName,
  string? OfficerTitle,
  bool IsDirector,
  bool IsOfficer,
  bool IsTenPercentOwner,
  DateOnly TransactionDate,
  string Code, //raw SEC vocabulary
  string ActivityType, // "Purchase" | "Sale" | "Other" (server side)
  decimal Shares,
  decimal? PricePerShare,
  decimal? TotalValue,  // shares * price, null when price unknown
  decimal? SharesOwnedAfter,
  DateOnly FilingDate,
  string AccessionNumber);