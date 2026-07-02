namespace StockIntel.Domain.Filings;

public class Company
{
  private readonly List<CompanyTicker> _tickers = new();

  public Guid Id { get; private set; }
  public string Cik { get; private set; } = string.Empty; //10 digits, zero-padded
  public string Name { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }
  public IReadOnlyCollection<CompanyTicker> Tickers => _tickers;

  private Company() { } //ef core

  public static Company Create(string cik, string name)
  {
    if (string.IsNullOrWhiteSpace(name)) 
      throw new ArgumentException("Company name is required", nameof(name));
    
    return new Company
    {
      Id = Guid.NewGuid(),
      Cik = NormalizeCik(cik),
      Name = name.Trim(),
      CreatedAt =  DateTime.UtcNow
    };
  }

  // EDGAR represents CIKs inconsistently: "320193", "0000320193", sometimes
  // with surrounding whitespace. We accept all of them and store exactly one
  // canonical form. Normalize at the boundary; never store two spellings of
  // the same identity.
  public static string NormalizeCik(string cik)
  {
    if (string.IsNullOrWhiteSpace(cik)) 
      throw new ArgumentException("Cik is required", nameof(cik));
    
    var trimmed = cik.Trim().TrimStart('0');
    if (trimmed.Length == 0) trimmed = "0";
    if (trimmed.Length > 10 || !trimmed.All(char.IsDigit))//checks if every char is a digit
      throw new ArgumentException($"{cik} is not a valid CIK", nameof(cik));

    return trimmed.PadLeft(10, '0');
  }

  public void AddTickerListening(Common.Ticker ticker)
  {
    if (_tickers.Any(t => t.Ticker == ticker)) return;
    _tickers.Add(new CompanyTicker(Id, ticker));
  }

}