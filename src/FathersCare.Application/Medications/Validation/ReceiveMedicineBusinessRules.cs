using FathersCare.Application.Abstractions;

namespace FathersCare.Application.Medications.Validation;

public sealed record MedicationValidationIssue(string Field, string MessageKey, string DefaultMessage);

public static class ReceiveMedicineBusinessRules
{
    public static IReadOnlyList<MedicationValidationIssue> Validate(ReceiveMedicineDto dto, DateOnly today)
    {
        var issues = new List<MedicationValidationIssue>();

        if (dto.ResidentId == Guid.Empty)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.ResidentId), "ValidationMedicationResidentRequired", "Resident is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.MedicineName))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.MedicineName), "ValidationMedicineNameRequired", "Medicine name is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.Form))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.Form), "ValidationMedicineFormRequired", "Medicine form is required."));
        }

        if (dto.QuantityReceived <= 0)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.QuantityReceived), "ValidationMedicineQuantityRequired", "Received quantity must be greater than zero."));
        }

        if (dto.ReceivedOn > today)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.ReceivedOn), "ValidationMedicineReceivedOnNotFuture", "Received date cannot be in the future."));
        }

        if (!dto.ExpiresOn.HasValue)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.ExpiresOn), "ValidationMedicineExpiryRequired", "Expiry date is required."));
        }
        else
        {
            if (dto.ExpiresOn.Value < today)
            {
                issues.Add(new MedicationValidationIssue(nameof(dto.ExpiresOn), "ValidationMedicineExpiryNotExpired", "Expired medicine cannot be accepted."));
            }

            if (dto.ExpiresOn.Value < dto.ReceivedOn)
            {
                issues.Add(new MedicationValidationIssue(nameof(dto.ExpiresOn), "ValidationMedicineExpiryAfterReceipt", "Expiry date must be on or after the received date."));
            }
        }

        if (string.IsNullOrWhiteSpace(dto.ReceivedFrom))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.ReceivedFrom), "ValidationMedicineReceivedFromRequired", "Received-from person is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.ReceivedBy))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.ReceivedBy), "ValidationMedicineReceivedByRequired", "Receiving staff member is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.PrescribedBy))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.PrescribedBy), "ValidationMedicinePrescribedByRequired", "Prescribing doctor is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.PrescriptionReference))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.PrescriptionReference), "ValidationMedicinePrescriptionReferenceRequired", "Prescription reference is required."));
        }

        if (!dto.PrescriptionDate.HasValue)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.PrescriptionDate), "ValidationMedicinePrescriptionDateRequired", "Prescription date is required."));
        }
        else if (dto.PrescriptionDate.Value > today)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.PrescriptionDate), "ValidationMedicinePrescriptionDateNotFuture", "Prescription date cannot be in the future."));
        }

        if (dto.StartsOn > dto.EndsOn)
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.EndsOn), "ValidationMedicinePlanDatesRequired", "Treatment plan end date must be on or after the start date."));
        }

        if (string.IsNullOrWhiteSpace(dto.Instructions))
        {
            issues.Add(new MedicationValidationIssue(nameof(dto.Instructions), "ValidationMedicineInstructionsRequired", "Prescription instructions are required."));
        }

        var validDoses = dto.Doses
            .Where(dose => !string.IsNullOrWhiteSpace(dose.Time) || dose.Quantity > 0)
            .ToList();

        if (validDoses.Count == 0)
        {
            issues.Add(new MedicationValidationIssue("Doses", "ValidationMedicineDoseRequired", "At least one dose time is required."));
        }

        var parsedTimes = new HashSet<TimeOnly>();
        foreach (var dose in validDoses)
        {
            if (!TimeOnly.TryParse(dose.Time, out var parsedTime))
            {
                issues.Add(new MedicationValidationIssue("DoseTime", "ValidationMedicineDoseTimeRequired", "Dose time must be a valid time."));
            }
            else if (!parsedTimes.Add(parsedTime))
            {
                issues.Add(new MedicationValidationIssue("DoseTime", "ValidationMedicineDoseTimeUnique", "Dose times must be unique."));
            }

            if (dose.Quantity <= 0)
            {
                issues.Add(new MedicationValidationIssue("DoseQuantity", "ValidationMedicineDoseQuantityRequired", "Dose quantity must be greater than zero."));
            }
        }

        var days = dto.EndsOn.DayNumber - dto.StartsOn.DayNumber + 1;
        if (days > 0)
        {
            var plannedQuantity = validDoses.Where(dose => dose.Quantity > 0).Sum(dose => dose.Quantity) * days;
            if (plannedQuantity > dto.QuantityReceived)
            {
                issues.Add(new MedicationValidationIssue(nameof(dto.QuantityReceived), "ValidationMedicineQuantityCoversPlan", "Received quantity must cover the treatment plan period."));
            }
        }

        return issues;
    }
}
