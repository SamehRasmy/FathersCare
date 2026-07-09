using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.UpdateRoomCapacity;

public sealed record UpdateRoomCapacityCommand(RoomCapacityUpdateDto Room) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
