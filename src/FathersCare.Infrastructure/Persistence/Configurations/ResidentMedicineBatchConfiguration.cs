using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentMedicineBatchConfiguration : IEntityTypeConfiguration<ResidentMedicineBatch>
{
    public void Configure(EntityTypeBuilder<ResidentMedicineBatch> builder)
    {
        builder.Property(batch => batch.ReceivedFrom).HasMaxLength(200).IsRequired();
        builder.Property(batch => batch.ReceivedBy).HasMaxLength(200).IsRequired();
        builder.Property(batch => batch.PrescriptionReference).HasMaxLength(160);
        builder.Property(batch => batch.Notes).HasMaxLength(500);
        builder.HasIndex(batch => new { batch.TenantId, batch.ResidentId, batch.ResidentMedicineId, batch.ExpiresOn });
    }
}
