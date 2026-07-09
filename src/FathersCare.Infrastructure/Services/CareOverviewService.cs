using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Residents;
using FathersCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FathersCare.Infrastructure.Services;

public sealed class CareOverviewService(AppDbContext db) : ICareOverviewService
{
    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var residents = await db.Residents.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var doses = await LoadMedicationRows(today, cancellationToken);
        var meals = await db.MealDistributions.AsNoTracking().Where(x => x.DistributionDate == today).ToListAsync(cancellationToken);
        var rooms = await db.Rooms.AsNoTracking().OrderBy(x => x.Number).ToListAsync(cancellationToken);
        var lateDoses = doses.Count(x => x.State == "danger");
        var givenDoses = doses.Count(x => x.State == "");
        var totalDoses = Math.Max(doses.Count, 1);
        var deliveredMeals = meals.Count(x => x.Delivered);
        var totalMeals = Math.Max(meals.Count, 1);

        return new DashboardViewModel
        {
            Stats =
            [
                new("إجمالي النزلاء", residents.Count.ToString(), "نزيل", "blue", "ن"),
                new("تنبيهات عاجلة", (lateDoses + LowMedicineCount()).ToString(), "تحتاج متابعة", "red", "!"),
                new("جرعات اليوم", doses.Count.ToString(), "جرعات مسجلة", "purple", "د"),
                new("وجبات اليوم", meals.Count.ToString(), "وجبات مسجلة", "gold", "و"),
                new("زيارات اليوم", "2", "زيارات مسجلة", "teal", "ز"),
                new("أنشطة روحية", "1", "نشاط", "purple", "ر"),
                new("مواعيد قادمة", "4", "مواعيد", "blue", "م")
            ],
            PriorityDoses = doses.Where(x => x.Status != "تم الإعطاء").OrderBy(x => x.Time).Take(4).ToList(),
            Rooms = rooms.Select((room, index) => new RoomTileDto(room.Number, (index % 7) switch
            {
                1 => "danger",
                3 => "warn",
                5 => "blue",
                6 => "purple",
                _ => ""
            })).ToList(),
            TodaySchedule =
            [
                new("08:00", "جرعات الصباح", "الأدوية الصباحية"),
                new("09:00", "وجبة الإفطار", "المطبخ"),
                new("10:30", "زيارة طبية", "الدكتور يوسف سامي"),
                new("12:00", "قداس منتصف النهار", "كنيسة الدار"),
                new("15:00", "نشاط ترفيهي", "قاعة الأنشطة"),
                new("20:00", "جرعات المساء", "جميع النزلاء")
            ],
            RoomAlerts =
            [
                new("بحاجة إلى دواء", lateDoses.ToString(), "danger"),
                new("في الطريق", "3", "warn"),
                new("حالة طبيعية", Math.Max(residents.Count - lateDoses, 0).ToString())
            ],
            LiveEvents =
            [
                new(DateTime.Now.AddMinutes(-1).ToString("HH:mm"), "تم تحديث بيانات الجرعات من قاعدة البيانات"),
                new(DateTime.Now.AddMinutes(-12).ToString("HH:mm"), $"تم تسليم {deliveredMeals} وجبات اليوم"),
                new(DateTime.Now.AddMinutes(-25).ToString("HH:mm"), $"يوجد {LowKitchenCount()} أصناف مطبخ تحت الحد الأدنى")
            ],
            GivenDoses = givenDoses,
            RemainingDoses = doses.Count(x => x.State == "warn"),
            LateDoses = lateDoses,
            MealCoveragePercent = Percent(deliveredMeals, totalMeals),
            DoseCoveragePercent = Percent(givenDoses, totalDoses)
        };
    }

    public async Task<MedicationViewModel> GetMedicationsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var totalResidents = await db.Residents.CountAsync(x => !x.IsDeleted, cancellationToken);
        var residentsWithMedicine = await db.ResidentMedicines.Select(x => x.ResidentId).Distinct().CountAsync(cancellationToken);
        var totalMedicines = await db.Medicines.CountAsync(cancellationToken);
        var rows = await LoadMedicationRows(today, cancellationToken);
        var inventory = await LoadMedicineInventory(cancellationToken);
        var given = rows.Count(x => x.State == "");

        return new MedicationViewModel
        {
            Stats =
            [
                new("إجمالي النزلاء", totalResidents.ToString(), "نزيل", "blue", "ن"),
                new("يتناولون أدوية", residentsWithMedicine.ToString(), "نزيل", "teal", "غ"),
                new("إجمالي الأدوية المسجلة", totalMedicines.ToString(), "دواء", "blue", "د"),
                new("إجمالي الجرعات اليومية", rows.Count.ToString(), "جرعة", "purple", "ج"),
                new("جرعات متأخرة", rows.Count(x => x.State == "danger").ToString(), "جرعات", "red", "!")
            ],
            Alerts =
            [
                new("جرعات متأخرة لم يتم إعطاؤها", rows.Count(x => x.State == "danger").ToString(), "danger"),
                new("أدوية قاربت على الانتهاء", inventory.Count(x => x.State == "warn").ToString(), "warn"),
                new("أدوية أقل من الحد الأدنى", inventory.Count(x => x.State == "danger").ToString(), "danger")
            ],
            UpcomingDoses = rows.Take(3).Select(x => new ScheduleRowDto(x.Time, x.Resident, x.Medicine)).ToList(),
            MedicationRows = rows,
            Inventory = inventory,
            TotalDailyDoses = rows.Count,
            GivenDailyDoses = given,
            CompliancePercent = Percent(given, Math.Max(rows.Count, 1))
        };
    }

    public async Task<NutritionViewModel> GetNutritionAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var residents = await db.Residents.CountAsync(x => !x.IsDeleted, cancellationToken);
        var meals = await db.MealDistributions.AsNoTracking().Where(x => x.DistributionDate == today).ToListAsync(cancellationToken);
        var specialDiets = await db.ResidentDietPlans.AsNoTracking().Include(x => x.DietPlan).ToListAsync(cancellationToken);
        var inventory = await LoadKitchenInventory(cancellationToken);
        var delivered = meals.Count(x => x.Delivered);

        return new NutritionViewModel
        {
            Stats =
            [
                new("إجمالي النزلاء المقيمين", residents.ToString(), "نزيل", "teal", "ن"),
                new("إجمالي وجبات اليوم", meals.Count.ToString(), "وجبة", "teal", "و"),
                new("وجبات خاصة حسب الحالة الصحية", specialDiets.Count.ToString(), "وجبة", "purple", "خ"),
                new("وجبات مهروسة / لينة", specialDiets.Count(x => x.DietPlan != null && x.DietPlan.Name.Contains("مهروسة")).ToString(), "وجبات", "gold", "م"),
                new("وجبات لمرضى السكر", specialDiets.Count(x => x.DietPlan != null && x.DietPlan.Name.Contains("السكر")).ToString(), "وجبات", "red", "س")
            ],
            InventoryAlerts = inventory.Where(x => x.State != "").Select(x => new SimpleRowDto(x.Item, x.Status, x.State)).ToList(),
            SpecialCases = await LoadSpecialCases(cancellationToken),
            MealSchedule =
            [
                new("08:00", "إفطار عادي", $"{delivered} وجبات تم تسليمها"),
                new("09:15", "وجبات خاصة", $"{specialDiets.Count} حالات"),
                new("13:00", "غداء اليوم", "قيد التحضير")
            ],
            SpecialMeals = await LoadSpecialMeals(cancellationToken),
            Inventory = inventory,
            PreparedMeals = meals.Count,
            DeliveredMeals = delivered,
            LateMeals = meals.Count(x => !x.Delivered),
            CoveragePercent = Percent(delivered, Math.Max(meals.Count, 1))
        };
    }

    public async Task<ReportsViewModel> GetReportsAsync(CancellationToken cancellationToken = default)
    {
        var revenues = await db.Revenues.AsNoTracking().ToListAsync(cancellationToken);
        var expenses = await db.Expenses.AsNoTracking().ToListAsync(cancellationToken);
        var totalRevenue = revenues.Sum(x => x.Amount);
        var totalExpense = expenses.Sum(x => x.Amount);
        var residents = await db.Residents.CountAsync(x => !x.IsDeleted, cancellationToken);
        var rooms = await db.Rooms.SumAsync(x => x.Capacity, cancellationToken);
        var net = totalRevenue - totalExpense;

        return new ReportsViewModel
        {
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpense,
            NetOperating = net,
            CoveragePercent = totalExpense == 0 ? 0 : Math.Round(totalRevenue / totalExpense * 100, 1),
            Stats =
            [
                new("إجمالي الإيرادات", Money(totalRevenue), "جنيه", "teal", "$"),
                new("إجمالي المصروفات", Money(totalExpense), "جنيه", "red", "م"),
                new("صافي التشغيل", Money(net), "جنيه", "blue", "ص"),
                new("نسبة تغطية التكاليف", (totalExpense == 0 ? "0" : Math.Round(totalRevenue / totalExpense * 100, 1)) + "%", "محقق", "purple", "%"),
                new("إشغال الغرف", Percent(residents, Math.Max(rooms, 1)) + "%", $"{residents} من {rooms}", "gold", "غ"),
                new("إجمالي النزلاء", residents.ToString(), "مقيمين", "teal", "ن"),
                new("متوسط تكلفة الإقامة", Money(residents == 0 ? 0 : totalExpense / residents), "جنيه / شهر", "gold", "ت")
            ],
            RevenueBreakdown = revenues.Select(x => new FinanceRowDto(x.Category, x.Amount, PercentDecimal(x.Amount, totalRevenue))).ToList(),
            ExpenseBreakdown = expenses.Select(x => new FinanceRowDto(x.Category, x.Amount, PercentDecimal(x.Amount, totalExpense))).ToList()
        };
    }

    public async Task<ResidentProfileViewModel?> GetResidentProfileAsync(Guid? residentId = null, CancellationToken cancellationToken = default)
    {
        var resident = await db.Residents.AsNoTracking()
            .Include(x => x.MedicalProfile)
            .Include(x => x.Contacts)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(x => residentId == null || x.Id == residentId.Value, cancellationToken);

        if (resident is null)
        {
            return null;
        }

        var room = resident.CurrentRoomId is null
            ? null
            : await db.Rooms.AsNoTracking().Include(x => x.Floor).FirstOrDefaultAsync(x => x.Id == resident.CurrentRoomId.Value, cancellationToken);

        var meds = await LoadMedicationRows(DateOnly.FromDateTime(DateTime.Today), cancellationToken);
        var residentMeds = meds.Where(x => x.Resident == resident.FullName).ToList();
        var age = resident.BirthDate is null ? "" : $"{DateTime.Today.Year - resident.BirthDate.Value.Year} سنة";

        return new ResidentProfileViewModel
        {
            Id = resident.Id,
            FullName = resident.FullName,
            Code = resident.Code,
            Status = StatusText(resident.Status),
            Room = room is null ? "غير محدد" : $"الغرفة {room.Number} - {room.Floor?.Name}",
            BirthDate = resident.BirthDate?.ToString("dd / MM / yyyy") ?? "",
            Age = age,
            BloodType = resident.MedicalProfile?.BloodType ?? "غير محدد",
            AllergiesSummary = resident.MedicalProfile?.AllergiesSummary ?? "لا يوجد",
            ChronicConditionsSummary = resident.MedicalProfile?.ChronicConditionsSummary ?? "لا يوجد",
            BasicInfo =
            [
                new("الاسم الرباعي", resident.FullName),
                new("الكود", resident.Code),
                new("تاريخ الميلاد", resident.BirthDate?.ToString("dd / MM / yyyy") ?? ""),
                new("السن", age),
                new("فصيلة الدم", resident.MedicalProfile?.BloodType ?? "غير محدد"),
                new("الحالة الحالية", StatusText(resident.Status), "")
            ],
            Contacts = resident.Contacts.Select(x => new SimpleRowDto($"{x.FullName} - {x.Relationship}", x.MobileNumber ?? x.PhoneNumber ?? "", x.IsPrimary ? "warn" : "")).ToList(),
            Medications = residentMeds,
            VitalSigns =
            [
                new("ضغط الدم", "120 / 80 mmHg"),
                new("السكر", "138 mg/dl"),
                new("النبض", "78 /min"),
                new("الوزن", "72 Kg")
            ],
            Activity =
            [
                new("زيارة عائلية", "05/06/2026"),
                new("تغيير جرعة", "12/06/2026"),
                new("تحليل دم", "18/06/2026"),
                new("تم إعطاء دواء", DateTime.Today.ToString("dd/MM/yyyy"))
            ]
        };
    }

    private async Task<List<MedicationRowDto>> LoadMedicationRows(DateOnly today, CancellationToken cancellationToken)
    {
        var query = await (
            from dose in db.DoseAdministrations.AsNoTracking()
            join schedule in db.MedicineSchedules.AsNoTracking() on dose.MedicineScheduleId equals schedule.Id
            join residentMedicine in db.ResidentMedicines.AsNoTracking() on schedule.ResidentMedicineId equals residentMedicine.Id
            join medicine in db.Medicines.AsNoTracking() on residentMedicine.MedicineId equals medicine.Id
            join resident in db.Residents.AsNoTracking() on residentMedicine.ResidentId equals resident.Id
            where dose.DoseDate == today
            orderby schedule.DoseTime
            select new
            {
                resident.FullName,
                Medicine = medicine.Name + " " + medicine.Strength,
                Dose = schedule.Quantity == 1 ? "قرص واحد" : schedule.Quantity.ToString("0.##"),
                Time = schedule.DoseTime,
                residentMedicine.Instructions,
                dose.AdministeredAt,
                dose.Status
            }).ToListAsync(cancellationToken);

        return query.Select(x => new MedicationRowDto(
            x.FullName,
            x.Medicine,
            x.Dose,
            x.Time.ToString("HH:mm"),
            "فموي",
            x.AdministeredAt?.ToString("dd/MM HH:mm") ?? "-",
            DoseStatusText(x.Status),
            x.Status switch
            {
                DoseAdministrationStatus.Given => "",
                DoseAdministrationStatus.Missed => "danger",
                _ => "warn"
            })).ToList();
    }

    private async Task<List<InventoryRowDto>> LoadMedicineInventory(CancellationToken cancellationToken)
    {
        return await db.MedicineShelves.AsNoTracking()
            .Include(x => x.Medicine)
            .OrderBy(x => x.Medicine!.Name)
            .Select(x => new InventoryRowDto(
                x.Medicine!.Name + " " + x.Medicine.Strength,
                $"{x.QuantityOnHand:0} علبة",
                $"{x.ReorderThreshold:0} علبة",
                x.QuantityOnHand < x.ReorderThreshold ? "أقل من الحد الأدنى" : "مخزون جيد",
                x.QuantityOnHand < x.ReorderThreshold ? "danger" : ""))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<InventoryRowDto>> LoadKitchenInventory(CancellationToken cancellationToken)
    {
        return await db.KitchenInventoryItems.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new InventoryRowDto(
                x.Name,
                $"{x.QuantityOnHand:0.##} {x.Unit}",
                $"{x.ReorderThreshold:0.##} {x.Unit}",
                x.QuantityOnHand < x.ReorderThreshold ? "أقل من الحد الأدنى" : "ضمن الحد الآمن",
                x.QuantityOnHand < x.ReorderThreshold ? "danger" : ""))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SimpleRowDto>> LoadSpecialCases(CancellationToken cancellationToken)
    {
        return await (
            from plan in db.ResidentDietPlans.AsNoTracking()
            join resident in db.Residents.AsNoTracking() on plan.ResidentId equals resident.Id
            join diet in db.DietPlans.AsNoTracking() on plan.DietPlanId equals diet.Id
            select new SimpleRowDto(resident.FullName, diet.Name, diet.Name.Contains("السكر") ? "danger" : "warn"))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SimpleRowDto>> LoadSpecialMeals(CancellationToken cancellationToken)
    {
        return await (
            from plan in db.ResidentDietPlans.AsNoTracking()
            join resident in db.Residents.AsNoTracking() on plan.ResidentId equals resident.Id
            join diet in db.DietPlans.AsNoTracking() on plan.DietPlanId equals diet.Id
            select new SimpleRowDto(resident.FullName, diet.Name, ""))
            .ToListAsync(cancellationToken);
    }

    private int LowMedicineCount() => db.MedicineShelves.Count(x => x.QuantityOnHand < x.ReorderThreshold);
    private int LowKitchenCount() => db.KitchenInventoryItems.Count(x => x.QuantityOnHand < x.ReorderThreshold);
    private static int Percent(int value, int total) => total == 0 ? 0 : (int)Math.Round(value * 100m / total);
    private static decimal PercentDecimal(decimal value, decimal total) => total == 0 ? 0 : Math.Round(value * 100m / total, 1);
    private static string Money(decimal value) => value.ToString("#,0");
    private static string StatusText(ResidentStatus status) => status switch { ResidentStatus.Active => "مقيم بالدار", ResidentStatus.TemporarilyAway => "خارج مؤقتًا", ResidentStatus.Discharged => "غادر الدار", ResidentStatus.Deceased => "منتقل", _ => status.ToString() };
    private static string DoseStatusText(DoseAdministrationStatus status) => status switch { DoseAdministrationStatus.Given => "تم الإعطاء", DoseAdministrationStatus.Missed => "متأخرة", DoseAdministrationStatus.Skipped => "تم التخطي", _ => "قائمة" };
}
