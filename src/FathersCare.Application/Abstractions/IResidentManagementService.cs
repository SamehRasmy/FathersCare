using FathersCare.Domain.Residents;

namespace FathersCare.Application.Abstractions;

public interface IResidentManagementService
{
    Task<ResidentsWorkspaceViewModel> GetWorkspaceAsync(string? search = null, string? status = null, CancellationToken cancellationToken = default);
    Task<ManagedResidentDetailsViewModel?> GetDetailsAsync(Guid residentId, CancellationToken cancellationToken = default);
    Task<ResidentEditorViewModel> GetEditorAsync(Guid? residentId = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(ResidentUpsertDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid residentId, ResidentUpsertDto dto, CancellationToken cancellationToken = default);
    Task<string> UploadPhotoAsync(Guid residentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<ResidentDocumentViewModel> UploadDocumentAsync(Guid residentId, ResidentDocumentUploadDto dto, Stream content, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(Guid residentId, Guid documentId, CancellationToken cancellationToken = default);
}

public sealed record ManagedResidentSummaryDto(
    Guid Id,
    string Code,
    string FullName,
    string PhotoPath,
    string MobilePhone,
    string Age,
    string Status,
    string StatusState,
    string Room,
    string BloodType,
    string PrimaryContact,
    int DailyMedicationCount);

public sealed record ManagedRoomSummaryDto(
    Guid Id,
    string Number,
    string Floor,
    int Capacity,
    int Occupied,
    string State);

public sealed record ResidentEditorOptionDto(Guid Id, string Label);

public sealed record ResidentContactEditorDto(
    Guid? Id,
    string FullName,
    string Relationship,
    string Job,
    string Address,
    string PhoneNumber,
    string MobileNumber,
    bool IsPrimary,
    bool IsEmergencyContact,
    string Notes);

public sealed record ResidentMedicalConditionEditorDto(
    Guid? Id,
    string ConditionCode,
    string ConditionName,
    bool HasCondition,
    string Notes);

public sealed record ResidentDocumentViewModel(
    Guid Id,
    string DocumentType,
    string Title,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize,
    string UploadedAt,
    string? ExpiryDate,
    string Notes,
    bool IsConfidential);

public sealed record ResidentDocumentUploadDto(
    string DocumentType,
    string Title,
    DateOnly? ExpiryDate,
    string Notes,
    bool IsConfidential);

public sealed class ResidentsWorkspaceViewModel
{
    public IReadOnlyList<ManagedResidentSummaryDto> Residents { get; init; } = [];
    public IReadOnlyList<ManagedRoomSummaryDto> Rooms { get; init; } = [];
    public int TotalResidents { get; init; }
    public int ActiveResidents { get; init; }
    public int AvailableBeds { get; init; }
    public int RoomsNeedingAttention { get; init; }
}

public sealed class ManagedResidentDetailsViewModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string FullNameArabic { get; init; } = string.Empty;
    public string FullNameEnglish { get; init; } = string.Empty;
    public string PhotoPath { get; init; } = string.Empty;
    public string MobilePhone { get; init; } = string.Empty;
    public string Age { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusState { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
    public string BirthDate { get; init; } = string.Empty;
    public string BirthPlace { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;
    public string Religion { get; init; } = string.Empty;
    public string Denomination { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string PassportNumber { get; init; } = string.Empty;
    public string MaritalStatus { get; init; } = string.Empty;
    public string EducationLevel { get; init; } = string.Empty;
    public string PreviousJob { get; init; } = string.Empty;
    public string CurrentAddress { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string AllergiesSummary { get; init; } = string.Empty;
    public string ChronicConditionsSummary { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public string AdmissionDate { get; init; } = string.Empty;
    public string AdmissionStatus { get; init; } = string.Empty;
    public string ResidencyType { get; init; } = string.Empty;
    public string RoomGrade { get; init; } = string.Empty;
    public string TreatingDoctorName { get; init; } = string.Empty;
    public string CompanionName { get; init; } = string.Empty;
    public string AdditionalInformation { get; init; } = string.Empty;
    public string MedicalDeclarationConfirmedBy { get; init; } = string.Empty;
    public string MedicalDeclarationDate { get; init; } = string.Empty;
    public bool MedicalDeclarationConfirmed { get; init; }
    public string AllergyDetails { get; init; } = string.Empty;
    public string PreviousSurgeries { get; init; } = string.Empty;
    public string PreviousInjuriesOrAccidents { get; init; } = string.Empty;
    public IReadOnlyList<ResidentContactEditorDto> Contacts { get; init; } = [];
    public IReadOnlyList<ResidentMedicalConditionEditorDto> MedicalConditions { get; init; } = [];
    public IReadOnlyList<ResidentDocumentViewModel> Documents { get; init; } = [];
    public IReadOnlyList<string> MissingRequiredDocuments { get; init; } = [];
    public IReadOnlyList<MedicationRowDto> Medications { get; init; } = [];
    public IReadOnlyList<SimpleRowDto> Timeline { get; init; } = [];
}

public sealed class ResidentEditorViewModel
{
    public Guid? Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string FullNameArabic { get; init; } = string.Empty;
    public string FullNameEnglish { get; init; } = string.Empty;
    public string PhotoPath { get; init; } = string.Empty;
    public string MobilePhone { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string BirthPlace { get; init; } = string.Empty;
    public string Gender { get; init; } = ResidentGender.Male.ToString();
    public string Religion { get; init; } = string.Empty;
    public string Denomination { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string PassportNumber { get; init; } = string.Empty;
    public string IdOrPassportIssueAuthority { get; init; } = string.Empty;
    public string MaritalStatus { get; init; } = Domain.Residents.MaritalStatus.Unknown.ToString();
    public string EducationLevel { get; init; } = string.Empty;
    public string PreviousJob { get; init; } = string.Empty;
    public string CurrentAddress { get; init; } = string.Empty;
    public string Status { get; init; } = ResidentStatus.Active.ToString();
    public string AdmissionStatus { get; init; } = Domain.Residents.AdmissionStatus.Pending.ToString();
    public string ResidencyType { get; init; } = Domain.Residents.ResidencyType.Permanent.ToString();
    public Guid? FloorId { get; init; }
    public Guid? RoomId { get; init; }
    public DateOnly? AdmissionDate { get; init; }
    public string RoomGrade { get; init; } = string.Empty;
    public string TreatingDoctorName { get; init; } = string.Empty;
    public string CompanionName { get; init; } = string.Empty;
    public string ResponsiblePersonName { get; init; } = string.Empty;
    public string ResponsiblePersonRelationship { get; init; } = string.Empty;
    public string ResponsiblePersonAddress { get; init; } = string.Empty;
    public string ResponsiblePersonPhone { get; init; } = string.Empty;
    public string ResponsiblePersonMobile { get; init; } = string.Empty;
    public string ResponsiblePersonWorkAddress { get; init; } = string.Empty;
    public string SecondResponsiblePersonName { get; init; } = string.Empty;
    public string SecondResponsiblePersonRelationship { get; init; } = string.Empty;
    public string SecondResponsiblePersonAddress { get; init; } = string.Empty;
    public string SecondResponsiblePersonPhone { get; init; } = string.Empty;
    public string SecondResponsiblePersonMobile { get; init; } = string.Empty;
    public string SecondResponsiblePersonWorkAddress { get; init; } = string.Empty;
    public string AdditionalInformation { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string AllergiesSummary { get; init; } = string.Empty;
    public string ChronicConditionsSummary { get; init; } = string.Empty;
    public string AllergyDetails { get; init; } = string.Empty;
    public string PreviousSurgeries { get; init; } = string.Empty;
    public string PreviousInjuriesOrAccidents { get; init; } = string.Empty;
    public bool MedicalDeclarationConfirmed { get; init; }
    public string MedicalDeclarationConfirmedBy { get; init; } = string.Empty;
    public DateOnly? MedicalDeclarationDate { get; init; }
    public string Notes { get; init; } = string.Empty;
    public IReadOnlyList<ResidentContactEditorDto> Contacts { get; init; } = [];
    public IReadOnlyList<ResidentMedicalConditionEditorDto> MedicalConditions { get; init; } = [];
    public IReadOnlyList<ResidentDocumentViewModel> Documents { get; init; } = [];
    public IReadOnlyList<ResidentEditorOptionDto> Rooms { get; init; } = [];
}

public sealed class ResidentUpsertDto
{
    public string Code { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string FullNameArabic { get; init; } = string.Empty;
    public string FullNameEnglish { get; init; } = string.Empty;
    public string MobilePhone { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string BirthPlace { get; init; } = string.Empty;
    public string Gender { get; init; } = ResidentGender.Male.ToString();
    public string Religion { get; init; } = string.Empty;
    public string Denomination { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string PassportNumber { get; init; } = string.Empty;
    public string IdOrPassportIssueAuthority { get; init; } = string.Empty;
    public string MaritalStatus { get; init; } = Domain.Residents.MaritalStatus.Unknown.ToString();
    public string EducationLevel { get; init; } = string.Empty;
    public string PreviousJob { get; init; } = string.Empty;
    public string CurrentAddress { get; init; } = string.Empty;
    public string Status { get; init; } = ResidentStatus.Active.ToString();
    public string AdmissionStatus { get; init; } = Domain.Residents.AdmissionStatus.Pending.ToString();
    public string ResidencyType { get; init; } = Domain.Residents.ResidencyType.Permanent.ToString();
    public Guid? RoomId { get; init; }
    public DateOnly? AdmissionDate { get; init; }
    public string RoomGrade { get; init; } = string.Empty;
    public string TreatingDoctorName { get; init; } = string.Empty;
    public string CompanionName { get; init; } = string.Empty;
    public string ResponsiblePersonName { get; init; } = string.Empty;
    public string ResponsiblePersonRelationship { get; init; } = string.Empty;
    public string ResponsiblePersonAddress { get; init; } = string.Empty;
    public string ResponsiblePersonPhone { get; init; } = string.Empty;
    public string ResponsiblePersonMobile { get; init; } = string.Empty;
    public string ResponsiblePersonWorkAddress { get; init; } = string.Empty;
    public string SecondResponsiblePersonName { get; init; } = string.Empty;
    public string SecondResponsiblePersonRelationship { get; init; } = string.Empty;
    public string SecondResponsiblePersonAddress { get; init; } = string.Empty;
    public string SecondResponsiblePersonPhone { get; init; } = string.Empty;
    public string SecondResponsiblePersonMobile { get; init; } = string.Empty;
    public string SecondResponsiblePersonWorkAddress { get; init; } = string.Empty;
    public string AdditionalInformation { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string AllergiesSummary { get; init; } = string.Empty;
    public string ChronicConditionsSummary { get; init; } = string.Empty;
    public string AllergyDetails { get; init; } = string.Empty;
    public string PreviousSurgeries { get; init; } = string.Empty;
    public string PreviousInjuriesOrAccidents { get; init; } = string.Empty;
    public bool MedicalDeclarationConfirmed { get; init; }
    public string MedicalDeclarationConfirmedBy { get; init; } = string.Empty;
    public DateOnly? MedicalDeclarationDate { get; init; }
    public string Notes { get; init; } = string.Empty;
    public IReadOnlyList<ResidentContactEditorDto> Contacts { get; init; } = [];
    public IReadOnlyList<ResidentMedicalConditionEditorDto> MedicalConditions { get; init; } = [];
}
