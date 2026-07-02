namespace StockIntel.Domain.Filings;

public class CompanyTicker
{
  public Guid Id { get; private set; }
  public Guid CompanyId { get; private set; }
  public Common.Ticker Ticker { get; private set; } = null!;

  private CompanyTicker() { } //ef core

  internal CompanyTicker(Guid companyId, Common.Ticker ticker)
  {
    Id = Guid.NewGuid();
    CompanyId = companyId;
    Ticker = ticker;
  }
}