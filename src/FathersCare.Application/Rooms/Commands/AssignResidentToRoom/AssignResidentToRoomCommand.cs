using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.AssignResidentToRoom;

public sealed record AssignResidentToRoomCommand(RoomAssignmentDto Assignment) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
