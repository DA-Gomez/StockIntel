using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StockIntel.Application.Common;
using StockIntel.Application.Users.Register;
using StockIntel.Application.Users.Login;

namespace StockIntel.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
  private readonly ICommandHandler<RegisterUserCommand, RegisterUserResponse> _registerHandler;
  private readonly ICommandHandler<LoginCommand, LoginResponse> _loginHandler;

  public UsersController(
    ICommandHandler<RegisterUserCommand, RegisterUserResponse> registerHandler,
    ICommandHandler<LoginCommand, LoginResponse> loginHandler)
  {
    _registerHandler = registerHandler;
    _loginHandler = loginHandler;
  }

  [HttpPost("register")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<IActionResult> Register(
    [FromBody] RegisterUserRequest request, //comes as JSON
    CancellationToken cancellationToken)
  {
    try
    {
      var command = new RegisterUserCommand(request.Email, request.Password);
      var result = await _registerHandler.HandleAsync(command, cancellationToken);
      return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }
    catch (ValidationException e)
    {
      return BadRequest(new { errors = e.Errors.Select(e => e.ErrorMessage)});
    }
    catch (InvalidOperationException e)
    {
      return Conflict(new { error = e.Message });
    }
  }

  [HttpPost("login")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
  {
    try
    {
      var command = new LoginCommand(request.Email, request.Password);
      var result = await _loginHandler.HandleAsync(command, cancellationToken);
      return Ok(result);
    }
    catch (UnauthorizedAccessException e)
    {
      return Unauthorized(new { error = e.Message });
    }
  }
}

public record RegisterUserRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);

//Without [FromBody] ASP.NET tries to guess where the data comes from