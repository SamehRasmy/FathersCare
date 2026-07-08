using FathersCare.Domain.Residents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.Property(resident => resident.Code).HasMaxLength(32).IsRequired();
        builder.Property(resident => resident.FullName).HasMaxLength(200).IsRequired();
        builder.Property(resident => resident.FullNameArabic).HasMaxLength(200);
        builder.Property(resident => resident.FullNameEnglish).HasMaxLength(200);
        builder.Property(resident => resident.BirthPlace).HasMaxLength(150);
        builder.Property(resident => resident.Religion).HasMaxLength(100);
        builder.Property(resident => resident.Denomination).HasMaxLength(100);
        builder.Property(resident => resident.Nationality).HasMaxLength(100);
        builder.Property(resident => resident.NationalId).HasMaxLength(50);
        builder.Property(resident => resident.PassportNumber).HasMaxLength(50);
        builder.Property(resident => resident.IdOrPassportIssueAuthority).HasMaxLength(150);
        builder.Property(resident => resident.EducationLevel).HasMaxLength(100);
        builder.Property(resident => resident.PreviousJob).HasMaxLength(150);
        builder.Property(resident => resident.CurrentAddress).HasMaxLength(250);
        builder.Property(resident => resident.PhoneNumber).HasMaxLength(32);
        builder.Property(resident => resident.MobilePhone).HasMaxLength(32);
        builder.Property(resident => resident.MobileNumber).HasMaxLength(32);
        builder.Property(resident => resident.PhotoPath).HasMaxLength(500);
        builder.Property(resident => resident.RoomGrade).HasMaxLength(100);
        builder.Property(resident => resident.TreatingDoctorName).HasMaxLength(200);
        builder.Property(resident => resident.CompanionName).HasMaxLength(200);
        builder.Property(resident => resident.ResponsiblePersonName).HasMaxLength(200);
        builder.Property(resident => resident.ResponsiblePersonRelationship).HasMaxLength(100);
        builder.Property(resident => resident.ResponsiblePersonAddress).HasMaxLength(250);
        builder.Property(resident => resident.ResponsiblePersonPhone).HasMaxLength(32);
        builder.Property(resident => resident.ResponsiblePersonMobile).HasMaxLength(32);
        builder.Property(resident => resident.ResponsiblePersonWorkAddress).HasMaxLength(250);
        builder.Property(resident => resident.SecondResponsiblePersonName).HasMaxLength(200);
        builder.Property(resident => resident.SecondResponsiblePersonRelationship).HasMaxLength(100);
        builder.Property(resident => resident.SecondResponsiblePersonAddress).HasMaxLength(250);
        builder.Property(resident => resident.SecondResponsiblePersonPhone).HasMaxLength(32);
        builder.Property(resident => resident.SecondResponsiblePersonMobile).HasMaxLength(32);
        builder.Property(resident => resident.SecondResponsiblePersonWorkAddress).HasMaxLength(250);
        builder.Property(resident => resident.AdditionalInformation).HasMaxLength(1000);
        builder.Property(resident => resident.Notes).HasMaxLength(1000);
        builder.HasIndex(resident => new { resident.TenantId, resident.Code }).IsUnique();
    }
}
