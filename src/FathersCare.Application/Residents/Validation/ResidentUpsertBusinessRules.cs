using FathersCare.Application.Abstractions;
using FathersCare.Domain.Residents;

namespace FathersCare.Application.Residents.Validation;

public sealed record ResidentValidationIssue(string Field, string MessageKey, string DefaultMessage);

public static class ResidentUpsertBusinessRules
{
    public static IReadOnlyList<ResidentValidationIssue> Validate(ResidentUpsertDto dto)
    {
        var issues = new List<ResidentValidationIssue>();

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.Code), "ValidationResidentCodeRequired", "Resident code is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.FullName), "ValidationFullNameRequired", "Full name is required."));
        }

        if (!dto.BirthDate.HasValue)
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.BirthDate), "ValidationBirthDateRequired", "Birth date is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.Gender))
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.Gender), "ValidationGenderRequired", "Gender is required."));
        }

        if (!dto.AdmissionDate.HasValue)
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.AdmissionDate), "ValidationAdmissionDateRequired", "Admission date is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.CurrentAddress))
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.CurrentAddress), "ValidationCurrentAddressRequired", "Current address is required."));
        }

        if (string.IsNullOrWhiteSpace(dto.NationalId) && string.IsNullOrWhiteSpace(dto.PassportNumber))
        {
            issues.Add(new ResidentValidationIssue("IdentityDocument", "ValidationIdentityDocumentRequired", "National ID or passport number is required."));
        }

        var normalizedContacts = dto.Contacts
            .Where(contact =>
                !string.IsNullOrWhiteSpace(contact.FullName) ||
                !string.IsNullOrWhiteSpace(contact.Relationship) ||
                !string.IsNullOrWhiteSpace(contact.Job) ||
                !string.IsNullOrWhiteSpace(contact.Address) ||
                !string.IsNullOrWhiteSpace(contact.PhoneNumber) ||
                !string.IsNullOrWhiteSpace(contact.MobileNumber) ||
                !string.IsNullOrWhiteSpace(contact.Notes))
            .ToList();

        if (normalizedContacts.Count == 0)
        {
            issues.Add(new ResidentValidationIssue("Contacts", "ValidationPrimaryContactRequired", "At least one family or emergency contact is required."));
        }
        else
        {
            var primaryContact = normalizedContacts.FirstOrDefault(contact => contact.IsPrimary) ?? normalizedContacts[0];

            if (string.IsNullOrWhiteSpace(primaryContact.FullName))
            {
                issues.Add(new ResidentValidationIssue("PrimaryContactFullName", "ValidationPrimaryContactNameRequired", "Primary contact name is required."));
            }

            if (string.IsNullOrWhiteSpace(primaryContact.Relationship))
            {
                issues.Add(new ResidentValidationIssue("PrimaryContactRelationship", "ValidationPrimaryContactRelationshipRequired", "Primary contact relationship is required."));
            }

            if (string.IsNullOrWhiteSpace(primaryContact.PhoneNumber) && string.IsNullOrWhiteSpace(primaryContact.MobileNumber))
            {
                issues.Add(new ResidentValidationIssue("PrimaryContactPhone", "ValidationPrimaryContactPhoneRequired", "Primary contact phone or mobile number is required."));
            }
        }

        if (Enum.TryParse<AdmissionStatus>(dto.AdmissionStatus, out var admissionStatus) &&
            Enum.TryParse<ResidentStatus>(dto.Status, out var residentStatus) &&
            admissionStatus == AdmissionStatus.Admitted &&
            residentStatus == ResidentStatus.Active &&
            !dto.RoomId.HasValue)
        {
            issues.Add(new ResidentValidationIssue(nameof(dto.RoomId), "ValidationRoomRequiredForActiveAdmission", "Room assignment is required for an active admitted resident."));
        }

        return issues;
    }
}
