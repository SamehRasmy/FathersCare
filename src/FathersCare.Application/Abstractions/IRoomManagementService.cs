namespace FathersCare.Application.Abstractions;

public interface IRoomManagementService
{
    Task<RoomsWorkspaceViewModel> GetWorkspaceAsync(Guid? floorId = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateFloorAsync(RoomFloorCreateDto dto, CancellationToken cancellationToken = default);
    Task<Guid> CreateRoomAsync(RoomCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateRoomCapacityAsync(RoomCapacityUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task AssignResidentToRoomAsync(RoomAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<Guid> CreateMaintenanceRequestAsync(RoomMaintenanceCreateDto dto, CancellationToken cancellationToken = default);
    Task CompleteMaintenanceRequestAsync(Guid requestId, string completedBy, string? notes = null, CancellationToken cancellationToken = default);
}

public sealed record RoomFloorOptionDto(Guid Id, string Name, int Number);
public sealed record RoomOptionDto(Guid Id, string Label, Guid FloorId);
public sealed record RoomResidentOptionDto(Guid Id, string Name, Guid? RoomId);

public sealed record RoomResidentDto(Guid Id, string Name, string Code, string MobilePhone, string PhotoPath);

public sealed record RoomCardDto(
    Guid Id,
    string Number,
    string Floor,
    int Capacity,
    int Occupied,
    int AvailableBeds,
    string OccupancyText,
    string State,
    string MaintenanceState,
    int DelayedMedicationCount,
    IReadOnlyList<RoomResidentDto> Residents);

public sealed record FloorSectionDto(Guid Id, string Name, int Number, int Capacity, int Occupied, IReadOnlyList<RoomCardDto> Rooms);

public sealed record RoomMaintenanceRowDto(
    Guid Id,
    Guid RoomId,
    string Room,
    string Floor,
    string Resident,
    string Title,
    string Description,
    string Priority,
    string PriorityState,
    string Status,
    string StatusState,
    string ReportedBy,
    string ReportedAt,
    string AssignedTo);

public sealed record RoomFloorCreateDto(string Name, int Number);
public sealed record RoomCreateDto(Guid FloorId, string Number, int Capacity);
public sealed record RoomCapacityUpdateDto(Guid RoomId, int Capacity);
public sealed record RoomAssignmentDto(Guid ResidentId, Guid RoomId);

public sealed record RoomMaintenanceCreateDto(
    Guid RoomId,
    Guid? ResidentId,
    string Title,
    string Description,
    string Priority,
    string ReportedBy,
    string AssignedTo);

public sealed class RoomsWorkspaceViewModel
{
    public IReadOnlyList<RoomFloorOptionDto> Floors { get; init; } = [];
    public IReadOnlyList<RoomOptionDto> RoomOptions { get; init; } = [];
    public IReadOnlyList<RoomResidentOptionDto> ResidentOptions { get; init; } = [];
    public IReadOnlyList<FloorSectionDto> FloorSections { get; init; } = [];
    public IReadOnlyList<RoomMaintenanceRowDto> MaintenanceRequests { get; init; } = [];
    public int TotalFloors { get; init; }
    public int TotalRooms { get; init; }
    public int TotalBeds { get; init; }
    public int OccupiedBeds { get; init; }
    public int AvailableBeds { get; init; }
    public int FullRooms { get; init; }
    public int OpenMaintenanceRequests { get; init; }
    public int UrgentMaintenanceRequests { get; init; }
}
