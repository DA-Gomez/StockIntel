using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class InsiderTransactionsConfiguration : IEntityTypeConfiguration<InsiderTransaction>
{
  public void Configure(EntityTypeBuilder<InsiderTransaction> builder)
  {
    builder.ToTable("insider_transactions");

    builder.HasKey(t => t.Id);
    builder.Property(t => t.Id).ValueGeneratedNever();
    
    builder.Property(t => t.Code).IsRequired().HasMaxLength(4);

    //numeric(18, 4). never float
    builder.Property(t => t.Shares).HasPrecision(18, 4);
    builder.Property(t => t.PricePerShare).HasPrecision(18, 4);
    builder.Property(t => t.SharesOwnedAfter).HasPrecision(18, 4);

    builder.HasIndex(t => t.TransactionDate);// the feed sorts/filters on this
  }
}