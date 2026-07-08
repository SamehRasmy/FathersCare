using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMedicationReceivedByArabic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicineBatches
                SET ReceivedBy = N'ممرضة الدار - إدخال من روشتة مصورة'
                WHERE ReceivedBy = N'???? ??????? - ????? ?????';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicineBatches
                SET ReceivedBy = N'???? ??????? - ????? ?????'
                WHERE ReceivedBy = N'ممرضة الدار - إدخال من روشتة مصورة';
                """);
        }
    }
}
