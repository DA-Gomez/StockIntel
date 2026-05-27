using StockIntel.Application.Common;

namespace StockIntel.Application.Users.Register;

public record RegisterUserCommand(string Email, string Password) : ICommand<RegisterUserResponse>;

public record RegisterUserResponse(Guid UserId, string Email);