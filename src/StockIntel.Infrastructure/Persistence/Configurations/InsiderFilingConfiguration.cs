using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockIntel.Domain.Common;
using StockIntel.Domain.Filings;

namespace StockIntel.Infrastructure.Persistence.Configurations;

public class InsiderFilingConfiguration : IEntityTypeConfiguration<InsiderFiling>
{
  public void Configure(EntityTypeBuilder<InsiderFiling> builder)
  {
    builder.HasKey(f => f.Id);
    builder.Property(f => f.Id).ValueGeneratedNever();
    
    builder.Property(f => f.AccessionNumber).IsRequired().HasMaxLength(20);
    builder.HasIndex(f => f.AccessionNumber).IsUnique(); //idempotency guarantee

    builder.Property(f => f.InsiderCik).IsRequired().HasMaxLength(10);
    builder.Property(f => f.InsiderName).IsRequired().HasMaxLength(200);
    builder.Property(f => f.OfficerTitle).HasMaxLength(20);

    //no Filings collection on company. filings are queried 
    builder.HasOne<Company>().WithMany().HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(f => f.Transactions).WithOne().HasForeignKey(t => t.FilingId).OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(f => f.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(f => new { f.CompanyId, f.FilingDate });
  }
}