using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentMedicationReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId",
                table: "ResidentMedicines");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "ResidentMedicines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndsOn",
                table: "ResidentMedicines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescribedBy",
                table: "ResidentMedicines",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PrescriptionDate",
                table: "ResidentMedicines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionReference",
                table: "ResidentMedicines",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartsOn",
                table: "ResidentMedicines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResidentMedicineBatchId",
                table: "DoseAdministrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResidentMedicineBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResidentMedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    PrescriptionReference = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentMedicineBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentMedicineBatches_ResidentMedicines_ResidentMedicineId",
                        column: x => x.ResidentMedicineId,
                        principalTable: "ResidentMedicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId_IsActive",
                table: "ResidentMedicines",
                columns: new[] { "TenantId", "ResidentId", "MedicineId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DoseAdministrations_ResidentMedicineBatchId",
                table: "DoseAdministrations",
                column: "ResidentMedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicineBatches_ResidentMedicineId",
                table: "ResidentMedicineBatches",
                column: "ResidentMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicineBatches_TenantId_ResidentId_ResidentMedicineId_ExpiresOn",
                table: "ResidentMedicineBatches",
                columns: new[] { "TenantId", "ResidentId", "ResidentMedicineId", "ExpiresOn" });

            migrationBuilder.AddForeignKey(
                name: "FK_DoseAdministrations_ResidentMedicineBatches_ResidentMedicineBatchId",
                table: "DoseAdministrations",
                column: "ResidentMedicineBatchId",
                principalTable: "ResidentMedicineBatches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoseAdministrations_ResidentMedicineBatches_ResidentMedicineBatchId",
                table: "DoseAdministrations");

            migrationBuilder.DropTable(
                name: "ResidentMedicineBatches");

            migrationBuilder.DropIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId_IsActive",
                table: "ResidentMedicines");

            migrationBuilder.DropIndex(
                name: "IX_DoseAdministrations_ResidentMedicineBatchId",
                table: "DoseAdministrations");

            migrationBuilder.DropColumn(
                name: "EndsOn",
                table: "ResidentMedicines");

            migrationBuilder.DropColumn(
                name: "PrescribedBy",
                table: "ResidentMedicines");

            migrationBuilder.DropColumn(
                name: "PrescriptionDate",
                table: "ResidentMedicines");

            migrationBuilder.DropColumn(
                name: "PrescriptionReference",
                table: "ResidentMedicines");

            migrationBuilder.DropColumn(
                name: "StartsOn",
                table: "ResidentMedicines");

            migrationBuilder.DropColumn(
                name: "ResidentMedicineBatchId",
                table: "DoseAdministrations");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "ResidentMedicines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicines_TenantId_ResidentId_MedicineId",
                table: "ResidentMedicines",
                columns: new[] { "TenantId", "ResidentId", "MedicineId" },
                unique: true);
        }
    }
}
