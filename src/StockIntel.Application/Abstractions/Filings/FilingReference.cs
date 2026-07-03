namespace StockIntel.Application.Abstractions.Filings;

//enough info to locate one filing in edgar. PrimaryDocument is stored exactly as EDGAR reports it
public record FilingReference(string Cik, string AccessionNumber, DateOnly FilingDate, string PrimaryDocument);