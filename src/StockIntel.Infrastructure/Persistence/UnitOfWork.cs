using StockIntel.Application.Abstractions.Persistence;

namespace StockIntel.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
  private readonly AppDbContext _db;

  public UnitOfWork(AppDbContext db)
  {
    _db = db;
  }
  
  //saves all entity modifications to db in one transaction
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return _db.SaveChangesAsync(cancellationToken);
  }
}