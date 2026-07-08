using FathersCare.Application.Abstractions;

namespace FathersCare.Infrastructure.Notifications;

public sealed class SignalRNotificationService : INotificationService
{
    public Task SendToTenantAsync(Guid tenantId, string title, string message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
