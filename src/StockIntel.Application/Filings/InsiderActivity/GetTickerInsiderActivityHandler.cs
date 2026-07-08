using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;
using StockIntel.Domain.Common;

namespace StockIntel.Application.Filings.InsiderActivity;

public class GetTickerInsiderActivityHandler : IQueryHandler<GetTickerInsiderActivityQuery, InsiderActivityPage>
{
  private readonly IInsiderActivityReader _reader;

  public GetTickerInsiderActivityHandler(IInsiderActivityReader reader) => _reader = reader;

  public Task<InsiderActivityPage> HandleAsync(GetTickerInsiderActivityQuery query, CancellationToken cancellationToken)
  {
    var ticker = Ticker.Create(query.Symbol);

    return InsiderActivityPages.LoadAsync(
      _reader, new[] { ticker.Symbol }, query.Cursor, query.PageSize, cancellationToken
    );
  }
}