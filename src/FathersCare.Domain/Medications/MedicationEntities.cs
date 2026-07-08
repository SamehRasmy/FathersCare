using FathersCare.Domain.Common;

namespace FathersCare.Domain.Medications;

public enum DoseTiming
{
    BeforeMeal = 1,
    WithMeal = 2,
    AfterMeal = 3,
    AnyTime = 4
}

public enum DoseAdministrationStatus
{
    Scheduled = 1,
    Given = 2,
    Missed = 3,
    Skipped = 4
}

public sealed class Medicine : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? Form { get; set; }
}

public sealed class ResidentMedicine : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Guid MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    public string? Instructions { get; set; }
    public string? PrescribedBy { get; set; }
    public string? PrescriptionReference { get; set; }
    public DateOnly? PrescriptionDate { get; set; }
    public DateOnly? StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class MedicineSchedule : TenantEntity
{
    public Guid ResidentMedicineId { get; set; }
    public ResidentMedicine? ResidentMedicine { get; set; }
    public TimeOnly DoseTime { get; set; }
    public decimal Quantity { get; set; }
    public DoseTiming Timing { get; set; } = DoseTiming.AnyTime;
}

public sealed class DoseAdministration : TenantEntity
{
    public Guid MedicineScheduleId { get; set; }
    public MedicineSchedule? MedicineSchedule { get; set; }
    public Guid? ResidentMedicineBatchId { get; set; }
    public ResidentMedicineBatch? ResidentMedicineBatch { get; set; }
    public DateOnly DoseDate { get; set; }
    public DoseAdministrationStatus Status { get; set; } = DoseAdministrationStatus.Scheduled;
    public DateTimeOffset? AdministeredAt { get; set; }
    public string? AdministeredBy { get; set; }
    public string? Notes { get; set; }
}

public sealed class ResidentMedicineBatch : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Guid ResidentMedicineId { get; set; }
    public ResidentMedicine? ResidentMedicine { get; set; }
    public DateOnly ReceivedOn { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public decimal QuantityReceived { get; set; }
    public decimal QuantityRemaining { get; set; }
    public DateOnly? ExpiresOn { get; set; }
    public string? PrescriptionReference { get; set; }
    public string? Notes { get; set; }
}

public sealed class MedicineShelf : TenantEntity
{
    public Guid MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderThreshold { get; set; }
}
