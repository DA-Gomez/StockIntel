using Microsoft.Extensions.Logging;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestTickerFilings;

namespace StockIntel.Application.Filings.IngestWatchedTickers;

public class IngestWatchedTickersHandler : ICommandHandler<IngestWatchedTickersCommand, IngestionRunSummary>
{
  private readonly IWatchlistRepository _watchlists;
  private readonly ICommandHandler<IngestTickerFilingsCommand, int> _ingestTicker;
  private readonly ILogger<IngestWatchedTickersHandler> _logger;

  public IngestWatchedTickersHandler(
    IWatchlistRepository watchlists,
    ICommandHandler<IngestTickerFilingsCommand, int> ingestTicker,
    ILogger<IngestWatchedTickersHandler> logger)
  {
    _watchlists = watchlists;
    _ingestTicker = ingestTicker;
    _logger = logger;
  }

  public async Task<IngestionRunSummary> HandleAsync(IngestWatchedTickersCommand command, CancellationToken cancellationToken)
  {
    var symbols = await _watchlists.GetAllWatchedSymbolsAsync(cancellationToken);
    if (symbols.Count == 0)
    {
      _logger.LogInformation("No Watched tickers; nothing to ingest");
      return new IngestionRunSummary(0, 0, 0);
    }

    var newFilings = 0;
    var failed = 0;

    //Sequential on prupose (the EDGAR rate limiter would serialize
    // parallel calls anyway, and one-at-a-time keeps every log line and
    // failure attributable to a ticker)
    foreach (var symbol in symbols)
    {
      cancellationToken.ThrowIfCancellationRequested();
      try
      {
        newFilings += await _ingestTicker.HandleAsync(new IngestTickerFilingsCommand(symbol), cancellationToken);
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        //ticker isolation. one does not affect the other, we let cancellations pass
        failed++;
        _logger.LogError(ex, "Ingestion failed for {Ticker}", symbol);
      }
    }

    return new IngestionRunSummary(symbols.Count, failed, newFilings);
  }
}