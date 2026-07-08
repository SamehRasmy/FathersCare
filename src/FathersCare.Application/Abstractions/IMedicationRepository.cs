using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;

namespace FathersCare.Application.Abstractions;

public interface IMedicationRepository
{
    Task<Medicine?> GetMedicineByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResidentMedicine?> GetResidentMedicineByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MedicineSchedule?> GetMedicineScheduleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DoseAdministration?> GetDoseAdministrationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Medicine>> GetMedicinesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResidentMedicine>> GetResidentMedicinesAsync(Guid residentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicineSchedule>> GetMedicineSchedulesAsync(Guid residentMedicineId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoseAdministration>> GetDoseAdministrationsAsync(
        Guid? residentMedicineId = null,
        DateOnly? date = null,
        CancellationToken cancellationToken = default);
    Task AddMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default);
    Task AddResidentMedicineAsync(ResidentMedicine residentMedicine, CancellationToken cancellationToken = default);
    Task AddMedicineScheduleAsync(MedicineSchedule schedule, CancellationToken cancellationToken = default);
    Task AddDoseAdministrationsAsync(IEnumerable<DoseAdministration> doses, CancellationToken cancellationToken = default);
    Task UpdateDoseAdministrationAsync(DoseAdministration dose, CancellationToken cancellationToken = default);
    Task<Guid> EnsureTenantAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void AddAudit(AuditLog log);
}
