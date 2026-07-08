using FathersCare.Application.Abstractions;
using FathersCare.Application.Medications.Validation;
using FathersCare.Domain.Medications;
using FathersCare.Domain.Notifications;
using FathersCare.Domain.Residents;
using FathersCare.Domain.Tenancy;
using FathersCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FathersCare.Infrastructure.Services;

public sealed class MedicationManagementService(AppDbContext db) : IMedicationManagementService
{
    public async Task<MedicationOperationsViewModel> GetWorkspaceAsync(Guid? residentId = null, CancellationToken cancellationToken = default)
    {
        await EnsureTodayDosesAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var inThirtyMinutes = now.AddMinutes(30);

        var residents = await db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted)
            .OrderBy(resident => resident.FullName)
            .Select(resident => new MedicationResidentOptionDto(resident.Id, resident.FullName))
            .ToListAsync(cancellationToken);

        var medicines = await db.Medicines.AsNoTracking()
            .OrderBy(medicine => medicine.Name)
            .Select(medicine => new MedicationItemOptionDto(medicine.Id, MedicineName(medicine.Name, medicine.Strength)))
            .ToListAsync(cancellationToken);

        var doseQuery =
            from dose in db.DoseAdministrations.AsNoTracking()
            join schedule in db.MedicineSchedules.AsNoTracking() on dose.MedicineScheduleId equals schedule.Id
            join residentMedicine in db.ResidentMedicines.AsNoTracking() on schedule.ResidentMedicineId equals residentMedicine.Id
            join medicine in db.Medicines.AsNoTracking() on residentMedicine.MedicineId equals medicine.Id
            join resident in db.Residents.AsNoTracking() on residentMedicine.ResidentId equals resident.Id
            where dose.DoseDate == today && !dose.IsDeleted && !schedule.IsDeleted && !residentMedicine.IsDeleted && !resident.IsDeleted
            orderby schedule.DoseTime
            select new { Dose = dose, Schedule = schedule, ResidentMedicine = residentMedicine, Medicine = medicine, Resident = resident };

        if (residentId.HasValue)
        {
            doseQuery = doseQuery.Where(row => row.Resident.Id == residentId.Value);
        }

        var doseRows = await doseQuery.ToListAsync(cancellationToken);
        var residentMedicineIdsForDoses = doseRows.Select(row => row.ResidentMedicine.Id).Distinct().ToList();
        var availableStockByPlan = await db.ResidentMedicineBatches.AsNoTracking()
            .Where(batch => !batch.IsDeleted
                && residentMedicineIdsForDoses.Contains(batch.ResidentMedicineId)
                && batch.QuantityRemaining > 0
                && (batch.ExpiresOn == null || batch.ExpiresOn >= today))
            .GroupBy(batch => batch.ResidentMedicineId)
            .Select(group => new { ResidentMedicineId = group.Key, Quantity = group.Sum(batch => batch.QuantityRemaining) })
            .ToDictionaryAsync(item => item.ResidentMedicineId, item => item.Quantity, cancellationToken);

        var todayDoses = doseRows.Select(row =>
        {
            var isLate = row.Dose.Status == DoseAdministrationStatus.Scheduled && row.Schedule.DoseTime < now;
            var hasStock = availableStockByPlan.GetValueOrDefault(row.ResidentMedicine.Id) >= row.Schedule.Quantity;
            return new DoseExecutionRowDto(
                row.Dose.Id,
                row.Resident.FullName,
                MedicineName(row.Medicine.Name, row.Medicine.Strength),
                row.Schedule.DoseTime.ToString("HH:mm"),
                $"{TimingText(row.Schedule.Timing)} - {DoseText(row.Schedule.Quantity, row.Medicine.Form)}",
                InstructionText(row.ResidentMedicine.Instructions),
                row.Dose.Status == DoseAdministrationStatus.Scheduled && !hasStock ? "لا يوجد رصيد" : DoseStatusText(row.Dose.Status, isLate),
                row.Dose.Status == DoseAdministrationStatus.Scheduled && !hasStock ? "danger" : DoseState(row.Dose.Status, isLate),
                row.Dose.Status == DoseAdministrationStatus.Scheduled && hasStock);
        }).ToList();

        var batchesQuery =
            from batch in db.ResidentMedicineBatches.AsNoTracking()
            join residentMedicine in db.ResidentMedicines.AsNoTracking() on batch.ResidentMedicineId equals residentMedicine.Id
            join medicine in db.Medicines.AsNoTracking() on residentMedicine.MedicineId equals medicine.Id
            join resident in db.Residents.AsNoTracking() on batch.ResidentId equals resident.Id
            where !batch.IsDeleted && !resident.IsDeleted
            orderby batch.ExpiresOn, batch.ReceivedOn descending
            select new { Batch = batch, Medicine = medicine, Resident = resident };

        if (residentId.HasValue)
        {
            batchesQuery = batchesQuery.Where(row => row.Resident.Id == residentId.Value);
        }

        var batchRows = await batchesQuery.ToListAsync(cancellationToken);
        var batches = batchRows.Select(row => new MedicationBatchRowDto(
            row.Batch.Id,
            row.Resident.FullName,
            MedicineName(row.Medicine.Name, row.Medicine.Strength),
            row.Batch.ReceivedOn.ToString("dd/MM/yyyy"),
            row.Batch.ExpiresOn?.ToString("dd/MM/yyyy") ?? "-",
            QuantityText(row.Batch.QuantityReceived, row.Medicine.Form),
            QuantityText(row.Batch.QuantityRemaining, row.Medicine.Form),
            row.Batch.ReceivedFrom,
            ReceivedByText(row.Batch.ReceivedBy),
            row.Batch.PrescriptionReference ?? "-",
            BatchState(row.Batch, today))).ToList();

        var planQuery =
            from residentMedicine in db.ResidentMedicines.AsNoTracking()
            join medicine in db.Medicines.AsNoTracking() on residentMedicine.MedicineId equals medicine.Id
            join resident in db.Residents.AsNoTracking() on residentMedicine.ResidentId equals resident.Id
            where residentMedicine.IsActive && !residentMedicine.IsDeleted && !resident.IsDeleted
            orderby resident.FullName, medicine.Name
            select new { ResidentMedicine = residentMedicine, Medicine = medicine, Resident = resident };

        if (residentId.HasValue)
        {
            planQuery = planQuery.Where(row => row.Resident.Id == residentId.Value);
        }

        var planRows = await planQuery.ToListAsync(cancellationToken);
        var planIds = planRows.Select(row => row.ResidentMedicine.Id).ToList();
        var schedules = await db.MedicineSchedules.AsNoTracking()
            .Where(schedule => !schedule.IsDeleted && planIds.Contains(schedule.ResidentMedicineId))
            .OrderBy(schedule => schedule.DoseTime)
            .ToListAsync(cancellationToken);
        var stockByPlan = batchRows
            .GroupBy(row => row.Batch.ResidentMedicineId)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Batch.QuantityRemaining));

        var plans = planRows.Select(row =>
        {
            var planSchedules = schedules.Where(schedule => schedule.ResidentMedicineId == row.ResidentMedicine.Id).ToList();
            var stock = stockByPlan.GetValueOrDefault(row.ResidentMedicine.Id);
            return new ResidentMedicationPlanRowDto(
                row.ResidentMedicine.Id,
                row.Resident.FullName,
                MedicineName(row.Medicine.Name, row.Medicine.Strength),
                InstructionText(row.ResidentMedicine.Instructions),
                string.Join("، ", planSchedules.Select(schedule => $"{schedule.DoseTime:HH\\:mm} - {TimingText(schedule.Timing)} ({DoseText(schedule.Quantity, row.Medicine.Form)})")),
                QuantityText(stock, row.Medicine.Form),
                $"{row.ResidentMedicine.StartsOn:dd/MM/yyyy} - {row.ResidentMedicine.EndsOn:dd/MM/yyyy}",
                stock <= planSchedules.Sum(schedule => schedule.Quantity) * 3 ? "danger" : "");
        }).ToList();

        var lateCount = doseRows.Count(row => row.Dose.Status == DoseAdministrationStatus.Scheduled && row.Schedule.DoseTime < now);
        var dueNowCount = doseRows.Count(row => row.Dose.Status == DoseAdministrationStatus.Scheduled && row.Schedule.DoseTime >= now && row.Schedule.DoseTime <= inThirtyMinutes);
        var lowStockCount = plans.Count(plan => plan.State == "danger");
        var expiringSoonCount = batchRows.Count(row => row.Batch.ExpiresOn.HasValue && row.Batch.ExpiresOn.Value <= today.AddDays(30));

        return new MedicationOperationsViewModel
        {
            Residents = residents,
            Medicines = medicines,
            TodayDoses = todayDoses,
            Batches = batches,
            Plans = plans,
            DueNowCount = dueNowCount,
            LateCount = lateCount,
            LowStockCount = lowStockCount,
            ExpiringSoonCount = expiringSoonCount,
            Alerts =
            [
                new("جرعات متأخرة تحتاج تنفيذ", lateCount.ToString(), lateCount > 0 ? "danger" : ""),
                new("جرعات خلال 30 دقيقة", dueNowCount.ToString(), dueNowCount > 0 ? "warn" : ""),
                new("أرصدة دواء منخفضة", lowStockCount.ToString(), lowStockCount > 0 ? "danger" : ""),
                new("صلاحيات خلال 30 يوم", expiringSoonCount.ToString(), expiringSoonCount > 0 ? "warn" : "")
            ]
        };
    }

    public async Task<MedicationPlanEditDto> GetPlanForEditAsync(Guid residentMedicineId, CancellationToken cancellationToken = default)
    {
        var plan = await db.ResidentMedicines.AsNoTracking()
            .Include(item => item.Medicine)
            .FirstOrDefaultAsync(item => item.Id == residentMedicineId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Treatment plan was not found.");

        var batch = await db.ResidentMedicineBatches.AsNoTracking()
            .Where(item => item.ResidentMedicineId == residentMedicineId && !item.IsDeleted)
            .OrderByDescending(item => item.ReceivedOn)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var schedules = await db.MedicineSchedules.AsNoTracking()
            .Where(item => item.ResidentMedicineId == residentMedicineId && !item.IsDeleted)
            .OrderBy(item => item.DoseTime)
            .Select(item => new DosePlanInputDto(item.DoseTime.ToString("HH:mm"), item.Quantity, item.Timing.ToString()))
            .ToListAsync(cancellationToken);

        var days = (plan.EndsOn?.DayNumber ?? DateOnly.FromDateTime(DateTime.Today).DayNumber) - (plan.StartsOn?.DayNumber ?? DateOnly.FromDateTime(DateTime.Today).DayNumber) + 1;
        var plannedQuantity = days > 0 ? schedules.Sum(item => item.Quantity) * days : schedules.Sum(item => item.Quantity);
        var medicine = plan.Medicine ?? throw new InvalidOperationException("Medicine was not found.");

        return new MedicationPlanEditDto(
            plan.Id,
            new ReceiveMedicineDto(
                plan.ResidentId,
                medicine.Name,
                medicine.Strength ?? "",
                medicine.Form ?? "",
                batch?.QuantityReceived ?? plannedQuantity,
                batch?.ReceivedOn ?? plan.StartsOn ?? DateOnly.FromDateTime(DateTime.Today),
                batch?.ExpiresOn,
                batch?.ReceivedFrom ?? "",
                batch?.ReceivedBy ?? "",
                plan.PrescribedBy ?? "",
                plan.PrescriptionReference ?? batch?.PrescriptionReference ?? "",
                plan.PrescriptionDate,
                plan.StartsOn ?? DateOnly.FromDateTime(DateTime.Today),
                plan.EndsOn ?? plan.StartsOn ?? DateOnly.FromDateTime(DateTime.Today),
                plan.Instructions ?? "",
                schedules));
    }

    public async Task<Guid> ReceiveMedicineAsync(ReceiveMedicineDto dto, CancellationToken cancellationToken = default)
    {
        EnsureReceiveMedicineIsValid(dto);

        var tenantId = await EnsureTenantAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var medicineName = dto.MedicineName.Trim();
        var strength = dto.Strength.Trim();

        var residentExists = await db.Residents.AnyAsync(resident => resident.Id == dto.ResidentId && !resident.IsDeleted, cancellationToken);
        if (!residentExists)
        {
            throw new ValidationException("Resident is required.");
        }

        var medicine = await db.Medicines
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Name == medicineName && item.Strength == strength, cancellationToken);

        if (medicine is null)
        {
            medicine = new Medicine
            {
                TenantId = tenantId,
                Name = medicineName,
                Strength = strength,
                Form = dto.Form.Trim(),
                CreatedAt = now
            };
            db.Medicines.Add(medicine);
        }
        else
        {
            medicine.Form = dto.Form.Trim();
            medicine.UpdatedAt = now;
        }

        var residentMedicine = new ResidentMedicine
        {
            TenantId = tenantId,
            ResidentId = dto.ResidentId,
            Medicine = medicine,
            Instructions = dto.Instructions.Trim(),
            PrescribedBy = dto.PrescribedBy.Trim(),
            PrescriptionReference = dto.PrescriptionReference.Trim(),
            PrescriptionDate = dto.PrescriptionDate,
            StartsOn = dto.StartsOn,
            EndsOn = dto.EndsOn,
            CreatedAt = now
        };
        db.ResidentMedicines.Add(residentMedicine);

        var batch = new ResidentMedicineBatch
        {
            TenantId = tenantId,
            ResidentId = dto.ResidentId,
            ResidentMedicine = residentMedicine,
            ReceivedOn = dto.ReceivedOn,
            ExpiresOn = dto.ExpiresOn,
            QuantityReceived = dto.QuantityReceived,
            QuantityRemaining = dto.QuantityReceived,
            ReceivedFrom = dto.ReceivedFrom.Trim(),
            ReceivedBy = dto.ReceivedBy.Trim(),
            PrescriptionReference = dto.PrescriptionReference.Trim(),
            Notes = dto.Instructions.Trim(),
            CreatedAt = now
        };
        db.ResidentMedicineBatches.Add(batch);
        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenantId,
            ActorId = "system",
            Operation = "ReceiveMedicine",
            EntityName = nameof(ResidentMedicine),
            EntityId = residentMedicine.Id,
            OccurredAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await GenerateDosePlanAsync(residentMedicine.Id, dto.Doses, dto.StartsOn, dto.EndsOn, cancellationToken);
        return residentMedicine.Id;
    }

    public async Task UpdatePlanAsync(Guid residentMedicineId, ReceiveMedicineDto dto, CancellationToken cancellationToken = default)
    {
        EnsureReceiveMedicineIsValid(dto);

        var plan = await db.ResidentMedicines
            .Include(item => item.Medicine)
            .FirstOrDefaultAsync(item => item.Id == residentMedicineId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Treatment plan was not found.");

        var residentExists = await db.Residents.AnyAsync(resident => resident.Id == dto.ResidentId && !resident.IsDeleted, cancellationToken);
        if (!residentExists)
        {
            throw new ValidationException("Resident is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var medicineName = dto.MedicineName.Trim();
        var strength = dto.Strength.Trim();
        var medicine = await db.Medicines
            .FirstOrDefaultAsync(item => item.TenantId == plan.TenantId && item.Name == medicineName && item.Strength == strength, cancellationToken);

        if (medicine is null)
        {
            medicine = new Medicine
            {
                TenantId = plan.TenantId,
                Name = medicineName,
                Strength = strength,
                Form = dto.Form.Trim(),
                CreatedAt = now
            };
            db.Medicines.Add(medicine);
        }
        else
        {
            medicine.Form = dto.Form.Trim();
            medicine.UpdatedAt = now;
        }

        plan.ResidentId = dto.ResidentId;
        plan.Medicine = medicine;
        plan.Instructions = dto.Instructions.Trim();
        plan.PrescribedBy = dto.PrescribedBy.Trim();
        plan.PrescriptionReference = dto.PrescriptionReference.Trim();
        plan.PrescriptionDate = dto.PrescriptionDate;
        plan.StartsOn = dto.StartsOn;
        plan.EndsOn = dto.EndsOn;
        plan.UpdatedAt = now;

        var batch = await db.ResidentMedicineBatches
            .Where(item => item.ResidentMedicineId == residentMedicineId && !item.IsDeleted)
            .OrderByDescending(item => item.ReceivedOn)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (batch is null)
        {
            batch = new ResidentMedicineBatch
            {
                TenantId = plan.TenantId,
                ResidentMedicine = plan,
                CreatedAt = now
            };
            db.ResidentMedicineBatches.Add(batch);
        }

        var consumedQuantity = Math.Max(0, batch.QuantityReceived - batch.QuantityRemaining);
        if (dto.QuantityReceived < consumedQuantity)
        {
            throw new ValidationException("Received quantity cannot be less than already administered doses.");
        }

        batch.ResidentId = dto.ResidentId;
        batch.ReceivedOn = dto.ReceivedOn;
        batch.ExpiresOn = dto.ExpiresOn;
        batch.QuantityReceived = dto.QuantityReceived;
        batch.QuantityRemaining = dto.QuantityReceived - consumedQuantity;
        batch.ReceivedFrom = dto.ReceivedFrom.Trim();
        batch.ReceivedBy = dto.ReceivedBy.Trim();
        batch.PrescriptionReference = dto.PrescriptionReference.Trim();
        batch.Notes = dto.Instructions.Trim();
        batch.UpdatedAt = now;

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = plan.TenantId,
            ActorId = "system",
            Operation = "UpdateMedicationPlan",
            EntityName = nameof(ResidentMedicine),
            EntityId = plan.Id,
            OccurredAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await ReplaceDosePlanAsync(plan.Id, dto.Doses, dto.StartsOn, dto.EndsOn, "UpdateMedicationDosePlan", cancellationToken);
    }

    public async Task GenerateDosePlanAsync(Guid residentMedicineId, IReadOnlyList<DosePlanInputDto> doses, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await ReplaceDosePlanAsync(residentMedicineId, doses, from, to, "GenerateMedicationDosePlan", cancellationToken);
    }

    private async Task ReplaceDosePlanAsync(
        Guid residentMedicineId,
        IReadOnlyList<DosePlanInputDto> doses,
        DateOnly from,
        DateOnly to,
        string operation,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            throw new InvalidOperationException("Start date must be before end date.");
        }

        if (!doses.Any(dose => TimeOnly.TryParse(dose.Time, out _) && dose.Quantity > 0))
        {
            throw new ValidationException("At least one valid dose is required.");
        }

        var residentMedicine = await db.ResidentMedicines
            .FirstOrDefaultAsync(item => item.Id == residentMedicineId, cancellationToken)
            ?? throw new InvalidOperationException("Resident medicine was not found.");

        var now = DateTimeOffset.UtcNow;
        var existingSchedules = await db.MedicineSchedules
            .Where(schedule => !schedule.IsDeleted && schedule.ResidentMedicineId == residentMedicineId)
            .ToListAsync(cancellationToken);
        var existingScheduleIds = existingSchedules.Select(schedule => schedule.Id).ToList();
        var existingDoses = await db.DoseAdministrations
            .Where(dose => !dose.IsDeleted && existingScheduleIds.Contains(dose.MedicineScheduleId))
            .ToListAsync(cancellationToken);

        foreach (var schedule in existingSchedules)
        {
            schedule.IsDeleted = true;
            schedule.DeletedAt = now;
            schedule.UpdatedAt = now;
        }

        foreach (var doseAdministration in existingDoses)
        {
            doseAdministration.IsDeleted = true;
            doseAdministration.DeletedAt = now;
            doseAdministration.UpdatedAt = now;
        }

        foreach (var dose in doses.Where(dose => !string.IsNullOrWhiteSpace(dose.Time) && dose.Quantity > 0))
        {
            if (!TimeOnly.TryParse(dose.Time, out var time))
            {
                continue;
            }

            var schedule = new MedicineSchedule
            {
                TenantId = residentMedicine.TenantId,
                ResidentMedicineId = residentMedicine.Id,
                DoseTime = time,
                Quantity = dose.Quantity,
                Timing = ParseTiming(dose.Timing),
                CreatedAt = now
            };
            db.MedicineSchedules.Add(schedule);

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                db.DoseAdministrations.Add(new DoseAdministration
                {
                    TenantId = residentMedicine.TenantId,
                    MedicineSchedule = schedule,
                    DoseDate = date,
                    Status = DoseAdministrationStatus.Scheduled,
                    CreatedAt = now
                });
            }
        }

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = residentMedicine.TenantId,
            ActorId = "system",
            Operation = operation,
            EntityName = nameof(ResidentMedicine),
            EntityId = residentMedicine.Id,
            OccurredAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlanAsync(Guid residentMedicineId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var plan = await db.ResidentMedicines
            .FirstOrDefaultAsync(item => item.Id == residentMedicineId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Treatment plan was not found.");

        var schedules = await db.MedicineSchedules
            .Where(item => item.ResidentMedicineId == residentMedicineId && !item.IsDeleted)
            .ToListAsync(cancellationToken);
        var scheduleIds = schedules.Select(item => item.Id).ToList();

        var doses = await db.DoseAdministrations
            .Where(item => scheduleIds.Contains(item.MedicineScheduleId) && !item.IsDeleted)
            .ToListAsync(cancellationToken);

        var batches = await db.ResidentMedicineBatches
            .Where(item => item.ResidentMedicineId == residentMedicineId && !item.IsDeleted)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        plan.IsActive = false;
        plan.IsDeleted = true;
        plan.DeletedAt = now;
        plan.UpdatedAt = now;
        if (!string.IsNullOrWhiteSpace(normalizedReason))
        {
            plan.Instructions = string.IsNullOrWhiteSpace(plan.Instructions)
                ? normalizedReason
                : $"{plan.Instructions} | Deleted: {normalizedReason}";
        }

        foreach (var schedule in schedules)
        {
            schedule.IsDeleted = true;
            schedule.DeletedAt = now;
            schedule.UpdatedAt = now;
        }

        foreach (var dose in doses)
        {
            dose.IsDeleted = true;
            dose.DeletedAt = now;
            dose.UpdatedAt = now;
            if (!string.IsNullOrWhiteSpace(normalizedReason))
            {
                dose.Notes = string.IsNullOrWhiteSpace(dose.Notes)
                    ? normalizedReason
                    : $"{dose.Notes} | Deleted: {normalizedReason}";
            }
        }

        foreach (var batch in batches)
        {
            batch.IsDeleted = true;
            batch.DeletedAt = now;
            batch.UpdatedAt = now;
            if (!string.IsNullOrWhiteSpace(normalizedReason))
            {
                batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                    ? normalizedReason
                    : $"{batch.Notes} | Deleted: {normalizedReason}";
            }
        }

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = plan.TenantId,
            ActorId = "system",
            Operation = "DeleteMedicationPlan",
            EntityName = nameof(ResidentMedicine),
            EntityId = plan.Id,
            OccurredAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDoseGivenAsync(Guid doseAdministrationId, string administeredBy, string? notes = null, CancellationToken cancellationToken = default)
    {
        var dose = await LoadDoseForUpdate(doseAdministrationId, cancellationToken);
        if (dose.Status == DoseAdministrationStatus.Given)
        {
            return;
        }

        var schedule = dose.MedicineSchedule ?? throw new InvalidOperationException("Dose schedule was not found.");
        var batch = await db.ResidentMedicineBatches
            .Where(item => !item.IsDeleted
                && item.ResidentMedicineId == schedule.ResidentMedicineId
                && item.QuantityRemaining >= schedule.Quantity
                && (item.ExpiresOn == null || item.ExpiresOn >= dose.DoseDate))
            .OrderBy(item => item.ExpiresOn ?? DateOnly.MaxValue)
            .ThenBy(item => item.ReceivedOn)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("لا يوجد رصيد كاف أو صالح لهذا الدواء.");

        batch.QuantityRemaining -= schedule.Quantity;
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        dose.ResidentMedicineBatchId = batch.Id;
        dose.Status = DoseAdministrationStatus.Given;
        dose.AdministeredAt = DateTimeOffset.UtcNow;
        dose.AdministeredBy = string.IsNullOrWhiteSpace(administeredBy) ? "system" : administeredBy.Trim();
        dose.Notes = notes?.Trim();
        dose.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDoseMissedAsync(Guid doseAdministrationId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var dose = await LoadDoseForUpdate(doseAdministrationId, cancellationToken);
        dose.Status = DoseAdministrationStatus.Missed;
        dose.Notes = notes?.Trim();
        dose.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SkipDoseAsync(Guid doseAdministrationId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var dose = await LoadDoseForUpdate(doseAdministrationId, cancellationToken);
        dose.Status = DoseAdministrationStatus.Skipped;
        dose.Notes = notes?.Trim();
        dose.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureTodayDosesAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var schedules = await db.MedicineSchedules.AsNoTracking()
            .Include(schedule => schedule.ResidentMedicine)
            .Where(schedule => !schedule.IsDeleted && schedule.ResidentMedicine!.IsActive && !schedule.ResidentMedicine.IsDeleted)
            .ToListAsync(cancellationToken);
        var existingScheduleIds = await db.DoseAdministrations.AsNoTracking()
            .Where(dose => dose.DoseDate == today && !dose.IsDeleted)
            .Select(dose => dose.MedicineScheduleId)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var schedule in schedules.Where(schedule => !existingScheduleIds.Contains(schedule.Id)))
        {
            db.DoseAdministrations.Add(new DoseAdministration
            {
                TenantId = schedule.TenantId,
                MedicineScheduleId = schedule.Id,
                DoseDate = today,
                Status = DoseAdministrationStatus.Scheduled,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DoseAdministration> LoadDoseForUpdate(Guid doseAdministrationId, CancellationToken cancellationToken)
    {
        return await db.DoseAdministrations
            .Include(dose => dose.MedicineSchedule)
            .FirstOrDefaultAsync(dose => dose.Id == doseAdministrationId && !dose.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Dose was not found.");
    }

    private async Task<Guid> EnsureTenantAsync(CancellationToken cancellationToken)
    {
        var tenantId = await db.Tenants.Select(tenant => tenant.Id).FirstOrDefaultAsync(cancellationToken);
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

    private static void EnsureReceiveMedicineIsValid(ReceiveMedicineDto dto)
    {
        var issues = ReceiveMedicineBusinessRules.Validate(dto, DateOnly.FromDateTime(DateTime.Today));
        if (issues.Count == 0)
        {
            return;
        }

        throw new ValidationException(string.Join(Environment.NewLine, issues.Select(issue => issue.DefaultMessage)));
    }

    private static DoseTiming ParseTiming(string value) => value switch
    {
        "BeforeMeal" => DoseTiming.BeforeMeal,
        "WithMeal" => DoseTiming.WithMeal,
        "AfterMeal" => DoseTiming.AfterMeal,
        _ => DoseTiming.AnyTime
    };

    private static string MedicineName(string name, string? strength) => string.IsNullOrWhiteSpace(strength) ? name : $"{name} {strength}";
    private static string DoseText(decimal quantity, string? form) => $"{quantity:0.##} {MedicineFormText(form)}";
    private static string QuantityText(decimal quantity, string? form) => $"{quantity:0.##} {MedicineFormText(form)}";
    private static string MedicineFormText(string? form)
    {
        if (string.IsNullOrWhiteSpace(form))
        {
            return "وحدة";
        }

        return form.Trim().ToLowerInvariant() switch
        {
            "tablet" or "tab" or "tabs" or "قرص" or "أقراص" => "قرص",
            "capsule" or "cap" or "caps" or "كبسولة" or "كبسولات" => "كبسولة",
            "syrup" or "شراب" => "مل",
            "drop" or "drops" or "قطرة" or "نقط" => "قطرة",
            "injection" or "ampoule" or "حقنة" or "أمبول" => "أمبول",
            "cream" or "كريم" => "جرعة",
            _ => form.Trim()
        };
    }

    private static string TimingText(DoseTiming timing) => timing switch
    {
        DoseTiming.BeforeMeal => "قبل الأكل",
        DoseTiming.WithMeal => "مع الأكل",
        DoseTiming.AfterMeal => "بعد الأكل",
        _ => "أي وقت"
    };

    private static string InstructionText(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            return "-";
        }

        return instructions.Trim().ToLowerInvariant() switch
        {
            "before breakfast" => "قبل الإفطار",
            "before breakfast by 30 minutes" => "قبل الإفطار بـ 30 دقيقة",
            "30 minutes before breakfast" => "قبل الإفطار بـ 30 دقيقة",
            "after breakfast" => "بعد الإفطار",
            "before lunch" => "قبل الغداء",
            "after lunch" => "بعد الغداء",
            "before dinner" => "قبل العشاء",
            "after dinner" => "بعد العشاء",
            "before meal" or "before meals" or "before eating" => "قبل الأكل",
            "with meal" or "with meals" or "with food" => "مع الأكل",
            "after meal" or "after meals" or "after eating" => "بعد الأكل",
            "morning" => "صباحًا",
            "evening" => "مساءً",
            "night" or "bedtime" => "قبل النوم",
            _ => instructions.Trim()
        };
    }

    private static string ReceivedByText(string? receivedBy)
    {
        if (string.IsNullOrWhiteSpace(receivedBy))
        {
            return "-";
        }

        var value = receivedBy.Trim();
        return value.Contains('?') || value == "ممرضة الدار - إدخال من روشتة مصورة"
            ? "ممرضة الدار"
            : value;
    }

    private static string BatchState(ResidentMedicineBatch batch, DateOnly today)
    {
        if (batch.QuantityRemaining <= 0)
        {
            return "danger";
        }

        if (batch.ExpiresOn.HasValue && batch.ExpiresOn.Value <= today.AddDays(30))
        {
            return "warn";
        }

        return "";
    }

    private static string DoseStatusText(DoseAdministrationStatus status, bool isLate) => status switch
    {
        DoseAdministrationStatus.Given => "تم الإعطاء",
        DoseAdministrationStatus.Missed => "فائتة",
        DoseAdministrationStatus.Skipped => "تم التخطي",
        _ => isLate ? "متأخرة" : "منتظرة"
    };

    private static string DoseState(DoseAdministrationStatus status, bool isLate) => status switch
    {
        DoseAdministrationStatus.Given => "",
        DoseAdministrationStatus.Missed => "danger",
        DoseAdministrationStatus.Skipped => "warn",
        _ => isLate ? "danger" : "warn"
    };
}
