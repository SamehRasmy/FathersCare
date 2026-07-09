using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.CreateRoomMaintenanceRequest;

public sealed class CreateRoomMaintenanceRequestCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<CreateRoomMaintenanceRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoomMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureValid(request.Maintenance);

        return await rooms.CreateMaintenanceRequestAsync(request.Maintenance, cancellationToken);
    }
}
