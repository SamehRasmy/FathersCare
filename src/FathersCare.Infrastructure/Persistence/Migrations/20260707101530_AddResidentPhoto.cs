using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Residents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Residents");
        }
    }
}
