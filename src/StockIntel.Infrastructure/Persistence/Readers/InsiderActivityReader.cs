using Microsoft.EntityFrameworkCore;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Filings.InsiderActivity;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Readers;

public class InsiderActivityReader : IInsiderActivityReader
{
  private readonly AppDbContext _db;

  public InsiderActivityReader(AppDbContext db) => _db = db;

  public async Task<IReadOnlyList<InsiderActivityItem>> GetActivityAsync(
    IReadOnlyCollection<string> symbols,
    DateOnly? afterTransactionDate,
    Guid? afterTransactionId,
    int limit,
    CancellationToken cancellationToken)
  {
    var tickers = symbols.Select(Ticker.Create).ToList();

    //phase 1: symbols -> companies
    //distinct ids prevent duplicate feed items when two watched symbols
    //map to one company (GOOG/GOOGL)
    var listings = await _db.Set<CompanyTicker>()
      .AsNoTracking()
      .Where(x => tickers.Contains(x.Ticker))
      .Select(x => new { x.CompanyId, x.Ticker })
      .ToListAsync(cancellationToken);

    if (listings.Count == 0) return Array.Empty<InsiderActivityItem>();

    var companyIds = listings.Select(x => x.CompanyId).Distinct().ToList();
    var symbolByCompany = listings
      .GroupBy(x => x.CompanyId)
      .ToDictionary(g => g.Key, g => g.First().Ticker.Symbol);

    //phase 2: the feed (a projection not an aggregate load)
    var query = 
      from t in _db.Set<InsiderTransaction>().AsNoTracking()
      join f in _db.InsiderFilings.AsNoTracking() on t.FilingId equals f.Id
      join c in _db.Companies.AsNoTracking() on f.CompanyId equals c.Id
      where companyIds.Contains(f.CompanyId)
      select new
      {
        t.TransactionDate, t.Id, t.Code, t.IsAcquisition,
        t.Shares, t.PricePerShare, t.SharesOwnedAfter,
        f.CompanyId, f.InsiderName, f.OfficerTitle,
        f.IsDirector, f.IsOfficer, f.IsTenPercentOwner,
        f.FilingDate, f.AccessionNumber,
        CompanyName = c.Name
      };

    if (afterTransactionDate is not null && afterTransactionId is not null)
    {
      var date = afterTransactionDate.Value;
      var id = afterTransactionId.Value;
      //keyset seek: after the cursor row in (date DESC, id DESC)
      query = query.Where(r =>
        r.TransactionDate < date
        || (r.TransactionDate == date && r.Id.CompareTo(id) < 0));//(transaction_date, id) < (@date, @id).
    }

    var rows = await query
      .OrderByDescending(r => r.TransactionDate)
      .ThenByDescending(r => r.Id)
      .Take(limit)
      .ToListAsync(cancellationToken);
    
    return rows.Select(r => new InsiderActivityItem(
      r.Id,
      symbolByCompany[r.CompanyId],
      r.CompanyName,
      r.InsiderName,
      r.OfficerTitle,
      r.IsDirector,
      r.IsOfficer,
      r.IsTenPercentOwner,
      r.TransactionDate,
      r.Code,
      ActivityType: TransactionCodes.IsOpenMarketPurchase(r.Code, r.IsAcquisition) ? "Purchase"
          : TransactionCodes.IsOpenMarketSale(r.Code, r.IsAcquisition) ? "Sale"
          : "Other",
      r.Shares,
      r.PricePerShare,
      TotalValue: r.PricePerShare is null ? null : r.Shares * r.PricePerShare,
      r.SharesOwnedAfter,
      r.FilingDate,
      r.AccessionNumber)).ToList();
  }
}

/*
why two phases. Resolving symbols -> companies separately 
keeps the join count down in the hot query
handles multi-symbol companies at the id level
hands us the symbol-stamping dictionary for free
*/
