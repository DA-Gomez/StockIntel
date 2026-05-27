using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Application.Common;
using StockIntel.Application.Users.Register;

namespace StockIntel.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

    services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserHandler>();

    return services;
  }
}