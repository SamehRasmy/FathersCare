using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.UpdateRoomCapacity;

public sealed class UpdateRoomCapacityCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<UpdateRoomCapacityCommand, bool>
{
    public async Task<bool> Handle(UpdateRoomCapacityCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureValid(request.Room);

        await rooms.UpdateRoomCapacityAsync(request.Room, cancellationToken);
        return true;
    }
}
