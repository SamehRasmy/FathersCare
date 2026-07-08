using FathersCare.Domain.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class MedicineShelfConfiguration : IEntityTypeConfiguration<MedicineShelf>
{
    public void Configure(EntityTypeBuilder<MedicineShelf> builder)
    {
        builder.HasIndex(s => new { s.TenantId, s.MedicineId }).IsUnique();
    }
}