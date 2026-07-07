namespace StockIntel.Application.Filings.IngestWatchedTickers;

public record IngestionRunSummary(int TickersProcessed, int TickersFailed, int NewFilings);