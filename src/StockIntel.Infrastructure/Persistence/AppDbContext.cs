using Microsoft.EntityFrameworkCore;
using StockIntel.Domain.Users;
using StockIntel.Domain.Users.Watchlists;

namespace StockIntel.Infrastructure.Persistence;

public class AppDbContext : DbContext //main class that talks to the database.
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<User> Users => Set<User>();
  public DbSet<Watchlist> Watchlists => Set<Watchlist>();
  public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    //tells EF to look for any classes in this assembly that implement IEntityTypeConfiguration<T> and apply them
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); 
    base.OnModelCreating(modelBuilder);
  }
}