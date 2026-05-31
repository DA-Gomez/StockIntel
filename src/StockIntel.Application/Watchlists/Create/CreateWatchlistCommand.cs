
using StockIntel.Application.Common;

namespace StockIntel.Application.Watchlists.Create;

public record CreateWatchlistCommand(string Name) : ICommand<CreateWatchlistResponse>;
public record CreateWatchlistResponse(Guid WatchlistId, string Name);