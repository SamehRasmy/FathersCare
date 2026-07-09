using System.ComponentModel.DataAnnotations;
using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Commands.CreateResident;
using FathersCare.Application.Residents.Commands.UpdateResident;
using FathersCare.Domain.Residents;

namespace FathersCare.UnitTests;

public class ResidentCommandHandlerTests
{
    [Fact]
    public async Task CreateResidentCommandHandler_CreatesResident_WhenRequestIsValid()
    {
        var service = new FakeResidentManagementService();
        var handler = new CreateResidentCommandHandler(service);
        var dto = BuildValidResident();

        var residentId = await handler.Handle(new CreateResidentCommand(dto), CancellationToken.None);

        Assert.Equal(service.CreatedResidentId, residentId);
        Assert.Same(dto, service.CreatedResident);
    }

    [Fact]
    public async Task CreateResidentCommandHandler_StopsBeforeService_WhenRequestIsInvalid()
    {
        var service = new FakeResidentManagementService();
        var handler = new CreateResidentCommandHandler(service);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateResidentCommand(new ResidentUpsertDto()), CancellationToken.None));

        Assert.Null(service.CreatedResident);
    }

    [Fact]
    public async Task UpdateResidentCommandHandler_UpdatesResident_WhenRequestIsValid()
    {
        var service = new FakeResidentManagementService();
        var handler = new UpdateResidentCommandHandler(service);
        var residentId = Guid.NewGuid();
        var dto = BuildValidResident();

        var updated = await handler.Handle(new UpdateResidentCommand(residentId, dto), CancellationToken.None);

        Assert.True(updated);
        Assert.Equal(residentId, service.UpdatedResidentId);
        Assert.Same(dto, service.UpdatedResident);
    }

    [Fact]
    public async Task UpdateResidentCommandHandler_RejectsEmptyResidentId()
    {
        var service = new FakeResidentManagementService();
        var handler = new UpdateResidentCommandHandler(service);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new UpdateResidentCommand(Guid.Empty, BuildValidResident()), CancellationToken.None));

        Assert.Null(service.UpdatedResident);
    }

    private static ResidentUpsertDto BuildValidResident() =>
        new()
        {
            Code = "R-100",
            FullName = "Resident Name",
            BirthDate = new DateOnly(1955, 2, 25),
            Gender = ResidentGender.Male.ToString(),
            CurrentAddress = "Cairo",
            NationalId = "12345678901234",
            AdmissionDate = new DateOnly(2026, 7, 8),
            AdmissionStatus = AdmissionStatus.Admitted.ToString(),
            Status = ResidentStatus.Active.ToString(),
            RoomId = Guid.NewGuid(),
            Contacts =
            [
                new ResidentContactEditorDto(
                    null,
                    "Primary Contact",
                    "Son",
                    string.Empty,
                    "Cairo",
                    "01000000000",
                    string.Empty,
                    true,
                    true,
                    string.Empty)
            ]
        };

    private sealed class FakeResidentManagementService : IResidentManagementService
    {
        public Guid CreatedResidentId { get; } = Guid.NewGuid();
        public ResidentUpsertDto? CreatedResident { get; private set; }
        public Guid? UpdatedResidentId { get; private set; }
        public ResidentUpsertDto? UpdatedResident { get; private set; }

        public Task<ResidentsWorkspaceViewModel> GetWorkspaceAsync(string? search = null, string? status = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ManagedResidentDetailsViewModel?> GetDetailsAsync(Guid residentId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ResidentEditorViewModel> GetEditorAsync(Guid? residentId = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Guid> CreateAsync(ResidentUpsertDto dto, CancellationToken cancellationToken = default)
        {
            CreatedResident = dto;
            return Task.FromResult(CreatedResidentId);
        }

        public Task UpdateAsync(Guid residentId, ResidentUpsertDto dto, CancellationToken cancellationToken = default)
        {
            UpdatedResidentId = residentId;
            UpdatedResident = dto;
            return Task.CompletedTask;
        }

        public Task<string> UploadPhotoAsync(Guid residentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ResidentDocumentViewModel> UploadDocumentAsync(Guid residentId, ResidentDocumentUploadDto dto, Stream content, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteDocumentAsync(Guid residentId, Guid documentId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
