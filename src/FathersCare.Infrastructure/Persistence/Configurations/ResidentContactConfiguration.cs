using FathersCare.Domain.Residents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentContactConfiguration : IEntityTypeConfiguration<ResidentContact>
{
    public void Configure(EntityTypeBuilder<ResidentContact> builder)
    {
        builder.Property(contact => contact.FullName).HasMaxLength(200).IsRequired();
        builder.Property(contact => contact.Relationship).HasMaxLength(100).IsRequired();
        builder.Property(contact => contact.Job).HasMaxLength(150);
        builder.Property(contact => contact.Address).HasMaxLength(250);
        builder.Property(contact => contact.PhoneNumber).HasMaxLength(32);
        builder.Property(contact => contact.MobileNumber).HasMaxLength(32);
        builder.Property(contact => contact.Notes).HasMaxLength(500);

        builder.HasOne(contact => contact.Resident)
            .WithMany(resident => resident.Contacts)
            .HasForeignKey(contact => contact.ResidentId);
    }
}
