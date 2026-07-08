using FathersCare.Domain.Common;

namespace FathersCare.Domain.Staff;

public enum StaffRole
{
    Administrator = 1,
    Nurse = 2,
    Kitchen = 3,
    Finance = 4,
    Caregiver = 5
}

public sealed class StaffMember : TenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public StaffRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Shift : TenantEntity
{
    public Guid StaffMemberId { get; set; }
    public StaffMember? StaffMember { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
}
