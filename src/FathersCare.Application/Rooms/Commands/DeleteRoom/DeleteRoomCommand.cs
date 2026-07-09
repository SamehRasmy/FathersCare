using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.DeleteRoom;

public sealed record DeleteRoomCommand(Guid RoomId) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
