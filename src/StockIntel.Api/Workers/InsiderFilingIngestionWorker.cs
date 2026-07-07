using Microsoft.Extensions.Options;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestWatchedTickers;

namespace StockIntel.Api.Workers;

public sealed class InsiderFilingIngestionWorker : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory; //not the handler
  private readonly IOptions<IngestionOptions> _options;
  private readonly ILogger<InsiderFilingIngestionWorker> _logger;

  public InsiderFilingIngestionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<IngestionOptions> options,
    ILogger<InsiderFilingIngestionWorker> logger)
  {
    _scopeFactory = scopeFactory;
    _options = options;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var options = _options.Value;
    if (!options.Enabled)
    {
      _logger.LogWarning("Insider filing ingestion is disabled by config");
      return;
    }

    _logger.LogInformation("Insider filing ingestion worker started; interval {Interval}", options.Interval);

    try
    {
      using var timer = new PeriodicTimer(options.Interval);
      do
      {
        await RunOnceAsync(cancellationToken);
      }
      while (await timer.WaitForNextTickAsync(cancellationToken));
    }
    catch (OperationCanceledException)
    {
      //shutdown not an error: host cancelled the token
    }

    _logger.LogInformation("Insider filing ingestion worker stopped");
  }

  private async Task RunOnceAsync(CancellationToken cancellationToken)
  {
    try
    {
      //scope per run
      using var scope = _scopeFactory.CreateScope();
      var handler = scope.ServiceProvider
        .GetRequiredService<ICommandHandler<IngestWatchedTickersCommand, IngestionRunSummary>>();
      var summary = await handler.HandleAsync(new IngestWatchedTickersCommand(), cancellationToken);

      _logger.LogInformation(
        "Ingestion run complete: {NewFilings} new filings across {Tickers} tickers ({Failed} failed):",
        summary.NewFilings, summary.TickersProcessed, summary.TickersFailed
      );
    }
    catch (Exception ex) when (!cancellationToken.IsCancellationRequested) //any failure is logged
    {
      //run level isolation. letting this esape stops completely
      _logger.LogError(ex, "Ingestion run failed; will retry on the next tick");
    }
  }
}