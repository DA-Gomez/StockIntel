namespace StockIntel.Domain.ValueObjects;

public sealed record Ticker { //record instead of class
  public string Symbol { get; }

  private Ticker(String symbol) { //private constructor prevents standard 'new' keyword usage
    Symbol = symbol;
  }

  public static Ticker Create(string symbol) {
    if (string.IsNullOrWhiteSpace(symbol))
      throw new ArgumentException("Ticker symbol is required", nameof(symbol));

    var normalized = symbol.Trim().ToUpperInvariant();

    if (normalized.Length < 1 || normalized.Length > 5) 
      throw new ArgumentException("Ticker must be 1-5 characters", nameof(symbol));
    if (!normalized.All(char.IsLetter))
      throw new ArgumentException("Ticker must contain only letters", nameof(symbol));

    return new Ticker(normalized);
  }
  public override string ToString() => Symbol;
}