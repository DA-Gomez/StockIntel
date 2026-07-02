namespace StockIntel.Domain.Filings;

public class InsiderTransaction
{
  public Guid Id { get; private set; }
  public Guid FilingId { get; private set; }
  public DateOnly TransactionDate { get; private set; }

  // Raw SEC transaction code, stored as is. Known values include P, S, A, M, G, F
  public string Code { get; private set; } = string.Empty;
  
  public decimal Shares { get; private set; }
  public decimal? PricePerShare { get; private set; } //can be null because of gifts
  public bool IsAcquisition { get; private set; } // EDGAR's A/D flag
  public decimal? SharesOwnedAfter { get; private set; }
  public bool IsDirectOwnership { get; private set; }

  // interpretation here, not in storage
  public bool IsOpenMarketPurchase => Code == "P" && IsAcquisition;
  public bool IsOpenMarketSale => Code == "S" && !IsAcquisition;

  private InsiderTransaction() {}

  internal InsiderTransaction(
    Guid filingId,
    DateOnly transactionDate,
    string code, 
    decimal shares,
    decimal? pricePerShare,
    bool isAcquisition,
    decimal? sharesOwnedAfter,
    bool isDirectOwnership)
  {
    if (string.IsNullOrWhiteSpace(code))
      throw new ArgumentException("Transaction code is required", nameof(code));
    if (shares < 0)
      throw new ArgumentException("Shares can't be negative", nameof(shares));
    if (pricePerShare is < 0)
      throw new ArgumentException("Price can't be negative", nameof(pricePerShare));

    Id = Guid.NewGuid();
    FilingId = filingId;
    TransactionDate = transactionDate;
    Code = code.Trim().ToUpperInvariant();
    Shares = shares;
    PricePerShare = pricePerShare;
    IsAcquisition = isAcquisition;
    SharesOwnedAfter = sharesOwnedAfter;
    IsDirectOwnership = isDirectOwnership;
  }
}