using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentMedicineConfiguration : IEntityTypeConfiguration<ResidentMedicine>
{
    public void Configure(EntityTypeBuilder<ResidentMedicine> builder)
    {
        builder.Property(rm => rm.Instructions).HasMaxLength(500);
        builder.Property(rm => rm.PrescribedBy).HasMaxLength(160);
        builder.Property(rm => rm.PrescriptionReference).HasMaxLength(160);
        builder.HasIndex(rm => new { rm.TenantId, rm.ResidentId, rm.MedicineId, rm.IsActive });
    }
}
