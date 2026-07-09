using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands;

namespace FathersCare.Application.Rooms.Commands.CreateFloor;

public sealed class CreateFloorCommandHandler(IRoomManagementService rooms)
    : ICommandHandler<CreateFloorCommand, Guid>
{
    public async Task<Guid> Handle(CreateFloorCommand request, CancellationToken cancellationToken)
    {
        RoomCommandValidation.EnsureValid(request.Floor);

        return await rooms.CreateFloorAsync(request.Floor, cancellationToken);
    }
}
