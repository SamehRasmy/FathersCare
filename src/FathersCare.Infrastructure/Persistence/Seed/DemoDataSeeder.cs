using FathersCare.Domain.Finance;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Nutrition;
using FathersCare.Domain.Residents;
using FathersCare.Domain.Rooms;
using FathersCare.Domain.Staff;
using FathersCare.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FathersCare.Infrastructure.Persistence.Seed;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Tenants.AnyAsync(cancellationToken))
        {
            return;
        }

        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "دار القديسين استفانوس",
            LegalName = "جمعية رعاية أبناء الكنيسة",
            CreatedAt = now
        };

        var floors = new[]
        {
            NewFloor(tenantId, "الدور الأرضي", 0, now),
            NewFloor(tenantId, "الدور الأول", 1, now),
            NewFloor(tenantId, "الدور الثاني", 2, now)
        };

        var rooms = new List<Room>();
        foreach (var number in new[] { "101", "102", "103", "104", "110", "111", "112", "113" })
        {
            rooms.Add(NewRoom(tenantId, floors[1].Id, number, 2, now));
        }

        foreach (var number in new[] { "210", "211", "212", "213", "216", "217", "218", "219" })
        {
            rooms.Add(NewRoom(tenantId, floors[2].Id, number, 2, now));
        }

        foreach (var number in new[] { "301", "302", "303", "304", "305", "306", "307", "308", "309" })
        {
            rooms.Add(NewRoom(tenantId, floors[2].Id, number, 1, now));
        }

        var residents = new[]
        {
            NewResident(tenantId, "R-001", "الأب رزق الله ميخائيل", new DateOnly(1950, 5, 15), rooms.Single(room => room.Number == "111").Id, now),
            NewResident(tenantId, "R-002", "الأب يوسف بشوي", new DateOnly(1948, 9, 4), rooms.Single(room => room.Number == "103").Id, now),
            NewResident(tenantId, "R-003", "الأب جرجس مرقص", new DateOnly(1952, 2, 11), rooms.Single(room => room.Number == "213").Id, now),
            NewResident(tenantId, "R-004", "الأب فادي مينا", new DateOnly(1955, 12, 7), rooms.Single(room => room.Number == "101").Id, now)
        };

        var contacts = new[]
        {
            NewContact(tenantId, residents[0].Id, "مينا رزق الله", "أخ", "0100 123 4567", "القاهرة - مصر الجديدة", true, now),
            NewContact(tenantId, residents[0].Id, "ماريا مينا", "أخت", "0111 987 6543", "القاهرة - مصر الجديدة", false, now),
            NewContact(tenantId, residents[0].Id, "فادي مينا", "ابن أخ", "0122 456 7890", "الجيزة - الدقي", false, now)
        };

        var medicalProfiles = new[]
        {
            NewMedicalProfile(tenantId, residents[0].Id, "O+", "حساسية من دواء واحد", "ضغط وسكر", "الحالة مستقرة بفضل الله.", now),
            NewMedicalProfile(tenantId, residents[1].Id, "A+", "لا يوجد", "سكر", "نظام غذائي خاص.", now),
            NewMedicalProfile(tenantId, residents[2].Id, "B+", "لا يوجد", "ضغط", "يحتاج وجبات مهروسة.", now)
        };

        var medicines = new[]
        {
            NewMedicine(tenantId, "Concor", "5 mg", "قرص", now),
            NewMedicine(tenantId, "Metformin", "500 mg", "قرص", now),
            NewMedicine(tenantId, "Lasix", "40 mg", "قرص", now),
            NewMedicine(tenantId, "Amlodipine", "5 mg", "قرص", now),
            NewMedicine(tenantId, "Omeprazole", "20 mg", "كبسولة", now),
            NewMedicine(tenantId, "Vitamin D3", "1000 IU", "كبسولة", now)
        };

        var residentMedicines = new[]
        {
            NewResidentMedicine(tenantId, residents[0].Id, medicines[0].Id, "قبل الإفطار بـ 30 دقيقة", now),
            NewResidentMedicine(tenantId, residents[1].Id, medicines[1].Id, "بعد الإفطار", now),
            NewResidentMedicine(tenantId, residents[2].Id, medicines[2].Id, "بعد الغذاء", now),
            NewResidentMedicine(tenantId, residents[3].Id, medicines[4].Id, "قبل الأكل", now)
        };

        var schedules = new[]
        {
            NewSchedule(tenantId, residentMedicines[0].Id, new TimeOnly(8, 0), 1, DoseTiming.BeforeMeal, now),
            NewSchedule(tenantId, residentMedicines[1].Id, new TimeOnly(9, 15), 1, DoseTiming.AfterMeal, now),
            NewSchedule(tenantId, residentMedicines[2].Id, new TimeOnly(13, 0), 1, DoseTiming.AfterMeal, now),
            NewSchedule(tenantId, residentMedicines[3].Id, new TimeOnly(7, 0), 1, DoseTiming.BeforeMeal, now)
        };

        var doseAdministrations = new[]
        {
            NewDose(tenantId, schedules[0].Id, today, DoseAdministrationStatus.Given, now.AddHours(-1), "مريم بولس", now),
            NewDose(tenantId, schedules[1].Id, today, DoseAdministrationStatus.Given, now.AddMinutes(-35), "مريم بولس", now),
            NewDose(tenantId, schedules[2].Id, today, DoseAdministrationStatus.Scheduled, null, null, now),
            NewDose(tenantId, schedules[3].Id, today, DoseAdministrationStatus.Missed, null, null, now)
        };

        var medicineShelves = new[]
        {
            NewMedicineShelf(tenantId, medicines[0].Id, "A-01", 18, 10, now),
            NewMedicineShelf(tenantId, medicines[1].Id, "A-02", 25, 10, now),
            NewMedicineShelf(tenantId, medicines[2].Id, "B-01", 8, 12, now),
            NewMedicineShelf(tenantId, medicines[4].Id, "B-03", 15, 10, now)
        };

        var meals = new[]
        {
            NewMeal(tenantId, "إفطار عادي", "إفطار", now),
            NewMeal(tenantId, "إفطار قليل الملح", "إفطار خاص", now),
            NewMeal(tenantId, "إفطار مرضى السكر", "إفطار خاص", now),
            NewMeal(tenantId, "غداء مهروس", "غداء خاص", now)
        };

        var dietPlans = new[]
        {
            NewDietPlan(tenantId, "نظام عادي", null, now),
            NewDietPlan(tenantId, "نظام قليل الملح", "متابعة ضغط الدم", now),
            NewDietPlan(tenantId, "نظام مرضى السكر", "بدون سكر مضاف", now),
            NewDietPlan(tenantId, "وجبات مهروسة", "قوام ناعم", now)
        };

        var residentDietPlans = new[]
        {
            NewResidentDietPlan(tenantId, residents[0].Id, dietPlans[1].Id, today.AddDays(-30), now),
            NewResidentDietPlan(tenantId, residents[1].Id, dietPlans[2].Id, today.AddDays(-20), now),
            NewResidentDietPlan(tenantId, residents[2].Id, dietPlans[3].Id, today.AddDays(-10), now)
        };

        var mealDistributions = new[]
        {
            NewMealDistribution(tenantId, residents[0].Id, meals[1].Id, today, true, now.AddHours(-1), now),
            NewMealDistribution(tenantId, residents[1].Id, meals[2].Id, today, true, now.AddHours(-1), now),
            NewMealDistribution(tenantId, residents[2].Id, meals[3].Id, today, false, null, now),
            NewMealDistribution(tenantId, residents[3].Id, meals[0].Id, today, true, now.AddHours(-1), now)
        };

        var kitchenInventory = new[]
        {
            NewKitchenItem(tenantId, "صدور دجاج", "كجم", 10, 15, now),
            NewKitchenItem(tenantId, "أرز مصري", "كجم", 15, 10, now),
            NewKitchenItem(tenantId, "عدس", "كجم", 5, 8, now),
            NewKitchenItem(tenantId, "حليب خالي الدسم", "لتر", 6, 8, now)
        };

        var revenues = new[]
        {
            NewRevenue(tenantId, today.AddDays(-10), "رسوم الإقامة الشهرية", 1142000, now),
            NewRevenue(tenantId, today.AddDays(-8), "رسوم الخدمات الطبية", 68750, now),
            NewRevenue(tenantId, today.AddDays(-5), "الأنشطة والبرامج", 23800, now),
            NewRevenue(tenantId, today.AddDays(-3), "تبرعات نقدية", 22900, now)
        };

        var expenses = new[]
        {
            NewExpense(tenantId, today.AddDays(-10), "الرواتب والأجور", 586300, now),
            NewExpense(tenantId, today.AddDays(-8), "الطعام والتغذية", 238450, now),
            NewExpense(tenantId, today.AddDays(-5), "الخدمات الطبية والأدوية", 187600, now),
            NewExpense(tenantId, today.AddDays(-3), "الصيانة والمرافق", 96800, now),
            NewExpense(tenantId, today.AddDays(-2), "مصروفات عامة", 89480, now)
        };

        var staff = new[]
        {
            NewStaff(tenantId, "مريم بولس", "0100 111 2222", StaffRole.Nurse, now),
            NewStaff(tenantId, "هاني سامي", "0100 333 4444", StaffRole.Kitchen, now)
        };

        db.Add(tenant);
        db.AddRange(floors);
        db.AddRange(rooms);
        db.AddRange(residents);
        db.AddRange(contacts);
        db.AddRange(medicalProfiles);
        db.AddRange(medicines);
        db.AddRange(residentMedicines);
        db.AddRange(schedules);
        db.AddRange(doseAdministrations);
        db.AddRange(medicineShelves);
        db.AddRange(meals);
        db.AddRange(dietPlans);
        db.AddRange(residentDietPlans);
        db.AddRange(mealDistributions);
        db.AddRange(kitchenInventory);
        db.AddRange(revenues);
        db.AddRange(expenses);
        db.AddRange(staff);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Floor NewFloor(Guid tenantId, string name, int number, DateTimeOffset now) => new() { TenantId = tenantId, Name = name, Number = number, CreatedAt = now };
    private static Room NewRoom(Guid tenantId, Guid floorId, string number, int capacity, DateTimeOffset now) => new() { TenantId = tenantId, FloorId = floorId, Number = number, Capacity = capacity, CreatedAt = now };
    private static Resident NewResident(Guid tenantId, string code, string name, DateOnly birthDate, Guid roomId, DateTimeOffset now) => new() { TenantId = tenantId, Code = code, FullName = name, BirthDate = birthDate, CurrentRoomId = roomId, CreatedAt = now };
    private static ResidentContact NewContact(Guid tenantId, Guid residentId, string name, string relationship, string phone, string address, bool primary, DateTimeOffset now) => new() { TenantId = tenantId, ResidentId = residentId, FullName = name, Relationship = relationship, PhoneNumber = phone, MobileNumber = phone, Address = address, IsPrimary = primary, CreatedAt = now };
    private static ResidentMedicalProfile NewMedicalProfile(Guid tenantId, Guid residentId, string bloodType, string allergies, string conditions, string notes, DateTimeOffset now) => new() { TenantId = tenantId, ResidentId = residentId, BloodType = bloodType, AllergiesSummary = allergies, ChronicConditionsSummary = conditions, Notes = notes, CreatedAt = now };
    private static Medicine NewMedicine(Guid tenantId, string name, string strength, string form, DateTimeOffset now) => new() { TenantId = tenantId, Name = name, Strength = strength, Form = form, CreatedAt = now };
    private static ResidentMedicine NewResidentMedicine(Guid tenantId, Guid residentId, Guid medicineId, string instructions, DateTimeOffset now) => new() { TenantId = tenantId, ResidentId = residentId, MedicineId = medicineId, Instructions = instructions, CreatedAt = now };
    private static MedicineSchedule NewSchedule(Guid tenantId, Guid residentMedicineId, TimeOnly time, decimal quantity, DoseTiming timing, DateTimeOffset now) => new() { TenantId = tenantId, ResidentMedicineId = residentMedicineId, DoseTime = time, Quantity = quantity, Timing = timing, CreatedAt = now };
    private static DoseAdministration NewDose(Guid tenantId, Guid scheduleId, DateOnly date, DoseAdministrationStatus status, DateTimeOffset? administeredAt, string? administeredBy, DateTimeOffset now) => new() { TenantId = tenantId, MedicineScheduleId = scheduleId, DoseDate = date, Status = status, AdministeredAt = administeredAt, AdministeredBy = administeredBy, CreatedAt = now };
    private static MedicineShelf NewMedicineShelf(Guid tenantId, Guid medicineId, string location, decimal quantity, decimal threshold, DateTimeOffset now) => new() { TenantId = tenantId, MedicineId = medicineId, LocationCode = location, QuantityOnHand = quantity, ReorderThreshold = threshold, CreatedAt = now };
    private static Meal NewMeal(Guid tenantId, string name, string mealType, DateTimeOffset now) => new() { TenantId = tenantId, Name = name, MealType = mealType, CreatedAt = now };
    private static DietPlan NewDietPlan(Guid tenantId, string name, string? notes, DateTimeOffset now) => new() { TenantId = tenantId, Name = name, Notes = notes, CreatedAt = now };
    private static ResidentDietPlan NewResidentDietPlan(Guid tenantId, Guid residentId, Guid dietPlanId, DateOnly startsOn, DateTimeOffset now) => new() { TenantId = tenantId, ResidentId = residentId, DietPlanId = dietPlanId, StartsOn = startsOn, CreatedAt = now };
    private static MealDistribution NewMealDistribution(Guid tenantId, Guid residentId, Guid mealId, DateOnly date, bool delivered, DateTimeOffset? deliveredAt, DateTimeOffset now) => new() { TenantId = tenantId, ResidentId = residentId, MealId = mealId, DistributionDate = date, Delivered = delivered, DeliveredAt = deliveredAt, CreatedAt = now };
    private static KitchenInventoryItem NewKitchenItem(Guid tenantId, string name, string unit, decimal quantity, decimal threshold, DateTimeOffset now) => new() { TenantId = tenantId, Name = name, Unit = unit, QuantityOnHand = quantity, ReorderThreshold = threshold, CreatedAt = now };
    private static Revenue NewRevenue(Guid tenantId, DateOnly date, string category, decimal amount, DateTimeOffset now) => new() { TenantId = tenantId, Date = date, Category = category, Amount = amount, CreatedAt = now };
    private static Expense NewExpense(Guid tenantId, DateOnly date, string category, decimal amount, DateTimeOffset now) => new() { TenantId = tenantId, Date = date, Category = category, Amount = amount, CreatedAt = now };
    private static StaffMember NewStaff(Guid tenantId, string name, string phone, StaffRole role, DateTimeOffset now) => new() { TenantId = tenantId, FullName = name, Phone = phone, Role = role, CreatedAt = now };
}
