using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;

namespace FathersCare.Application.Medications.Commands.MarkDoseAsGiven;

public sealed class MarkDoseAsGivenCommandHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime) : ICommandHandler<MarkDoseAsGivenCommand, bool>
{
    public async Task<bool> Handle(MarkDoseAsGivenCommand request, CancellationToken cancellationToken)
    {
        var dose = await repository.GetDoseAdministrationByIdAsync(request.DoseAdministrationId, cancellationToken)
            ?? throw new InvalidOperationException("Dose administration not found.");

        if (dose.IsDeleted)
        {
            throw new InvalidOperationException("Dose administration is deleted.");
        }

        var now = dateTime.UtcNow;
        dose.Status = DoseAdministrationStatus.Given;
        dose.AdministeredAt = request.GivenAt;
        dose.AdministeredBy = request.GivenBy;
        dose.UpdatedAt = now;
        dose.UpdatedBy = currentUser.UserId;

        await repository.UpdateDoseAdministrationAsync(dose, cancellationToken);
        repository.AddAudit(new AuditLog
        {
            TenantId = dose.TenantId,
            ActorId = currentUser.UserId ?? "system",
            Operation = "MarkDoseAsGiven",
            EntityName = nameof(DoseAdministration),
            EntityId = dose.Id,
            OccurredAt = now
        });
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}