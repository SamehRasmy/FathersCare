using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;

namespace FathersCare.Application.Medications.Commands.RegisterResidentMedicine;

public sealed class RegisterResidentMedicineCommandHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime) : ICommandHandler<RegisterResidentMedicineCommand, Guid>
{
    public async Task<Guid> Handle(RegisterResidentMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await repository.GetMedicineByIdAsync(request.MedicineId, cancellationToken)
            ?? throw new InvalidOperationException("Medicine not found.");

        var tenantId = currentUser.TenantId ?? await repository.EnsureTenantAsync(cancellationToken);
        var now = dateTime.UtcNow;

        var residentMedicine = new ResidentMedicine
        {
            TenantId = tenantId,
            ResidentId = request.ResidentId,
            MedicineId = request.MedicineId,
            Instructions = request.Instructions?.Trim(),
            IsActive = true,
            CreatedAt = now,
            CreatedBy = currentUser.UserId
        };

        await repository.AddResidentMedicineAsync(residentMedicine, cancellationToken);
        repository.AddAudit(new AuditLog
        {
            TenantId = tenantId,
            ActorId = currentUser.UserId ?? "system",
            Operation = "RegisterResidentMedicine",
            EntityName = nameof(ResidentMedicine),
            EntityId = residentMedicine.Id,
            OccurredAt = now
        });
        await repository.SaveChangesAsync(cancellationToken);
        return residentMedicine.Id;
    }
}