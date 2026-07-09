using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.AssignResidentToRoom;

public sealed class AssignResidentToRoomCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<AssignResidentToRoomCommand, bool>
{
    public async Task<bool> Handle(AssignResidentToRoomCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureValid(request.Assignment);

        await rooms.AssignResidentToRoomAsync(request.Assignment, cancellationToken);
        return true;
    }
}
