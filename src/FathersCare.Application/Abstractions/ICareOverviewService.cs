namespace FathersCare.Application.Abstractions;

public interface ICareOverviewService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<MedicationViewModel> GetMedicationsAsync(CancellationToken cancellationToken = default);
    Task<NutritionViewModel> GetNutritionAsync(CancellationToken cancellationToken = default);
    Task<ReportsViewModel> GetReportsAsync(CancellationToken cancellationToken = default);
    Task<ResidentProfileViewModel?> GetResidentProfileAsync(Guid? residentId = null, CancellationToken cancellationToken = default);
}

public sealed record StatCardDto(string Title, string Value, string Caption, string Color, string Icon);
public sealed record SimpleRowDto(string Label, string Value, string Status = "");
public sealed record ScheduleRowDto(string Time, string Title, string Details);
public sealed record RoomTileDto(string Number, string State);
public sealed record MedicationRowDto(string Resident, string Medicine, string Dose, string Time, string Method, string LastDose, string Status, string State);
public sealed record InventoryRowDto(string Item, string Quantity, string Threshold, string Status, string State);
public sealed record FinanceRowDto(string Category, decimal Amount, decimal Percent);

public sealed class DashboardViewModel
{
    public IReadOnlyList<StatCardDto> Stats { get; init; } = [];
    public IReadOnlyList<MedicationRowDto> PriorityDoses { get; init; } = [];
    public IReadOnlyList<RoomTileDto> Rooms { get; init; } = [];
    public IReadOnlyList<ScheduleRowDto> TodaySchedule { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> RoomAlerts { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> LiveEvents { get; init; } = [];
    public int GivenDoses { get; init; }
    public int RemainingDoses { get; init; }
    public int LateDoses { get; init; }
    public int MealCoveragePercent { get; init; }
    public int DoseCoveragePercent { get; init; }
}

public sealed class MedicationViewModel
{
    public IReadOnlyList<StatCardDto> Stats { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> Alerts { get; init; } = [];
    public IReadOnlyList<ScheduleRowDto> UpcomingDoses { get; init; } = [];
    public IReadOnlyList<MedicationRowDto> MedicationRows { get; init; } = [];
    public IReadOnlyList<InventoryRowDto> Inventory { get; init; } = [];
    public int TotalDailyDoses { get; init; }
    public int GivenDailyDoses { get; init; }
    public int CompliancePercent { get; init; }
}

public sealed class NutritionViewModel
{
    public IReadOnlyList<StatCardDto> Stats { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> InventoryAlerts { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> SpecialCases { get; init; } = [];
    public IReadOnlyList<ScheduleRowDto> MealSchedule { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> SpecialMeals { get; init; } = [];
    public IReadOnlyList<InventoryRowDto> Inventory { get; init; } = [];
    public int PreparedMeals { get; init; }
    public int DeliveredMeals { get; init; }
    public int LateMeals { get; init; }
    public int CoveragePercent { get; init; }
}

public sealed class ReportsViewModel
{
    public IReadOnlyList<StatCardDto> Stats { get; init; } = [];
    public IReadOnlyList<FinanceRowDto> RevenueBreakdown { get; init; } = [];
    public IReadOnlyList<FinanceRowDto> ExpenseBreakdown { get; init; } = [];
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetOperating { get; init; }
    public decimal CoveragePercent { get; init; }
}

public sealed class ResidentProfileViewModel
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
    public string BirthDate { get; init; } = string.Empty;
    public string Age { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string AllergiesSummary { get; init; } = string.Empty;
    public string ChronicConditionsSummary { get; init; } = string.Empty;
    public IReadOnlyList<SimpleRowDto> BasicInfo { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> Contacts { get; init; } = [];
    public IReadOnlyList<MedicationRowDto> Medications { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> VitalSigns { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> Activity { get; init; } = [];
}
