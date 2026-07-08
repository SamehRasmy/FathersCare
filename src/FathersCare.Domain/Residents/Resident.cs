using FathersCare.Domain.Common;

namespace FathersCare.Domain.Residents;

public sealed class Resident : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FullNameArabic { get; set; }
    public string? FullNameEnglish { get; set; }
    public string? BirthPlace { get; set; }
    public ResidentGender Gender { get; set; } = ResidentGender.Male;
    public string? Religion { get; set; }
    public string? Denomination { get; set; }
    public string? Nationality { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string? IdOrPassportIssueAuthority { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;
    public string? EducationLevel { get; set; }
    public string? PreviousJob { get; set; }
    public string? CurrentAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobilePhone { get; set; }
    public string? MobileNumber { get; set; }
    public string? PhotoPath { get; set; }
    public DateOnly? BirthDate { get; set; }
    public ResidentStatus Status { get; set; } = ResidentStatus.Active;
    public DateOnly? AdmissionDate { get; set; }
    public AdmissionStatus AdmissionStatus { get; set; } = AdmissionStatus.Pending;
    public ResidencyType ResidencyType { get; set; } = ResidencyType.Permanent;
    public Guid? CurrentFloorId { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public string? RoomGrade { get; set; }
    public string? TreatingDoctorName { get; set; }
    public string? CompanionName { get; set; }
    public string? ResponsiblePersonName { get; set; }
    public string? ResponsiblePersonRelationship { get; set; }
    public string? ResponsiblePersonAddress { get; set; }
    public string? ResponsiblePersonPhone { get; set; }
    public string? ResponsiblePersonMobile { get; set; }
    public string? ResponsiblePersonWorkAddress { get; set; }
    public string? SecondResponsiblePersonName { get; set; }
    public string? SecondResponsiblePersonRelationship { get; set; }
    public string? SecondResponsiblePersonAddress { get; set; }
    public string? SecondResponsiblePersonPhone { get; set; }
    public string? SecondResponsiblePersonMobile { get; set; }
    public string? SecondResponsiblePersonWorkAddress { get; set; }
    public string? AdditionalInformation { get; set; }
    public string? Notes { get; set; }
    public ResidentMedicalProfile? MedicalProfile { get; set; }
    public ICollection<ResidentContact> Contacts { get; set; } = [];
    public ICollection<ResidentDocument> Documents { get; set; } = [];
}
