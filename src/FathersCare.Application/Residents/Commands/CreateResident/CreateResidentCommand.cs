using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Residents.Commands.CreateResident;

public sealed record CreateResidentCommand(ResidentUpsertDto Resident) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
