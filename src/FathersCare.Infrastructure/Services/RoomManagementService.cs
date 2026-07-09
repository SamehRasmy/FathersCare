using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;
using FathersCare.Domain.Residents;
using FathersCare.Domain.Rooms;
using FathersCare.Domain.Tenancy;
using FathersCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FathersCare.Infrastructure.Services;

public sealed class RoomManagementService(AppDbContext db) : IRoomManagementService
{
    public async Task<RoomsWorkspaceViewModel> GetWorkspaceAsync(Guid? floorId = null, CancellationToken cancellationToken = default)
    {
        var floors = await db.Floors.AsNoTracking()
            .Where(floor => !floor.IsDeleted)
            .OrderBy(floor => floor.Number)
            .ToListAsync(cancellationToken);

        var roomsQuery = db.Rooms.AsNoTracking()
            .Include(room => room.Floor)
            .Where(room => !room.IsDeleted);

        if (floorId.HasValue)
        {
            roomsQuery = roomsQuery.Where(room => room.FloorId == floorId.Value);
        }

        var rooms = await roomsQuery
            .OrderBy(room => room.Floor!.Number)
            .ThenBy(room => room.Number)
            .ToListAsync(cancellationToken);
        var roomIds = rooms.Select(room => room.Id).ToList();
        var roomLookup = rooms.ToDictionary(room => room.Id);
        var floorLookup = floors.ToDictionary(floor => floor.Id);

        var residents = await db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active && resident.CurrentRoomId.HasValue)
            .OrderBy(resident => resident.FullName)
            .Select(resident => new
            {
                resident.Id,
                resident.Code,
                resident.FullName,
                resident.MobilePhone,
                resident.MobileNumber,
                resident.PhotoPath,
                resident.CurrentRoomId
            })
            .ToListAsync(cancellationToken);

        var allActiveResidents = await db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active)
            .OrderBy(resident => resident.FullName)
            .Select(resident => new RoomResidentOptionDto(resident.Id, resident.FullName, resident.CurrentRoomId))
            .ToListAsync(cancellationToken);
        var activeResidentIds = residents.Select(resident => resident.Id).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);
        var delayedMedicationCountByResident = await (
            from dose in db.DoseAdministrations.AsNoTracking()
            join schedule in db.MedicineSchedules.AsNoTracking() on dose.MedicineScheduleId equals schedule.Id
            join residentMedicine in db.ResidentMedicines.AsNoTracking() on schedule.ResidentMedicineId equals residentMedicine.Id
            where activeResidentIds.Contains(residentMedicine.ResidentId)
                && dose.DoseDate == today
                && !dose.IsDeleted
                && !schedule.IsDeleted
                && !residentMedicine.IsDeleted
                && residentMedicine.IsActive
                && dose.Status == DoseAdministrationStatus.Scheduled
                && schedule.DoseTime < nowTime
                && (residentMedicine.StartsOn == null || residentMedicine.StartsOn <= today)
                && (residentMedicine.EndsOn == null || residentMedicine.EndsOn >= today)
            group dose by residentMedicine.ResidentId into residentDoses
            select new
            {
                ResidentId = residentDoses.Key,
                Count = residentDoses.Count()
            })
            .ToDictionaryAsync(item => item.ResidentId, item => item.Count, cancellationToken);

        var maintenanceSource = await db.RoomMaintenanceRequests.AsNoTracking()
            .Where(request => !request.IsDeleted && request.Status != RoomMaintenanceStatus.Completed && request.Status != RoomMaintenanceStatus.Cancelled)
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.ReportedAt)
            .ToListAsync(cancellationToken);
        var maintenanceResidentIds = maintenanceSource
            .Where(request => request.ResidentId.HasValue)
            .Select(request => request.ResidentId!.Value)
            .Distinct()
            .ToList();
        var maintenanceResidents = await db.Residents.AsNoTracking()
            .Where(resident => maintenanceResidentIds.Contains(resident.Id))
            .Select(resident => new { resident.Id, resident.FullName })
            .ToDictionaryAsync(resident => resident.Id, resident => resident.FullName, cancellationToken);
        var maintenanceRows = maintenanceSource
            .Where(request => roomLookup.ContainsKey(request.RoomId))
            .Select(request =>
            {
                var room = roomLookup[request.RoomId];
                var floor = floorLookup.GetValueOrDefault(room.FloorId);
                var residentName = request.ResidentId.HasValue && maintenanceResidents.TryGetValue(request.ResidentId.Value, out var foundName)
                    ? foundName
                    : string.Empty;
                return new RoomMaintenanceProjection(request, room, floor, residentName);
            })
            .ToList();

        var maintenanceByRoom = maintenanceRows
            .GroupBy(row => row.Room.Id)
            .ToDictionary(group => group.Key, group => group.Select(row => row.Request).ToList());

        var residentRowsByRoom = residents
            .Where(resident => resident.CurrentRoomId.HasValue)
            .GroupBy(resident => resident.CurrentRoomId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Select(resident => new RoomResidentDto(
                    resident.Id,
                    resident.FullName,
                    resident.Code,
                    resident.MobileNumber ?? resident.MobilePhone ?? string.Empty,
                    resident.PhotoPath ?? string.Empty)).ToList());

        var roomCards = rooms.Select(room =>
        {
            var roomResidents = residentRowsByRoom.GetValueOrDefault(room.Id) ?? [];
            var occupied = roomResidents.Count;
            var available = Math.Max(0, room.Capacity - occupied);
            var roomMaintenance = maintenanceByRoom.GetValueOrDefault(room.Id) ?? [];
            var delayedMedicationCount = roomResidents.Sum(resident => delayedMedicationCountByResident.GetValueOrDefault(resident.Id));

            return new RoomCardDto(
                room.Id,
                room.Number,
                room.Floor?.Name ?? string.Empty,
                room.Capacity,
                occupied,
                available,
                $"{occupied} / {room.Capacity}",
                RoomState(occupied, room.Capacity, roomMaintenance, delayedMedicationCount > 0),
                MaintenanceState(roomMaintenance),
                delayedMedicationCount,
                roomResidents);
        }).ToList();

        var sections = roomCards
            .GroupBy(room => new { room.Floor, FloorId = roomLookup[room.Id].FloorId })
            .Select(group =>
            {
                var floor = floorLookup.GetValueOrDefault(group.Key.FloorId);
                var floorRooms = group.ToList();
                return new FloorSectionDto(
                    group.Key.FloorId,
                    group.Key.Floor,
                    floor?.Number ?? 0,
                    floorRooms.Sum(room => room.Capacity),
                    floorRooms.Sum(room => room.Occupied),
                    floorRooms);
            })
            .OrderBy(section => section.Number)
            .ToList();

        var maintenance = maintenanceRows.Select(row => new RoomMaintenanceRowDto(
            row.Request.Id,
            row.Room.Id,
            row.Room.Number,
            row.Floor?.Name ?? string.Empty,
            string.IsNullOrWhiteSpace(row.ResidentName) ? "-" : row.ResidentName,
            row.Request.Title,
            row.Request.Description,
            row.Request.Priority.ToString(),
            PriorityState(row.Request.Priority),
            row.Request.Status.ToString(),
            StatusState(row.Request.Status),
            row.Request.ReportedBy,
            row.Request.ReportedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
            string.IsNullOrWhiteSpace(row.Request.AssignedTo) ? "-" : row.Request.AssignedTo!)).ToList();

        var totalBeds = rooms.Sum(room => room.Capacity);
        var occupiedBeds = residents.Count(resident => resident.CurrentRoomId.HasValue && roomIds.Contains(resident.CurrentRoomId.Value));

        return new RoomsWorkspaceViewModel
        {
            Floors = floors.Select(floor => new RoomFloorOptionDto(floor.Id, floor.Name, floor.Number)).ToList(),
            RoomOptions = rooms.Select(room => new RoomOptionDto(room.Id, RoomLabel(room.Number, room.Floor?.Name), room.FloorId)).ToList(),
            ResidentOptions = allActiveResidents,
            FloorSections = sections,
            MaintenanceRequests = maintenance,
            TotalFloors = floors.Count,
            TotalRooms = rooms.Count,
            TotalBeds = totalBeds,
            OccupiedBeds = occupiedBeds,
            AvailableBeds = Math.Max(0, totalBeds - occupiedBeds),
            FullRooms = roomCards.Count(room => room.Occupied >= room.Capacity),
            OpenMaintenanceRequests = maintenanceRows.Count,
            UrgentMaintenanceRequests = maintenanceRows.Count(row => row.Request.Priority >= RoomMaintenancePriority.High)
        };
    }

    public async Task<Guid> CreateFloorAsync(RoomFloorCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Floor name is required.");
        }

        if (dto.Number <= 0)
        {
            throw new ValidationException("Floor number must be greater than zero.");
        }

        var tenantId = await EnsureTenantAsync(cancellationToken);
        var exists = await db.Floors.AnyAsync(floor => floor.TenantId == tenantId && floor.Number == dto.Number && !floor.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new ValidationException("Floor number already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var floor = new Floor
        {
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            Number = dto.Number,
            CreatedAt = now
        };
        db.Floors.Add(floor);
        AddAudit(tenantId, "CreateFloor", nameof(Floor), floor.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return floor.Id;
    }

    public async Task<Guid> CreateRoomAsync(RoomCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.FloorId == Guid.Empty)
        {
            throw new ValidationException("Floor is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Number))
        {
            throw new ValidationException("Room number is required.");
        }

        if (dto.Capacity <= 0)
        {
            throw new ValidationException("Room capacity must be greater than zero.");
        }

        var floor = await db.Floors.FirstOrDefaultAsync(item => item.Id == dto.FloorId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Floor was not found.");
        var number = dto.Number.Trim();
        var exists = await db.Rooms.AnyAsync(room => room.TenantId == floor.TenantId && room.FloorId == dto.FloorId && room.Number == number && !room.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new ValidationException("Room number already exists on this floor.");
        }

        var now = DateTimeOffset.UtcNow;
        var room = new Room
        {
            TenantId = floor.TenantId,
            FloorId = dto.FloorId,
            Number = number,
            Capacity = dto.Capacity,
            CreatedAt = now
        };
        db.Rooms.Add(room);
        AddAudit(floor.TenantId, "CreateRoom", nameof(Room), room.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return room.Id;
    }

    public async Task UpdateRoomCapacityAsync(RoomCapacityUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.RoomId == Guid.Empty)
        {
            throw new ValidationException("Room is required.");
        }

        if (dto.Capacity <= 0)
        {
            throw new ValidationException("Room capacity must be greater than zero.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(item => item.Id == dto.RoomId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Room was not found.");
        var occupied = await CountCurrentResidentsAsync(room.Id, cancellationToken);

        if (dto.Capacity < occupied)
        {
            throw new ValidationException("Room capacity cannot be less than current residents.");
        }

        if (room.Capacity == dto.Capacity)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        room.Capacity = dto.Capacity;
        room.UpdatedAt = now;
        AddAudit(room.TenantId, "UpdateRoomCapacity", nameof(Room), room.Id, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        if (roomId == Guid.Empty)
        {
            throw new ValidationException("Room is required.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(item => item.Id == roomId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Room was not found.");
        var occupied = await CountCurrentResidentsAsync(room.Id, cancellationToken);

        if (occupied > 0)
        {
            throw new ValidationException("Room cannot be deleted while residents are assigned.");
        }

        var hasOpenMaintenance = await db.RoomMaintenanceRequests.AnyAsync(request =>
            request.RoomId == room.Id &&
            !request.IsDeleted &&
            request.Status != RoomMaintenanceStatus.Completed &&
            request.Status != RoomMaintenanceStatus.Cancelled,
            cancellationToken);
        if (hasOpenMaintenance)
        {
            throw new ValidationException("Room cannot be deleted while maintenance requests are open.");
        }

        var now = DateTimeOffset.UtcNow;
        room.IsDeleted = true;
        room.DeletedAt = now;
        room.UpdatedAt = now;
        AddAudit(room.TenantId, "DeleteRoom", nameof(Room), room.Id, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignResidentToRoomAsync(RoomAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ResidentId == Guid.Empty)
        {
            throw new ValidationException("Resident is required.");
        }

        if (dto.RoomId == Guid.Empty)
        {
            throw new ValidationException("Room is required.");
        }

        var resident = await db.Residents.FirstOrDefaultAsync(item => item.Id == dto.ResidentId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Resident was not found.");
        var room = await db.Rooms.Include(item => item.Floor).FirstOrDefaultAsync(item => item.Id == dto.RoomId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Room was not found.");
        var occupied = await db.Residents.CountAsync(item => item.Id != resident.Id && !item.IsDeleted && item.Status == ResidentStatus.Active && item.CurrentRoomId == room.Id, cancellationToken);

        if (occupied >= room.Capacity)
        {
            throw new ValidationException("Room has no available beds.");
        }

        if (resident.CurrentRoomId == room.Id)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var activeOccupancy = await db.RoomOccupancies
            .Where(item => item.ResidentId == resident.Id && item.EndsOn == null)
            .OrderByDescending(item => item.StartsOn)
            .FirstOrDefaultAsync(cancellationToken);
        if (activeOccupancy is not null)
        {
            activeOccupancy.EndsOn = DateOnly.FromDateTime(DateTime.Today);
            activeOccupancy.UpdatedAt = now;
        }

        resident.CurrentRoomId = room.Id;
        resident.CurrentFloorId = room.FloorId;
        resident.UpdatedAt = now;
        db.RoomOccupancies.Add(new RoomOccupancy
        {
            TenantId = room.TenantId,
            RoomId = room.Id,
            ResidentId = resident.Id,
            StartsOn = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = now
        });
        AddAudit(room.TenantId, "AssignResidentToRoom", nameof(RoomOccupancy), resident.Id, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateMaintenanceRequestAsync(RoomMaintenanceCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.RoomId == Guid.Empty)
        {
            throw new ValidationException("Room is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationException("Maintenance title is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("Maintenance description is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.ReportedBy))
        {
            throw new ValidationException("Reporter name is required.");
        }

        var room = await db.Rooms.FirstOrDefaultAsync(item => item.Id == dto.RoomId && !item.IsDeleted, cancellationToken)
            ?? throw new ValidationException("Room was not found.");

        if (dto.ResidentId.HasValue)
        {
            var residentExists = await db.Residents.AnyAsync(resident => resident.Id == dto.ResidentId.Value && !resident.IsDeleted, cancellationToken);
            if (!residentExists)
            {
                throw new ValidationException("Resident was not found.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var request = new RoomMaintenanceRequest
        {
            TenantId = room.TenantId,
            RoomId = room.Id,
            ResidentId = dto.ResidentId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Priority = ParsePriority(dto.Priority),
            Status = RoomMaintenanceStatus.Open,
            ReportedBy = dto.ReportedBy.Trim(),
            AssignedTo = NormalizeOptional(dto.AssignedTo),
            ReportedAt = now,
            CreatedAt = now
        };
        db.RoomMaintenanceRequests.Add(request);
        AddAudit(room.TenantId, "CreateRoomMaintenanceRequest", nameof(RoomMaintenanceRequest), request.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task CompleteMaintenanceRequestAsync(Guid requestId, string completedBy, string? notes = null, CancellationToken cancellationToken = default)
    {
        var request = await db.RoomMaintenanceRequests.FirstOrDefaultAsync(item => item.Id == requestId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Maintenance request was not found.");

        var now = DateTimeOffset.UtcNow;
        request.Status = RoomMaintenanceStatus.Completed;
        request.CompletedAt = now;
        request.CompletionNotes = notes?.Trim();
        request.UpdatedAt = now;
        db.AuditLogs.Add(new AuditLog
        {
            TenantId = request.TenantId,
            ActorId = string.IsNullOrWhiteSpace(completedBy) ? "system" : completedBy.Trim(),
            Operation = "CompleteRoomMaintenanceRequest",
            EntityName = nameof(RoomMaintenanceRequest),
            EntityId = request.Id,
            OccurredAt = now
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> EnsureTenantAsync(CancellationToken cancellationToken)
    {
        var tenantId = await db.Tenants.Select(tenant => tenant.Id).FirstOrDefaultAsync(cancellationToken);
        if (tenantId != Guid.Empty)
        {
            return tenantId;
        }

        var tenant = new Tenant
        {
            Name = "Saint Stephen Care",
            LegalName = "Saint Stephen Care",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }

    private static RoomMaintenancePriority ParsePriority(string value) => value switch
    {
        "Low" => RoomMaintenancePriority.Low,
        "High" => RoomMaintenancePriority.High,
        "Urgent" => RoomMaintenancePriority.Urgent,
        _ => RoomMaintenancePriority.Normal
    };

    private static string? NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Task<int> CountCurrentResidentsAsync(Guid roomId, CancellationToken cancellationToken) =>
        db.Residents.CountAsync(item => !item.IsDeleted && item.Status == ResidentStatus.Active && item.CurrentRoomId == roomId, cancellationToken);

    private static string RoomLabel(string number, string? floor) =>
        string.IsNullOrWhiteSpace(floor) ? number : $"{number} - {floor}";

    private static string RoomState(int occupied, int capacity, IReadOnlyList<RoomMaintenanceRequest> maintenance, bool hasDelayedMedication)
    {
        if (hasDelayedMedication)
        {
            return "danger";
        }

        if (maintenance.Any(item => item.Priority == RoomMaintenancePriority.Urgent))
        {
            return "danger";
        }

        if (maintenance.Count > 0)
        {
            return "warn";
        }

        if (occupied >= capacity)
        {
            return "teal";
        }

        return occupied == 0 ? "blue" : "";
    }

    private static string MaintenanceState(IReadOnlyList<RoomMaintenanceRequest> maintenance)
    {
        if (maintenance.Count == 0)
        {
            return "none";
        }

        if (maintenance.Any(item => item.Priority == RoomMaintenancePriority.Urgent))
        {
            return "urgent";
        }

        return "open";
    }

    private static string PriorityState(RoomMaintenancePriority priority) => priority switch
    {
        RoomMaintenancePriority.High => "warn",
        RoomMaintenancePriority.Urgent => "danger",
        _ => ""
    };

    private static string StatusState(RoomMaintenanceStatus status) => status switch
    {
        RoomMaintenanceStatus.Completed => "",
        RoomMaintenanceStatus.Cancelled => "danger",
        RoomMaintenanceStatus.InProgress => "warn",
        _ => "warn"
    };

    private void AddAudit(Guid tenantId, string operation, string entityName, Guid entityId, DateTimeOffset now)
    {
        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenantId,
            ActorId = "system",
            Operation = operation,
            EntityName = entityName,
            EntityId = entityId,
            OccurredAt = now
        });
    }

    private sealed record RoomMaintenanceProjection(RoomMaintenanceRequest Request, Room Room, Floor? Floor, string ResidentName);
}
