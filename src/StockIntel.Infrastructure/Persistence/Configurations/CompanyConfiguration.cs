using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
  public void Configure(EntityTypeBuilder<Company> builder)
  {
    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id).ValueGeneratedNever();

    builder.Property(c => c.Cik).IsRequired().HasMaxLength(10);
    builder.HasIndex(c => c.Name).IsUnique();
    
    builder.Property(c => c.Name).IsRequired().HasMaxLength(300);

    builder.HasMany(c => c.Tickers).WithOne().HasForeignKey(t => t.CompanyId).OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(c => c.Tickers).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}