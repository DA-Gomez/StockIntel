using StockIntel.Application.Abstractions.Security;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Common;

namespace StockIntel.Application.Users.Login;

public class LoginHandler : ICommandHandler<LoginCommand, LoginResponse>
{
  private readonly IUserRepository _users;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IJwtTokenGenerator _tokenGenerator;

  public LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator)
  {
    _users = users;
    _passwordHasher = passwordHasher;
    _tokenGenerator = tokenGenerator;
  }

  public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
  {
    var user = await _users.GetByEmailAsync(command.Email, cancellationToken);


    if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash)) 
      throw new UnauthorizedAccessException("Invalid email or password");

    var token = _tokenGenerator.GerateToken(user);

    return new LoginResponse(token, user.Id, user.Email);
  }
}