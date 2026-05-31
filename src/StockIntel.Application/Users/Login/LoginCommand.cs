using StockIntel.Application.Common;

namespace StockIntel.Application.Users.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public record LoginResponse(string AccessToken, Guid UserId, string Email);