using Microsoft.EntityFrameworkCore;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
  private readonly AppDbContext _db;

  public CompanyRepository(AppDbContext db) => _db = db;

  public Task<Company?> GetByCikAsync(string cik, CancellationToken cancellationToken)
  {
    //unclude tickers (same reason Watchlists loads it items)
    return _db.Companies
      .Include(c => c.Tickers)
      .FirstOrDefaultAsync(c => c.Cik == cik, cancellationToken);
  }

  public async Task AddAsync(Company company, CancellationToken cancellationToken)
  {
    await _db.Companies.AddAsync(company, cancellationToken);
  }
}