using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Commands.CreateMedicineSchedule;

public sealed record CreateMedicineScheduleCommand(
    Guid ResidentMedicineId,
    TimeOnly DoseTime,
    decimal Quantity,
    DoseTiming Timing) : ICommand<Guid>
{
    public Guid Id { get; } = Guid.NewGuid();
}
