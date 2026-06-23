using StockIntel.Application.Abstractions.Authentication;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using StockIntel.Domain.Common
;
namespace StockIntel.Application.Watchlists.AddTicker;

public class AddTickerHandler : ICommandHandler<AddTickerCommand, Unit>
{
  private readonly IWatchlistRepository _watchlists;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICurrentUser _currentUser;

  public AddTickerHandler(
    IWatchlistRepository watchlists,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
  {
    _watchlists = watchlists;
    _unitOfWork = unitOfWork;
    _currentUser = currentUser;
  }

  public async Task<Unit> HandleAsync(AddTickerCommand command, CancellationToken cancellationToken)
  {
    var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Not authenticated");
    var watchlist = await _watchlists.GetByIdAsync(command.WatchlistId, cancellationToken)
      ?? throw new KeyNotFoundException("Watchlist not found");

    if (watchlist.UserId != userId)
      throw new UnauthorizedAccessException("Watchlist not found");
    
    var ticker = Ticker.Create(command.Symbol);
    watchlist.AddTicker(ticker);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Unit.Value;
  }
}