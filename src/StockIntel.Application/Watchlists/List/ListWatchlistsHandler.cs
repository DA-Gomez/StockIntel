using StockIntel.Application.Abstractions.Authentication;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;

namespace StockIntel.Application.Watchlists.List;

public class ListWatchlistHandler : IQueryHandler<ListWatchlistsQuery, ListWatchlistsResponse>
{
  private readonly IWatchlistRepository _watchlists;
  private readonly ICurrentUser _currentUser;

  public ListWatchlistHandler(IWatchlistRepository watchlists, ICurrentUser currentUser)
  {
    _watchlists = watchlists;
    _currentUser = currentUser;
  }

  public async Task<ListWatchlistsResponse> HandleAsync(ListWatchlistsQuery query, CancellationToken cancellationToken)
  {
    var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Not authenticated");
    var watchlists = await _watchlists.GetByUserIdAsync(userId, cancellationToken);

    var dtos = watchlists.Select(w => new WatchlistDto(
      w.Id,
      w.Name,
      w.CreatedAt,
      w.Items.Select(i => new TickerDto(i.Ticker.Symbol, i.AddedAt)).ToList()
    )).ToList();

    return new ListWatchlistsResponse(dtos);
  }
}