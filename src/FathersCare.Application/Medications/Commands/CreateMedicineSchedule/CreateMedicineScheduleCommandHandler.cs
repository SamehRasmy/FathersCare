using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;

namespace FathersCare.Application.Medications.Commands.CreateMedicineSchedule;

public sealed class CreateMedicineScheduleCommandHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime) : ICommandHandler<CreateMedicineScheduleCommand, Guid>
{
    public async Task<Guid> Handle(CreateMedicineScheduleCommand request, CancellationToken cancellationToken)
    {
        var residentMedicine = await repository.GetResidentMedicineByIdAsync(request.ResidentMedicineId, cancellationToken)
            ?? throw new InvalidOperationException("Resident medicine not found.");

        var tenantId = currentUser.TenantId ?? await repository.EnsureTenantAsync(cancellationToken);
        var now = dateTime.UtcNow;

        var schedule = new MedicineSchedule
        {
            TenantId = tenantId,
            ResidentMedicineId = request.ResidentMedicineId,
            DoseTime = request.DoseTime,
            Quantity = request.Quantity,
            Timing = request.Timing,
            CreatedAt = now,
            CreatedBy = currentUser.UserId
        };

        await repository.AddMedicineScheduleAsync(schedule, cancellationToken);
        
        // Generate dose administrations for the next 30 days
        var today = DateOnly.FromDateTime(DateTime.Today);
        var doses = new List<DoseAdministration>();
        for (int i = 0; i < 30; i++)
        {
            var doseDate = today.AddDays(i);
            doses.Add(new DoseAdministration
            {
                TenantId = schedule.TenantId,
                MedicineScheduleId = schedule.Id,
                DoseDate = doseDate,
                Status = DoseAdministrationStatus.Scheduled,
                CreatedAt = now,
                CreatedBy = currentUser.UserId
            });
        }

        await repository.AddDoseAdministrationsAsync(doses, cancellationToken);
        repository.AddAudit(new AuditLog
        {
            TenantId = tenantId,
            ActorId = currentUser.UserId ?? "system",
            Operation = "CreateMedicineSchedule",
            EntityName = nameof(MedicineSchedule),
            EntityId = schedule.Id,
            OccurredAt = now
        });
        await repository.SaveChangesAsync(cancellationToken);
        return schedule.Id;
    }
}
