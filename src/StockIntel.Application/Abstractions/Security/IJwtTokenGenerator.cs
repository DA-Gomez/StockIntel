using StockIntel.Domain.Entities;

namespace StockIntel.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
  string GerateToken(User user);
}