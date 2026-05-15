using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Entities;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class WatchlistConfiguration : IEntityTypeConfiguration<Watchlist>
{
  public void Configure(EntityTypeBuilder<Watchlist> builder)
  {
    builder.ToTable("watchlists");

    builder.HasKey(w => w.Id);
    
    builder.Property(w => w.UserId).IsRequired();
    builder.HasIndex(w => w.UserId);

    builder.Property(w => w.Name).IsRequired().HasMaxLength(100);

    //map the private _items field to the Items navigation
    builder.HasMany(w => w.Items).WithOne().HasForeignKey(i => i.WatchlistId).OnDelete(DeleteBehavior.Cascade);

    //tell EF to use the backing field instead of the property
    builder.Metadata.FindNavigation(nameof(Watchlist.Items))!.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}