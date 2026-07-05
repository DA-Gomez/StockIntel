using StockIntel.Application.Common;

namespace StockIntel.Application.Filings.IngestTickerFilings;

//Ingest recent Form 4 filings for one ticker. Returns the number of new filings stored (0 on a fully caught-up run)
public record IngestTickerFilingsCommand(string Symbol) : ICommand<int>;