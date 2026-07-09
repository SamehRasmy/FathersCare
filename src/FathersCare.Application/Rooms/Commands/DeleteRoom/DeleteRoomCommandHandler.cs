using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.DeleteRoom;

public sealed class DeleteRoomCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<DeleteRoomCommand, bool>
{
    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureRoomId(request.RoomId);

        await rooms.DeleteRoomAsync(request.RoomId, cancellationToken);
        return true;
    }
}
