using System.ComponentModel.DataAnnotations;
using FathersCare.Application.Abstractions;
using FathersCare.Application.Rooms.Commands.CreateRoom;
using FathersCare.Application.Rooms.Commands.CreateRoomMaintenanceRequest;
using FathersCare.Application.Rooms.Commands.DeleteRoom;

namespace FathersCare.UnitTests;

public class RoomCommandHandlerTests
{
    [Fact]
    public async Task CreateRoomCommandHandler_CreatesRoom_WhenRequestIsValid()
    {
        var service = new FakeRoomManagementService();
        var handler = new CreateRoomCommandHandler(service);
        var dto = new RoomCreateDto(Guid.NewGuid(), "101", 2);

        var roomId = await handler.Handle(new CreateRoomCommand(dto), CancellationToken.None);

        Assert.Equal(service.CreatedRoomId, roomId);
        Assert.Equal(dto, service.CreatedRoom);
    }

    [Fact]
    public async Task CreateRoomCommandHandler_StopsBeforeService_WhenRequestIsInvalid()
    {
        var service = new FakeRoomManagementService();
        var handler = new CreateRoomCommandHandler(service);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateRoomCommand(new RoomCreateDto(Guid.Empty, string.Empty, 0)), CancellationToken.None));

        Assert.Null(service.CreatedRoom);
    }

    [Fact]
    public async Task DeleteRoomCommandHandler_RejectsEmptyRoomId()
    {
        var service = new FakeRoomManagementService();
        var handler = new DeleteRoomCommandHandler(service);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new DeleteRoomCommand(Guid.Empty), CancellationToken.None));

        Assert.Null(service.DeletedRoomId);
    }

    [Fact]
    public async Task CreateRoomMaintenanceRequestCommandHandler_CreatesRequest_WhenRequestIsValid()
    {
        var service = new FakeRoomManagementService();
        var handler = new CreateRoomMaintenanceRequestCommandHandler(service);
        var dto = new RoomMaintenanceCreateDto(Guid.NewGuid(), null, "AC issue", "Needs inspection", "Normal", "Nurse", "Maintenance");

        var requestId = await handler.Handle(new CreateRoomMaintenanceRequestCommand(dto), CancellationToken.None);

        Assert.Equal(service.CreatedMaintenanceRequestId, requestId);
        Assert.Equal(dto, service.CreatedMaintenanceRequest);
    }

    private sealed class FakeRoomManagementService : IRoomManagementService
    {
        public Guid CreatedRoomId { get; } = Guid.NewGuid();
        public RoomCreateDto? CreatedRoom { get; private set; }
        public Guid? DeletedRoomId { get; private set; }
        public Guid CreatedMaintenanceRequestId { get; } = Guid.NewGuid();
        public RoomMaintenanceCreateDto? CreatedMaintenanceRequest { get; private set; }

        public Task<RoomsWorkspaceViewModel> GetWorkspaceAsync(Guid? floorId = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Guid> CreateFloorAsync(RoomFloorCreateDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Guid> CreateRoomAsync(RoomCreateDto dto, CancellationToken cancellationToken = default)
        {
            CreatedRoom = dto;
            return Task.FromResult(CreatedRoomId);
        }

        public Task UpdateRoomCapacityAsync(RoomCapacityUpdateDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            DeletedRoomId = roomId;
            return Task.CompletedTask;
        }

        public Task AssignResidentToRoomAsync(RoomAssignmentDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Guid> CreateMaintenanceRequestAsync(RoomMaintenanceCreateDto dto, CancellationToken cancellationToken = default)
        {
            CreatedMaintenanceRequest = dto;
            return Task.FromResult(CreatedMaintenanceRequestId);
        }

        public Task CompleteMaintenanceRequestAsync(Guid requestId, string completedBy, string? notes = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
