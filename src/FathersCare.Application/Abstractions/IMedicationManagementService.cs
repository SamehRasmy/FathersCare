namespace FathersCare.Application.Abstractions;

public interface IMedicationManagementService
{
    Task<MedicationOperationsViewModel> GetWorkspaceAsync(Guid? residentId = null, CancellationToken cancellationToken = default);
    Task<MedicationPlanEditDto> GetPlanForEditAsync(Guid residentMedicineId, CancellationToken cancellationToken = default);
    Task<Guid> ReceiveMedicineAsync(ReceiveMedicineDto dto, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(Guid residentMedicineId, ReceiveMedicineDto dto, CancellationToken cancellationToken = default);
    Task GenerateDosePlanAsync(Guid residentMedicineId, IReadOnlyList<DosePlanInputDto> doses, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(Guid residentMedicineId, string? reason = null, CancellationToken cancellationToken = default);
    Task MarkDoseGivenAsync(Guid doseAdministrationId, string administeredBy, string? notes = null, CancellationToken cancellationToken = default);
    Task MarkDoseMissedAsync(Guid doseAdministrationId, string? notes = null, CancellationToken cancellationToken = default);
    Task SkipDoseAsync(Guid doseAdministrationId, string? notes = null, CancellationToken cancellationToken = default);
}

public sealed record MedicationResidentOptionDto(Guid Id, string Name);
public sealed record MedicationItemOptionDto(Guid Id, string Name);
public sealed record DosePlanInputDto(string Time, decimal Quantity, string Timing);

public sealed record ReceiveMedicineDto(
    Guid ResidentId,
    string MedicineName,
    string Strength,
    string Form,
    decimal QuantityReceived,
    DateOnly ReceivedOn,
    DateOnly? ExpiresOn,
    string ReceivedFrom,
    string ReceivedBy,
    string PrescribedBy,
    string PrescriptionReference,
    DateOnly? PrescriptionDate,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Instructions,
    IReadOnlyList<DosePlanInputDto> Doses);

public sealed record MedicationPlanEditDto(Guid Id, ReceiveMedicineDto Medicine);

public sealed record MedicationBatchRowDto(
    Guid Id,
    string Resident,
    string Medicine,
    string ReceivedOn,
    string ExpiresOn,
    string QuantityReceived,
    string QuantityRemaining,
    string ReceivedFrom,
    string ReceivedBy,
    string PrescriptionReference,
    string State);

public sealed record DoseExecutionRowDto(
    Guid Id,
    string Resident,
    string Medicine,
    string Time,
    string Dose,
    string Instructions,
    string Status,
    string State,
    bool CanGive);

public sealed record ResidentMedicationPlanRowDto(
    Guid Id,
    string Resident,
    string Medicine,
    string Instructions,
    string Schedule,
    string Stock,
    string Period,
    string State);

public sealed class MedicationOperationsViewModel
{
    public IReadOnlyList<MedicationResidentOptionDto> Residents { get; init; } = [];
    public IReadOnlyList<MedicationItemOptionDto> Medicines { get; init; } = [];
    public IReadOnlyList<DoseExecutionRowDto> TodayDoses { get; init; } = [];
    public IReadOnlyList<MedicationBatchRowDto> Batches { get; init; } = [];
    public IReadOnlyList<ResidentMedicationPlanRowDto> Plans { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> Alerts { get; init; } = [];
    public int DueNowCount { get; init; }
    public int LateCount { get; init; }
    public int LowStockCount { get; init; }
    public int ExpiringSoonCount { get; init; }
}
