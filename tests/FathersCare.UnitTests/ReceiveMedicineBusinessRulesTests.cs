using FathersCare.Application.Abstractions;
using FathersCare.Application.Medications.Validation;

namespace FathersCare.UnitTests;

public class ReceiveMedicineBusinessRulesTests
{
    private static readonly DateOnly Today = new(2026, 7, 8);

    [Fact]
    public void Validate_ReturnsNoIssues_WhenReceiptAndDosePlanAreComplete()
    {
        var dto = BuildValidReceipt();

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ReturnsExpectedIssues_WhenCoreReceiptDataIsMissing()
    {
        var dto = new ReceiveMedicineDto(
            Guid.Empty,
            "",
            "",
            "",
            0,
            Today.AddDays(1),
            null,
            "",
            "",
            "",
            "",
            null,
            Today,
            Today,
            "",
            []);

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicationResidentRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineNameRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineQuantityRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineExpiryRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicinePrescribedByRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineDoseRequired");
    }

    [Fact]
    public void Validate_ReturnsQuantityCoverageIssue_WhenReceivedQuantityCannotCoverPlan()
    {
        var dto = BuildValidReceipt(quantityReceived: 5);

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineQuantityCoversPlan");
    }

    [Fact]
    public void Validate_ReturnsExpiryIssue_WhenMedicineIsExpired()
    {
        var dto = BuildValidReceipt(expiresOn: Today.AddDays(-1));

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineExpiryNotExpired");
    }

    [Fact]
    public void Validate_ReturnsDoseIssues_WhenDoseTimeIsInvalidOrRepeated()
    {
        var dto = BuildValidReceipt(doses:
        [
            new DosePlanInputDto("bad-time", 1, "AfterMeal"),
            new DosePlanInputDto("08:00", 1, "AfterMeal"),
            new DosePlanInputDto("08:00", 1, "AfterMeal")
        ]);

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineDoseTimeRequired");
        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineDoseTimeUnique");
    }

    [Fact]
    public void Validate_ReturnsDoseQuantityIssue_WhenDoseQuantityIsZero()
    {
        var dto = BuildValidReceipt(doses: [new DosePlanInputDto("08:00", 0, "AfterMeal")]);

        var issues = ReceiveMedicineBusinessRules.Validate(dto, Today);

        Assert.Contains(issues, issue => issue.MessageKey == "ValidationMedicineDoseQuantityRequired");
    }

    private static ReceiveMedicineDto BuildValidReceipt(
        decimal quantityReceived = 31,
        DateOnly? expiresOn = null,
        IReadOnlyList<DosePlanInputDto>? doses = null) =>
        new(
            Guid.NewGuid(),
            "Concor",
            "5 mg",
            "Tablet",
            quantityReceived,
            Today,
            expiresOn ?? Today.AddMonths(6),
            "Family",
            "Nurse",
            "Dr. Mina",
            "RX-100",
            Today,
            Today,
            Today.AddDays(30),
            "After breakfast",
            doses ?? [new DosePlanInputDto("08:00", 1, "AfterMeal")]);
}
