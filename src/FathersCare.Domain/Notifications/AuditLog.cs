using FathersCare.Domain.Common;

namespace FathersCare.Domain.Notifications;

public sealed class AuditLog : BaseEntity
{
    public Guid TenantId { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? DetailsJson { get; set; }
}
