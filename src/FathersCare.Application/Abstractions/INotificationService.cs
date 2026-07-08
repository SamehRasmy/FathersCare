namespace FathersCare.Application.Abstractions;

public interface INotificationService
{
    Task SendToTenantAsync(Guid tenantId, string title, string message, CancellationToken cancellationToken = default);
}
