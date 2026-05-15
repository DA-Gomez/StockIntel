using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Infrastructure.Persistence;

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

    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

    return services;
  }
}