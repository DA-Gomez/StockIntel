namespace StockIntel.Application.Common;

public interface IQuery<TResponse> { }

public interface IQueryHandler<in TQuery, TReponse> where TQuery : IQuery<TReponse>
{
  Task<TReponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}