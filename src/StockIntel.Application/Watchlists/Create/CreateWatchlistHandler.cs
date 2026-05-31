using StockIntel.Application.Abstractions.Authentication;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using StockIntel.Domain.Entities;

namespace StockIntel.Application.Watchlists.Create;

public class CreateWatchlistHandler : ICommandHandler<CreateWatchlistCommand, CreateWatchlistResponse>
{
  private readonly IWatchlistRepository _watchlists;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICurrentUser _currentUser;

  public CreateWatchlistHandler(
    IWatchlistRepository watchlists,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
  {
    _watchlists = watchlists;
    _unitOfWork = unitOfWork;
    _currentUser = currentUser;
  }

  public async Task<CreateWatchlistResponse> HandleAsync(CreateWatchlistCommand command, CancellationToken cancellationToken)
  {
    var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Not authenticated");
    var watchlist = Watchlist.Create(userId, command.Name);

    await _watchlists.AddAsync(watchlist, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return new CreateWatchlistResponse(watchlist.Id, watchlist.Name);
  }

}