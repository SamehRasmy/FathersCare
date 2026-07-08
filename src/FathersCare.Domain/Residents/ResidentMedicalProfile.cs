using FathersCare.Domain.Common;

namespace FathersCare.Domain.Residents;

public sealed class ResidentMedicalProfile : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Resident? Resident { get; set; }
    public string? BloodType { get; set; }
    public string? AllergiesSummary { get; set; }
    public string? ChronicConditionsSummary { get; set; }
    public string? AllergyDetails { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? PreviousInjuriesOrAccidents { get; set; }
    public bool MedicalDeclarationConfirmed { get; set; }
    public string? MedicalDeclarationConfirmedBy { get; set; }
    public DateOnly? MedicalDeclarationDate { get; set; }
    public string? Notes { get; set; }
    public ICollection<ResidentMedicalCondition> Conditions { get; set; } = [];
}
