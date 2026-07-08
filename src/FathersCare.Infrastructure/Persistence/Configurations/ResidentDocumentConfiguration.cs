using FathersCare.Domain.Residents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class ResidentDocumentConfiguration : IEntityTypeConfiguration<ResidentDocument>
{
    public void Configure(EntityTypeBuilder<ResidentDocument> builder)
    {
        builder.Property(document => document.Title).HasMaxLength(200).IsRequired();
        builder.Property(document => document.FileName).HasMaxLength(260).IsRequired();
        builder.Property(document => document.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(document => document.UploadedBy).HasMaxLength(128);
        builder.Property(document => document.Notes).HasMaxLength(500);

        builder.HasOne(document => document.Resident)
            .WithMany(resident => resident.Documents)
            .HasForeignKey(document => document.ResidentId);
    }
}
