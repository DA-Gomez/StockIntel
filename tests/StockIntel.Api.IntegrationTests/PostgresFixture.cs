using Microsoft.EntityFrameworkCore;
using StockIntel.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace StockIntel.Api.IntegrationTests;

public class PostgresFixture : IAsyncLifetime//xUnit's interface for async setup/teardown
{
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("stockintel_test")
    .WithUsername("test")
    .WithPassword("test")
    .Build();

  public string ConnectionString => _container.GetConnectionString();

  public async Task InitializeAsync()
  {
    await _container.StartAsync();

    //apply migrations to the test db
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseNpgsql(ConnectionString)
      .UseSnakeCaseNamingConvention()
      .Options;

    using var db = new AppDbContext(options);
    await db.Database.MigrateAsync();

    // var tables = await db.Database
    //     .SqlQueryRaw<string>("SELECT tablename FROM pg_tables WHERE schemaname = 'public'")
    //     .ToListAsync();

    // throw new Exception("TABLES FOUND: " + string.Join(", ", tables));
  }

  public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }