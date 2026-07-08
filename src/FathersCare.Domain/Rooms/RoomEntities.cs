using FathersCare.Domain.Common;

namespace FathersCare.Domain.Rooms;

public sealed class Floor : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
}

public sealed class Room : TenantEntity
{
    public Guid FloorId { get; set; }
    public Floor? Floor { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public sealed class RoomOccupancy : TenantEntity
{
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public Guid ResidentId { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
}

public enum RoomMaintenancePriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum RoomMaintenanceStatus
{
    Open = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class RoomMaintenanceRequest : TenantEntity
{
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public Guid? ResidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoomMaintenancePriority Priority { get; set; } = RoomMaintenancePriority.Normal;
    public RoomMaintenanceStatus Status { get; set; } = RoomMaintenanceStatus.Open;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTimeOffset ReportedAt { get; set; }
    public string? AssignedTo { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletionNotes { get; set; }
}
