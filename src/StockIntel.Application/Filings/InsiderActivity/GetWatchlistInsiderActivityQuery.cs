using StockIntel.Application.Abstractions.Authentication;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;

namespace StockIntel.Application.Filings.InsiderActivity;

public record GetWatchlistInsiderActivityQuery(Guid WatchlistId, string? Cursor, int? PageSize)
  : IQuery<InsiderActivityPage>;

public record GetWatchlistInsiderActivityHandler : IQueryHandler<GetWatchlistInsiderActivityQuery, InsiderActivityPage>
{
  private readonly IWatchlistRepository _watchlists;
  private readonly IInsiderActivityReader _reader;
  private readonly ICurrentUser _currentUser;

  public GetWatchlistInsiderActivityHandler(
    IWatchlistRepository watchlists,
    IInsiderActivityReader reader,
    ICurrentUser currentUser)
  {
    _watchlists = watchlists;
    _reader = reader;
    _currentUser = currentUser;
  }

  public async Task<InsiderActivityPage> HandleAsync(
    GetWatchlistInsiderActivityQuery query, CancellationToken cancellationToken)
  {
    var userId = _currentUser.UserId
      ?? throw new UnauthorizedAccessException("Not authenticated");

    var watchlist = await _watchlists.GetByIdAsync(query.WatchlistId, cancellationToken)
      ?? throw new KeyNotFoundException("Watchlist not found");

    if (watchlist.UserId != userId)
      throw new UnauthorizedAccessException("You do not own this watchlist");

    var symbols = watchlist.Items.Select(i => i.Ticker.Symbol).Distinct().ToList();
    if (symbols.Count == 0)
      return new InsiderActivityPage(Array.Empty<InsiderActivityItem>(), null);
    
    return await InsiderActivityPages.LoadAsync(_reader, symbols, query.Cursor, query.PageSize, cancellationToken);
  }
}