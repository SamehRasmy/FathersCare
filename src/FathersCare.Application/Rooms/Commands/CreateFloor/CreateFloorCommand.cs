using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.CreateFloor;

public sealed record CreateFloorCommand(RoomFloorCreateDto Floor) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
