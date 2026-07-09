using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.CompleteRoomMaintenanceRequest;

public sealed class CompleteRoomMaintenanceRequestCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<CompleteRoomMaintenanceRequestCommand, bool>
{
    public async Task<bool> Handle(CompleteRoomMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureMaintenanceRequestId(request.RequestId);

        await rooms.CompleteMaintenanceRequestAsync(request.RequestId, request.CompletedBy, request.Notes, cancellationToken);
        return true;
    }
}
