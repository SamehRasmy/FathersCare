using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.CreateRoomMaintenanceRequest;

public sealed record CreateRoomMaintenanceRequestCommand(RoomMaintenanceCreateDto Maintenance) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
