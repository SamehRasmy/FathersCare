using FathersCare.Domain.Common;

namespace FathersCare.Domain.Tenancy;

public sealed class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public bool IsActive { get; set; } = true;
}
