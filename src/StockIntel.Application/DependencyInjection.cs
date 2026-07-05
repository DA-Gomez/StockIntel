using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Application.Common;
using StockIntel.Application.Users.Register;
using StockIntel.Application.Users.Login;
using StockIntel.Application.Watchlists.AddTicker;
using StockIntel.Application.Watchlists.Create;
using StockIntel.Application.Watchlists.List;
using StockIntel.Application.Filings.IngestTickerFilings;

namespace StockIntel.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

    services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserHandler>();
    
    services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginHandler>();
    
    services.AddScoped<ICommandHandler<CreateWatchlistCommand, CreateWatchlistResponse>, CreateWatchlistHandler>();
    services.AddScoped<IQueryHandler<ListWatchlistsQuery, ListWatchlistsResponse>, ListWatchlistHandler>();

    services.AddScoped<ICommandHandler<AddTickerCommand, Unit>, AddTickerHandler>();

    services.AddScoped<ICommandHandler<IngestTickerFilingsCommand, int>, IngestTickerFilingsHandler>();

    return services;
  }
}