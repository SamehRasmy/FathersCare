using FathersCare.Domain.Common;

namespace FathersCare.Domain.Residents;

public sealed class ResidentContact : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Resident? Resident { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string? Job { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsEmergencyContact { get; set; }
    public string? Notes { get; set; }
}
