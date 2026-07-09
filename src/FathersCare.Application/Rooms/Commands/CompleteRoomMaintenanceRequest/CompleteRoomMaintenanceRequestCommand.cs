using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Rooms.Commands.CompleteRoomMaintenanceRequest;

public sealed record CompleteRoomMaintenanceRequestCommand(Guid RequestId, string CompletedBy, string? Notes = null) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
