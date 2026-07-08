using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Commands.MarkDoseAsMissed;

public sealed record MarkDoseAsMissedCommand(
    Guid DoseAdministrationId,
    string? Notes) : ICommand<bool>
{
    public Guid Id { get; } = Guid.NewGuid();
}