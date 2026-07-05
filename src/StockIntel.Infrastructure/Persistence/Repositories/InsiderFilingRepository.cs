using Microsoft.EntityFrameworkCore;
using Npgsql;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Domain.Filings;
using StockIntel.Infrastructure.Persistence;

namespace StockIntelk.Infrastructure.Persistence.Repositories;

public class InsiderFilingRepository : IInsiderFilingRepository
{
  private readonly AppDbContext _db;

  public InsiderFilingRepository(AppDbContext db) => _db = db;

  public Task<bool> ExistsAsync(string accessionNumber, CancellationToken cancellationToken)
  {
    return _db.InsiderFilings.AnyAsync(f => f.AccessionNumber == accessionNumber, cancellationToken);
  }

  public async Task<bool> TryAddAsync(InsiderFiling filing, CancellationToken cancellationToken)
  {
    _db.InsiderFilings.Add(filing);
    try
    {
      await _db.SaveChangesAsync(cancellationToken);
      return true;
    }
    catch (DbUpdateException ex) when (//when clause is evaluated before the catch commits (if it doesnt match the exception continues up untouched)
      ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
    {
      // Someone else inserted this accession number between the check and the save (TOCTOU), 
      // resolved by the unique index either way
      // Detach the failed graph so  DbContext stays usable for the rest of the  run
      foreach (var entry in _db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
      {
        entry.State = EntityState.Detached;
      }
      return false;
    }
  }
}