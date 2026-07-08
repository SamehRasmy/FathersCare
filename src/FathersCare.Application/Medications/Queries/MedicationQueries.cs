using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Queries;

public sealed record GetMedicinesQuery : IQuery<IReadOnlyList<MedicineDto>>
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed record MedicineDto(
    Guid Id,
    string Name,
    string? Strength,
    string? Form);

public sealed record GetResidentMedicinesQuery(Guid ResidentId) : IQuery<IReadOnlyList<ResidentMedicineDto>>
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed record ResidentMedicineDto(
    Guid Id,
    Guid MedicineId,
    string MedicineName,
    string? Strength,
    string? Instructions,
    bool IsActive);

public sealed record GetMedicineSchedulesQuery(Guid ResidentMedicineId) : IQuery<IReadOnlyList<MedicineScheduleDto>>
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed record MedicineScheduleDto(
    Guid Id,
    TimeOnly DoseTime,
    decimal Quantity,
    DoseTiming Timing);

public sealed record GetDoseAdministrationsQuery(
    Guid? ResidentMedicineId = null,
    DateOnly? Date = null) : IQuery<IReadOnlyList<DoseAdministrationDto>>
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed record DoseAdministrationDto(
    Guid Id,
    Guid MedicineScheduleId,
    DateOnly DoseDate,
    DoseAdministrationStatus Status,
    DateTimeOffset? AdministeredAt,
    string? AdministeredBy,
    string? Notes);
