using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;
using FathersCare.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FathersCare.Infrastructure.Persistence;

public sealed class MedicationRepository(AppDbContext db) : IMedicationRepository
{
    public async Task<Medicine?> GetMedicineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Medicines.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<ResidentMedicine?> GetResidentMedicineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.ResidentMedicines.FirstOrDefaultAsync(rm => rm.Id == id && !rm.IsDeleted, cancellationToken);
    }

    public async Task<MedicineSchedule?> GetMedicineScheduleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.MedicineSchedules.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<DoseAdministration?> GetDoseAdministrationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.DoseAdministrations.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Medicine>> GetMedicinesAsync(CancellationToken cancellationToken = default)
    {
        return await db.Medicines.Where(m => !m.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResidentMedicine>> GetResidentMedicinesAsync(Guid residentId, CancellationToken cancellationToken = default)
    {
        return await db.ResidentMedicines
            .Where(rm => rm.ResidentId == residentId && !rm.IsDeleted)
            .Include(rm => rm.Medicine)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MedicineSchedule>> GetMedicineSchedulesAsync(Guid residentMedicineId, CancellationToken cancellationToken = default)
    {
        return await db.MedicineSchedules
            .Where(s => s.ResidentMedicineId == residentMedicineId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DoseAdministration>> GetDoseAdministrationsAsync(
        Guid? residentMedicineId = null,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.DoseAdministrations.AsNoTracking().Where(d => !d.IsDeleted);
        
        if (residentMedicineId.HasValue)
        {
            query = query.Where(d => d.MedicineSchedule!.ResidentMedicineId == residentMedicineId.Value);
        }
        
        if (date.HasValue)
        {
            query = query.Where(d => d.DoseDate == date.Value);
        }

        return await query
            .Include(d => d.MedicineSchedule)
            .ThenInclude(s => s!.ResidentMedicine)
            .ThenInclude(rm => rm!.Medicine)
            .ToListAsync(cancellationToken);
    }

    public async Task AddMedicineAsync(Medicine medicine, CancellationToken cancellationToken = default)
    {
        await db.Medicines.AddAsync(medicine, cancellationToken);
    }

    public async Task AddResidentMedicineAsync(ResidentMedicine residentMedicine, CancellationToken cancellationToken = default)
    {
        await db.ResidentMedicines.AddAsync(residentMedicine, cancellationToken);
    }

    public async Task AddMedicineScheduleAsync(MedicineSchedule schedule, CancellationToken cancellationToken = default)
    {
        await db.MedicineSchedules.AddAsync(schedule, cancellationToken);
    }

    public async Task AddDoseAdministrationsAsync(IEnumerable<DoseAdministration> doses, CancellationToken cancellationToken = default)
    {
        await db.DoseAdministrations.AddRangeAsync(doses, cancellationToken);
    }

    public async Task UpdateDoseAdministrationAsync(DoseAdministration dose, CancellationToken cancellationToken = default)
    {
        db.DoseAdministrations.Update(dose);
    }

    public async Task<Guid> EnsureTenantAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = await db.Tenants.Select(t => t.Id).FirstOrDefaultAsync(cancellationToken);
        if (tenantId != Guid.Empty)
        {
            return tenantId;
        }

        var tenant = new Tenant
        {
            Name = "دار القديسين استفانوس",
            LegalName = "جمعية راهبات الصليب",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public void AddAudit(AuditLog log)
    {
        db.AuditLogs.Add(log);
    }
}
