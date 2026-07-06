using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class CompanyTickerConfiguration : IEntityTypeConfiguration<CompanyTicker>
{
  public void Configure(EntityTypeBuilder<CompanyTicker> builder)
  {
    builder.ToTable("company_tickers");

    builder.HasKey(t => t.Id);
    builder.Property(t => t.Id).ValueGeneratedNever();

    builder.Property(t => t.Ticker).IsRequired().HasMaxLength(5).HasConversion(t => t.Symbol, s => Ticker.Create(s));

    builder.HasIndex(t => t.Ticker).IsUnique(); //one ticker to one company at a time
  }
}