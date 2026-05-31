using StockIntel.Application.Common;

namespace StockIntel.Application.Watchlists.List;

public record ListWatchlistsQuery() : IQuery<ListWatchlistsResponse>;

public record ListWatchlistsResponse(IReadOnlyList<WatchlistDto> Watchlists);

public record WatchlistDto(Guid Id, string Name, DateTime CreatedAt, IReadOnlyList<TickerDto> Tickers);

public record TickerDto(string Symbol, DateTime AddedAt);

//dto -> Data Transfer Object: 
// design pattern used to encapsulate and transfer data between different layers or components of an app without exposing business logic or database schemas