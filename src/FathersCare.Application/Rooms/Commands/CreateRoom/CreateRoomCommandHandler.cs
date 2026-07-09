using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<CreateRoomCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureValid(request.Room);

        return await rooms.CreateRoomAsync(request.Room, cancellationToken);
    }
}
