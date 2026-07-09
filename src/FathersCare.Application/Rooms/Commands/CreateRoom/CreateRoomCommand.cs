using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.CreateRoom;

public sealed record CreateRoomCommand(RoomCreateDto Room) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
