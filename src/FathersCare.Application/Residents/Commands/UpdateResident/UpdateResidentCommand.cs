using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Residents.Commands.UpdateResident;

public sealed record UpdateResidentCommand(Guid ResidentId, ResidentUpsertDto Resident) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
