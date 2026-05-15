using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Entities;
using StockIntel.Domain.ValueObjects;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
  public void Configure(EntityTypeBuilder<WatchlistItem> builder)
  {
    builder.ToTable("watchlist_items");

    builder.HasKey(i => i.Id);
    
    builder.HasIndex(i => i.WatchlistId);

    // convert the Ticker value object to a string column
    builder.Property(i => i.Ticker)
      .IsRequired()
      .HasMaxLength(5)
      .HasConversion(
        ticker => ticker.Symbol,
        symbol => Ticker.Create(symbol)
      );

    builder.Property(i => i.AddedAt).IsRequired();

    builder.HasIndex(i => new { i.WatchlistId, i.Ticker }).IsUnique();
  }
}

/*
The HasConversion call is the bridge for our Ticker value object. 
EF stores it as a string column in the database but loads it back as a Ticker object.
The first lambda converts to the database type; the second converts back. 
This pattern is how value objects integrate with EF Core
*/