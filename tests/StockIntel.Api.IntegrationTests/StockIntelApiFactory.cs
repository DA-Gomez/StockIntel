using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.VisualStudio.TestPlatform.TestHost;
using StockIntel.Infrastructure.Persistence;

namespace StockIntel.Api.IntegrationTests;

public class StockIntelApiFactory : WebApplicationFactory<Program>//needs Program accessible in Program.cs at Stockintel.API
{
  private readonly string _connectionString;

  public StockIntelApiFactory(string connectionString)
  {
    _connectionString = connectionString;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      //remove existing DbContext and replace with the test conn string
      var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
      if (descriptor is not null) services.Remove(descriptor);

      services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_connectionString).UseSnakeCaseNamingConvention());
    });

    builder.UseEnvironment("Development");
  }
}