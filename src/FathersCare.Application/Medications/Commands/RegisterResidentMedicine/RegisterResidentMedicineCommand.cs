using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Commands.RegisterResidentMedicine;

public sealed record RegisterResidentMedicineCommand(
    Guid ResidentId,
    Guid MedicineId,
    string? Instructions) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}