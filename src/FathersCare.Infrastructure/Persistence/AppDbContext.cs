using FathersCare.Domain.Finance;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;
using FathersCare.Domain.Nutrition;
using FathersCare.Domain.Residents;
using FathersCare.Domain.Rooms;
using FathersCare.Domain.Staff;
using FathersCare.Domain.Tenancy;
using FathersCare.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FathersCare.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<ResidentContact> ResidentContacts => Set<ResidentContact>();
    public DbSet<ResidentDocument> ResidentDocuments => Set<ResidentDocument>();
    public DbSet<ResidentMedicalProfile> ResidentMedicalProfiles => Set<ResidentMedicalProfile>();
    public DbSet<ResidentMedicalCondition> ResidentMedicalConditions => Set<ResidentMedicalCondition>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<ResidentMedicine> ResidentMedicines => Set<ResidentMedicine>();
    public DbSet<ResidentMedicineBatch> ResidentMedicineBatches => Set<ResidentMedicineBatch>();
    public DbSet<MedicineSchedule> MedicineSchedules => Set<MedicineSchedule>();
    public DbSet<DoseAdministration> DoseAdministrations => Set<DoseAdministration>();
    public DbSet<MedicineShelf> MedicineShelves => Set<MedicineShelf>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<DietPlan> DietPlans => Set<DietPlan>();
    public DbSet<ResidentDietPlan> ResidentDietPlans => Set<ResidentDietPlan>();
    public DbSet<MealDistribution> MealDistributions => Set<MealDistribution>();
    public DbSet<KitchenInventoryItem> KitchenInventoryItems => Set<KitchenInventoryItem>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomOccupancy> RoomOccupancies => Set<RoomOccupancy>();
    public DbSet<RoomMaintenanceRequest> RoomMaintenanceRequests => Set<RoomMaintenanceRequest>();
    public DbSet<Revenue> Revenues => Set<Revenue>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<OperatingSummary> OperatingSummaries => Set<OperatingSummary>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
