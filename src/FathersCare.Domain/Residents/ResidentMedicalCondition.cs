using FathersCare.Domain.Common;

namespace FathersCare.Domain.Residents;

public sealed class ResidentMedicalCondition : TenantEntity
{
    public Guid ResidentMedicalProfileId { get; set; }
    public ResidentMedicalProfile? ResidentMedicalProfile { get; set; }
    public MedicalConditionCode ConditionCode { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public bool HasCondition { get; set; }
    public string? Notes { get; set; }
}
