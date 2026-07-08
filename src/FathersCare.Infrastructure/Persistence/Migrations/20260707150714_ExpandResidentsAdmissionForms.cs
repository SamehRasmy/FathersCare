using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandResidentsAdmissionForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "ResidentContacts");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoPath",
                table: "Residents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInformation",
                table: "Residents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AdmissionDate",
                table: "Residents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdmissionStatus",
                table: "Residents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BirthPlace",
                table: "Residents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanionName",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentAddress",
                table: "Residents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentFloorId",
                table: "Residents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Denomination",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameArabic",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameEnglish",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Residents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdOrPassportIssueAuthority",
                table: "Residents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "Residents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Residents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Residents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "Residents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousJob",
                table: "Residents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidencyType",
                table: "Residents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonAddress",
                table: "Residents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonMobile",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonName",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonPhone",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonRelationship",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonWorkAddress",
                table: "Residents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomGrade",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonAddress",
                table: "Residents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonMobile",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonName",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonPhone",
                table: "Residents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonRelationship",
                table: "Residents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondResponsiblePersonWorkAddress",
                table: "Residents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatingDoctorName",
                table: "Residents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChronicConditionsSummary",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AllergiesSummary",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllergyDetails",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MedicalDeclarationConfirmed",
                table: "ResidentMedicalProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicalDeclarationConfirmedBy",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MedicalDeclarationDate",
                table: "ResidentMedicalProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousInjuriesOrAccidents",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSurgeries",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "ResidentDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ResidentDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentType",
                table: "ResidentDocuments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ResidentDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                table: "ResidentDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ResidentDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfidential",
                table: "ResidentDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ResidentDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ResidentDocuments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "ResidentDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Relationship",
                table: "ResidentContacts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ResidentContacts",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "ResidentContacts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyContact",
                table: "ResidentContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Job",
                table: "ResidentContacts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "ResidentContacts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ResidentContacts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ResidentContacts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResidentMedicalConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResidentMedicalProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConditionCode = table.Column<int>(type: "int", nullable: false),
                    ConditionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HasCondition = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ResidentMedicalConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentMedicalConditions_ResidentMedicalProfiles_ResidentMedicalProfileId",
                        column: x => x.ResidentMedicalProfileId,
                        principalTable: "ResidentMedicalProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentMedicalConditions_ResidentMedicalProfileId",
                table: "ResidentMedicalConditions",
                column: "ResidentMedicalProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResidentMedicalConditions");

            migrationBuilder.DropColumn(
                name: "AdditionalInformation",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "AdmissionDate",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "AdmissionStatus",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "BirthPlace",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "CompanionName",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "CurrentAddress",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "CurrentFloorId",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Denomination",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "FullNameArabic",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "FullNameEnglish",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "IdOrPassportIssueAuthority",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "PreviousJob",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResidencyType",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonAddress",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonMobile",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonName",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonPhone",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonRelationship",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonWorkAddress",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "RoomGrade",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonAddress",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonMobile",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonName",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonPhone",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonRelationship",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SecondResponsiblePersonWorkAddress",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "TreatingDoctorName",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "AllergyDetails",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "MedicalDeclarationConfirmed",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "MedicalDeclarationConfirmedBy",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "MedicalDeclarationDate",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "PreviousInjuriesOrAccidents",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "PreviousSurgeries",
                table: "ResidentMedicalProfiles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "IsConfidential",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "ResidentDocuments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "IsEmergencyContact",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "Job",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ResidentContacts");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ResidentContacts");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoPath",
                table: "Residents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChronicConditionsSummary",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AllergiesSummary",
                table: "ResidentMedicalProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "ResidentDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ResidentDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "ResidentDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Relationship",
                table: "ResidentContacts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ResidentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ResidentContacts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "ResidentContacts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
