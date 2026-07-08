using FathersCare.Domain.Residents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentMedicalConditionConfiguration : IEntityTypeConfiguration<ResidentMedicalCondition>
{
    public void Configure(EntityTypeBuilder<ResidentMedicalCondition> builder)
    {
        builder.Property(condition => condition.ConditionName).HasMaxLength(150).IsRequired();
        builder.Property(condition => condition.Notes).HasMaxLength(500);

        builder.HasOne(condition => condition.ResidentMedicalProfile)
            .WithMany(profile => profile.Conditions)
            .HasForeignKey(condition => condition.ResidentMedicalProfileId);
    }
}
