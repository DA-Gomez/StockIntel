using StockIntel.Application.Abstractions.Persistence;

namespace StockIntel.Application.Filings.InsiderActivity;

internal static class InsiderActivityPages
{
  public const int DefaultPageSize = 20;
  public const int MaxPageSize = 100;

  public static async Task<InsiderActivityPage> LoadAsync(
    IInsiderActivityReader reader,
    IReadOnlyCollection<string> symbols,
    string? cursor,
    int? requestedPageSize,
    CancellationToken cancellationToken)
  {
    var pageSize = Math.Clamp(requestedPageSize ?? DefaultPageSize, 1, MaxPageSize);

    DateOnly? afterDate = null;
    Guid? afterId = null;
    if (!string.IsNullOrWhiteSpace(cursor))
    {
      if (!ActivityCursor.TryDecode(cursor, out var date, out var id)) //'out' means the method might return date or id. Think of out as "fill this variable"
        throw new ArgumentException("Invalid cursor");
      afterDate = date;
      afterId = id;
    }

    //pagesize + 1: the extra row's existanse means theres more
    var rows = await reader.GetActivityAsync(symbols, afterDate, afterId, pageSize + 1, cancellationToken);

    var hasMore = rows.Count > pageSize;
    var items = hasMore ? rows.Take(pageSize).ToList() : (IReadOnlyList<InsiderActivityItem>)rows;
    var NextCursor = hasMore
      ? ActivityCursor.Encode(items[^1].TransactionDate, items[^1].TransactionId) //^ is index-from-end operator ^1 is last
      : null;

    return new InsiderActivityPage(items, NextCursor);
  }
}