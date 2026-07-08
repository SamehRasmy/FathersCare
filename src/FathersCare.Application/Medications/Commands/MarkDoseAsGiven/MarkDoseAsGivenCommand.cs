using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Medications.Commands.MarkDoseAsGiven;

public sealed record MarkDoseAsGivenCommand(Guid DoseAdministrationId, DateTimeOffset GivenAt, string GivenBy) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}
