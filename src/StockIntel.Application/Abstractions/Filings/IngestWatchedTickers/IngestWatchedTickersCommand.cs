using StockIntel.Application.Common;

namespace StockIntel.Application.Filings.IngestWatchedTickers;

public record IngestWatchedTickersCommand : ICommand<IngestionRunSummary>;