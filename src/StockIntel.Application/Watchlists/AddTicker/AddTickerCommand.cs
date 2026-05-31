using StockIntel.Application.Common;

namespace StockIntel.Application.Watchlists.AddTicker;

public record AddTickerCommand(Guid WatchlistId, string Symbol) : ICommand;