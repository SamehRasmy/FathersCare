using System.ComponentModel.DataAnnotations;
using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands;

internal static class RoomCommandValidation
{
    public static void EnsureValid(RoomFloorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Floor name is required.");
        }

        if (dto.Number <= 0)
        {
            throw new ValidationException("Floor number must be greater than zero.");
        }
    }

    public static void EnsureValid(RoomCreateDto dto)
    {
        EnsureFloorId(dto.FloorId);

        if (string.IsNullOrWhiteSpace(dto.Number))
        {
            throw new ValidationException("Room number is required.");
        }

        EnsureCapacity(dto.Capacity);
    }

    public static void EnsureValid(RoomCapacityUpdateDto dto)
    {
        EnsureRoomId(dto.RoomId);
        EnsureCapacity(dto.Capacity);
    }

    public static void EnsureValid(RoomAssignmentDto dto)
    {
        if (dto.ResidentId == Guid.Empty)
        {
            throw new ValidationException("Resident is required.");
        }

        EnsureRoomId(dto.RoomId);
    }

    public static void EnsureValid(RoomMaintenanceCreateDto dto)
    {
        EnsureRoomId(dto.RoomId);

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
    }

    public static void EnsureRoomId(Guid roomId)
    {
        if (roomId == Guid.Empty)
        {
            throw new ValidationException("Room is required.");
        }
    }

    public static void EnsureMaintenanceRequestId(Guid requestId)
    {
        if (requestId == Guid.Empty)
        {
            throw new ValidationException("Maintenance request is required.");
        }
    }

    private static void EnsureFloorId(Guid floorId)
    {
        if (floorId == Guid.Empty)
        {
            throw new ValidationException("Floor is required.");
        }
    }

    private static void EnsureCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ValidationException("Room capacity must be greater than zero.");
        }
    }
}
