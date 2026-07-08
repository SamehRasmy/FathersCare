using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationModuleChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId",
                table: "ResidentMedicines",
                columns: new[] { "TenantId", "ResidentId", "MedicineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineShelves_TenantId_MedicineId",
                table: "MedicineShelves",
                columns: new[] { "TenantId", "MedicineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineSchedules_TenantId_ResidentMedicineId_DoseTime",
                table: "MedicineSchedules",
                columns: new[] { "TenantId", "ResidentMedicineId", "DoseTime" });

            migrationBuilder.CreateIndex(
                name: "IX_DoseAdministrations_TenantId_MedicineScheduleId_DoseDate_Status",
                table: "DoseAdministrations",
                columns: new[] { "TenantId", "MedicineScheduleId", "DoseDate", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId",
                table: "ResidentMedicines");

            migrationBuilder.DropIndex(
                name: "IX_MedicineShelves_TenantId_MedicineId",
                table: "MedicineShelves");

            migrationBuilder.DropIndex(
                name: "IX_MedicineSchedules_TenantId_ResidentMedicineId_DoseTime",
                table: "MedicineSchedules");

            migrationBuilder.DropIndex(
                name: "IX_DoseAdministrations_TenantId_MedicineScheduleId_DoseDate_Status",
                table: "DoseAdministrations");
        }
    }
}
