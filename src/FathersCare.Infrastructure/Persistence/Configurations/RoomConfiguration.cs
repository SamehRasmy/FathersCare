using FathersCare.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.Property(floor => floor.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(floor => new { floor.TenantId, floor.Number }).IsUnique();
    }
}

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.Property(room => room.Number).HasMaxLength(40).IsRequired();
        builder.HasOne(room => room.Floor).WithMany().HasForeignKey(room => room.FloorId);
        builder.HasIndex(room => new { room.TenantId, room.FloorId, room.Number }).IsUnique();
    }
}

public sealed class RoomOccupancyConfiguration : IEntityTypeConfiguration<RoomOccupancy>
{
    public void Configure(EntityTypeBuilder<RoomOccupancy> builder)
    {
        builder.HasOne(occupancy => occupancy.Room).WithMany().HasForeignKey(occupancy => occupancy.RoomId);
        builder.HasIndex(occupancy => new { occupancy.TenantId, occupancy.RoomId, occupancy.ResidentId, occupancy.EndsOn });
    }
}

public sealed class RoomMaintenanceRequestConfiguration : IEntityTypeConfiguration<RoomMaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<RoomMaintenanceRequest> builder)
    {
        builder.Property(request => request.Title).HasMaxLength(160).IsRequired();
        builder.Property(request => request.Description).HasMaxLength(1000);
        builder.Property(request => request.ReportedBy).HasMaxLength(160).IsRequired();
        builder.Property(request => request.AssignedTo).HasMaxLength(160);
        builder.Property(request => request.CompletionNotes).HasMaxLength(1000);
        builder.HasOne(request => request.Room).WithMany().HasForeignKey(request => request.RoomId);
        builder.HasIndex(request => new { request.TenantId, request.RoomId, request.Status, request.Priority });
    }
}
