using FluentValidation;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Abstractions.Security;
using StockIntel.Application.Common;
using StockIntel.Domain.Entities;

namespace StockIntel.Application.Users.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
  private readonly IUserRepository _users;
  private readonly IUnitOfWork _UnitOfWork;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IValidator<RegisterUserCommand> _validator;

  public RegisterUserHandler(
    IUserRepository users,
    IUnitOfWork UnitOfWord,
    IPasswordHasher passwordHasher,
    IValidator<RegisterUserCommand> validator)
  {
    _users = users;
    _UnitOfWork = UnitOfWord;
    _passwordHasher = passwordHasher;
    _validator = validator;
  }

  public async Task<RegisterUserResponse> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
  {
    //validate input shape
    var validation = await _validator.ValidateAsync(command, cancellationToken);
    if (!validation.IsValid)
      throw new ValidationException(validation.Errors);

    //busniess rule: email must be unique
    if (await _users.EmailExistsAsync(command.Email, cancellationToken))
      throw new InvalidOperationException("A user with this email already exists.");

    var passwordHash = _passwordHasher.Hash(command.Password);

    var user = User.Register(command.Email, passwordHash);

    await _users.AddAsync(user, cancellationToken);
    await _UnitOfWork.SaveChangesAsync(cancellationToken);

    return new RegisterUserResponse(user.Id, user.Email);
  }
}

//handler
//Domain