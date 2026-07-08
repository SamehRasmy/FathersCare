using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class DoseAdministrationConfiguration : IEntityTypeConfiguration<DoseAdministration>
{
    public void Configure(EntityTypeBuilder<DoseAdministration> builder)
    {
        builder.HasIndex(d => new { d.TenantId, d.MedicineScheduleId, d.DoseDate, d.Status });
    }
}