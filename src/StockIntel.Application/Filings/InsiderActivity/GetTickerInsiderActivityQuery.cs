using StockIntel.Application.Common;

namespace StockIntel.Application.Filings.InsiderActivity;

public record GetTickerInsiderActivityQuery(string Symbol, string? Cursor, int? PageSize)
  : IQuery<InsiderActivityPage>;