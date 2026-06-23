using Microsoft.EntityFrameworkCore;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Domain.Users;

namespace StockIntel.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
  private readonly AppDbContext _db;

  public UserRepository(AppDbContext db)
  {
    _db = db;
  }

  public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
  {
    var normalized = email.Trim().ToLowerInvariant();
    return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
  }

  public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
  }

  //Whatever returns from async methods are wrapped in a Task
  public async Task AddAsync(User user, CancellationToken cancellationToken)
  {
    //AddAsync doesn't actually save to the database, it just stages the addition in EF's change tracker
    await _db.Users.AddAsync(user, cancellationToken);
  }

  public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
  {
    var normalized = email.Trim().ToLowerInvariant();
    return _db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
  }
}