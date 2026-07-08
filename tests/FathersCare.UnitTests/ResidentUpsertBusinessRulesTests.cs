using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Validation;
using FathersCare.Domain.Residents;

namespace FathersCare.UnitTests;

public class ResidentUpsertBusinessRulesTests
{
    [Fact]
    public void Validate_ReturnsNoIssues_WhenResidentRecordIsOperationallyComplete()
    {
        var dto = BuildValidResident();

        var issues = ResidentUpsertBusinessRules.Validate(dto);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ReturnsExpectedIssues_WhenCoreResidentDataIsMissing()
    {
        var dto = new ResidentUpsertDto();

        var issues = ResidentUpsertBusinessRules.Validate(dto);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationResidentCodeRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationFullNameRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationBirthDateRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationAdmissionDateRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationCurrentAddressRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationIdentityDocumentRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationPrimaryContactRequired");
    }

    [Fact]
    public void Validate_ReturnsRoomIssue_WhenResidentIsActiveAndAdmittedWithoutRoom()
    {
        var dto = BuildValidResident(includeRoom: false);
        var issues = ResidentUpsertBusinessRules.Validate(dto);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationRoomRequiredForActiveAdmission");
    }

    private static ResidentUpsertDto BuildValidResident(bool includeRoom = true) =>
        new()
        {
            Code = "R-100",
            FullName = "Resident Name",
            BirthDate = new DateOnly(1955, 2, 25),
            Gender = ResidentGender.Male.ToString(),
            CurrentAddress = "Cairo",
            NationalId = "12345678901234",
            AdmissionDate = new DateOnly(2026, 7, 8),
            AdmissionStatus = AdmissionStatus.Admitted.ToString(),
            Status = ResidentStatus.Active.ToString(),
            RoomId = includeRoom ? Guid.NewGuid() : null,
            Contacts =
            [
                new ResidentContactEditorDto(
                    null,
                    "Primary Contact",
                    "Son",
                    string.Empty,
                    "Cairo",
                    "01000000000",
                    string.Empty,
                    true,
                    true,
                    string.Empty)
            ]
        };
}
