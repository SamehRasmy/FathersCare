using FathersCare.Domain.Residents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentMedicalProfileConfiguration : IEntityTypeConfiguration<ResidentMedicalProfile>
{
    public void Configure(EntityTypeBuilder<ResidentMedicalProfile> builder)
    {
        builder.Property(profile => profile.BloodType).HasMaxLength(20);
        builder.Property(profile => profile.AllergiesSummary).HasMaxLength(500);
        builder.Property(profile => profile.ChronicConditionsSummary).HasMaxLength(500);
        builder.Property(profile => profile.AllergyDetails).HasMaxLength(500);
        builder.Property(profile => profile.PreviousSurgeries).HasMaxLength(1000);
        builder.Property(profile => profile.PreviousInjuriesOrAccidents).HasMaxLength(1000);
        builder.Property(profile => profile.MedicalDeclarationConfirmedBy).HasMaxLength(200);
        builder.Property(profile => profile.Notes).HasMaxLength(1000);

        builder.HasOne(profile => profile.Resident)
            .WithOne(resident => resident.MedicalProfile)
            .HasForeignKey<ResidentMedicalProfile>(profile => profile.ResidentId);
    }
}
