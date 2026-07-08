namespace StockIntel.Application.Filings.InsiderActivity;

//One page of the feed. NextCursor is null on the last page
public record InsiderActivityPage(IReadOnlyList<InsiderActivityItem> Items, string? NextCursor); 