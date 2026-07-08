using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class MedicineScheduleConfiguration : IEntityTypeConfiguration<MedicineSchedule>
{
    public void Configure(EntityTypeBuilder<MedicineSchedule> builder)
    {
        builder.HasIndex(s => new { s.TenantId, s.ResidentMedicineId, s.DoseTime });
    }
}