using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.IngestTickerFilings;
using StockIntel.Application.Filings.IngestWatchedTickers;

namespace StockIntel.Application.UnitTests.Filings;

//dont test the timer, extract the work and test that
public class IngestWatchedTickersHandlerTests
{
  private readonly IWatchlistRepository _watchlists = Substitute.For<IWatchlistRepository>();
  private readonly ICommandHandler<IngestTickerFilingsCommand, int> _ingestTicker
    = Substitute.For<ICommandHandler<IngestTickerFilingsCommand, int>>();
  private readonly IngestWatchedTickersHandler _handler;

  public IngestWatchedTickersHandlerTests()
  {
    _handler = new IngestWatchedTickersHandler(_watchlists, _ingestTicker, NullLogger<IngestWatchedTickersHandler>.Instance);
  }

  [Fact]
  public async Task Ingest_each_wanted_symbol_once_and_sums_the_results()
  {
    _watchlists.GetAllWatchedSymbolsAsync(Arg.Any<CancellationToken>())
      .Returns(new[] { "AAPL", "MSFT" });
    _ingestTicker.HandleAsync(Arg.Is<IngestTickerFilingsCommand>(c => c.Symbol == "AAPL"), Arg.Any<CancellationToken>())
      .Returns(2);
    _ingestTicker.HandleAsync(Arg.Is<IngestTickerFilingsCommand>(c => c.Symbol == "MSFT"), Arg.Any<CancellationToken>())
      .Returns(1);

    var summary = await _handler.HandleAsync(new IngestWatchedTickersCommand(), default);

    summary.Should().Be(new IngestionRunSummary(2, 0, 3));
  }

  [Fact]
  public async Task One_failing_ticker_does_not_stop_the_run()
  {
    _watchlists.GetAllWatchedSymbolsAsync(Arg.Any<CancellationToken>())
      .Returns(new[] { "AAPL", "MSFT" });
    _ingestTicker.HandleAsync(Arg.Is<IngestTickerFilingsCommand>(c => c.Symbol == "AAPL"), Arg.Any<CancellationToken>())
      .ThrowsAsync(new HttpRequestException("EDGAR 503 after retried"));
    _ingestTicker.HandleAsync(Arg.Is<IngestTickerFilingsCommand>(c => c.Symbol == "MSFT"), Arg.Any<CancellationToken>())
      .Returns(1);

    var summary = await _handler.HandleAsync(new IngestWatchedTickersCommand(), default);

    summary.Should().Be(new IngestionRunSummary(2, 1, 1));
    await _ingestTicker.Received(1).HandleAsync(Arg.Is<IngestTickerFilingsCommand>(c => c.Symbol == "MSFT"), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Cancellation_is_not_swallowed_as_a_ticker_failure()
  {
    _watchlists.GetAllWatchedSymbolsAsync(Arg.Any<CancellationToken>())
      .Returns(new[] { "AAPL", "MSFT" });
    _ingestTicker.HandleAsync(Arg.Any<IngestTickerFilingsCommand>(), Arg.Any<CancellationToken>())
      .ThrowsAsync(new OperationCanceledException());

    await FluentActions.Invoking(() => _handler.HandleAsync(new IngestWatchedTickersCommand(), default))
      .Should().ThrowAsync<OperationCanceledException>();
    
    //shutdown means stop. second ticker should not be attempted
    await _ingestTicker.Received(1).HandleAsync(Arg.Any<IngestTickerFilingsCommand>(), Arg.Any<CancellationToken>());
  }
}