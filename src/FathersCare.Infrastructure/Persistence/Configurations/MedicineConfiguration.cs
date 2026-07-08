using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.Property(medicine => medicine.Name).HasMaxLength(160).IsRequired();
        builder.Property(medicine => medicine.Strength).HasMaxLength(80);
        builder.Property(medicine => medicine.Form).HasMaxLength(80);
        builder.HasIndex(medicine => new { medicine.TenantId, medicine.Name, medicine.Strength });
    }
}
