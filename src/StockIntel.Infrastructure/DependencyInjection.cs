using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Infrastructure.Persistence;
using StockIntel.Infrastructure.Persistence.Repositories;
using StockIntel.Infrastructure.Security;
using StockIntel.Application.Abstractions.Persistence;
using StockIntel.Application.Abstractions.Security;

namespace StockIntel.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration configuration
  )
  {
    var connectionString = configuration.GetConnectionString("Postgres")
      ?? throw new InvalidOperationException("Connection string 'Postgres' not configured");

    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

    services.AddScoped<IWatchlistRepository, WatchlistRepository>();

    return services;
  }
}

/*
  lifetimes: 
  Scoped - one instance per HTTP request
  Singleton - one instance for the entire app lifetime
  Transient - a new instance every time
*/