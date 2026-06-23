using StockIntel.Domain.Users;

namespace StockIntel.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
  string GerateToken(User user);
}