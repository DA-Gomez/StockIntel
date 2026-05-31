namespace StockIntel.Application.Abstractions.Authentication;

public interface ICurrentUser
{
  Guid? UserId { get; } //nullable
  bool IsAuthenticated { get; }
}