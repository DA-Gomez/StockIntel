using StockIntel.Domain.Users;
namespace StockIntel.Application.Abstractions.Persistence;

public interface IUserRepository
{
  Task<User?> GetByEmailAsync(string name, CancellationToken cancellationToken);
  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task AddAsync(User user, CancellationToken cancellationToken);
  Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
}