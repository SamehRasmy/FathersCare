using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeMedicationArabicDisplayText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicineBatches
                SET ReceivedBy = N'ممرضة الدار'
                WHERE ReceivedBy IN (N'???? ??????? - ????? ?????', N'ممرضة الدار - إدخال من روشتة مصورة');

                UPDATE ResidentMedicines
                SET Instructions = REPLACE(Instructions, N' | Deleted: ', N' | تم الحذف: ')
                WHERE Instructions LIKE N'% | Deleted: %';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicineBatches
                SET ReceivedBy = N'ممرضة الدار - إدخال من روشتة مصورة'
                WHERE ReceivedBy = N'ممرضة الدار';

                UPDATE ResidentMedicines
                SET Instructions = REPLACE(Instructions, N' | تم الحذف: ', N' | Deleted: ')
                WHERE Instructions LIKE N'% | تم الحذف: %';
                """);
        }
    }
}
