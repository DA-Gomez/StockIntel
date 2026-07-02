namespace StockIntel.Domain.Filings;

public class InsiderTransaction
{
  public Guid Id { set; private get; }
  public Guid FilingId { set; private get; }
  public DateOnly TransactionDate { set; private get; }

  // Raw SEC transaction code, stored as is. Known values include P, S, A, M, G, F
  public string Code { set; private get; } = string.Empty;
  
  public decimal Shares { set; private get; }
  public decimal? PricePerShare { set; private get; } //can be null because of gifts
  public bool IsAcquisition { set; private get; } // EDGAR's A/D flag
  public decimal? SharesOwnedAfter { set; private get; }
  public bool IsDirectOwnership { set; private get; }

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