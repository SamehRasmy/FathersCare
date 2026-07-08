using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Validation;
using FathersCare.Domain.Notifications;
using FathersCare.Domain.Residents;
using FathersCare.Domain.Rooms;
using FathersCare.Domain.Tenancy;
using FathersCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FathersCare.Infrastructure.Services;

public sealed class ResidentManagementService(AppDbContext db, IFileStorageService fileStorage) : IResidentManagementService
{
    private static readonly ResidentDocumentType[] RequiredDocumentTypes =
    [
        ResidentDocumentType.NationalIdCopy,
        ResidentDocumentType.MedicalReport,
        ResidentDocumentType.AdmissionApplication,
        ResidentDocumentType.GeneralResidentInformationForm,
        ResidentDocumentType.MedicalHistoryForm,
        ResidentDocumentType.ConsentForm
    ];

    public async Task<ResidentsWorkspaceViewModel> GetWorkspaceAsync(string? search = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var residentsQuery = db.Residents
            .AsNoTracking()
            .Include(resident => resident.MedicalProfile)
            .Include(resident => resident.Contacts)
            .Where(resident => !resident.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            residentsQuery = residentsQuery.Where(resident =>
                resident.FullName.Contains(normalizedSearch) ||
                resident.Code.Contains(normalizedSearch) ||
                (resident.FullNameArabic != null && resident.FullNameArabic.Contains(normalizedSearch)) ||
                (resident.FullNameEnglish != null && resident.FullNameEnglish.Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ResidentStatus>(status, out var parsedStatus))
        {
            residentsQuery = residentsQuery.Where(resident => resident.Status == parsedStatus);
        }

        var residents = await residentsQuery.OrderBy(resident => resident.Code).ToListAsync(cancellationToken);
        var rooms = await db.Rooms.AsNoTracking()
            .Include(room => room.Floor)
            .OrderBy(room => room.Floor!.Number)
            .ThenBy(room => room.Number)
            .ToListAsync(cancellationToken);
        var residentMedicineCounts = await db.ResidentMedicines.AsNoTracking()
            .Where(medicine => medicine.IsActive)
            .GroupBy(medicine => medicine.ResidentId)
            .Select(group => new { ResidentId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ResidentId, item => item.Count, cancellationToken);
        var occupiedByRoom = await db.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.CurrentRoomId.HasValue && resident.Status == ResidentStatus.Active)
            .GroupBy(resident => resident.CurrentRoomId!.Value)
            .Select(group => new { RoomId = group.Key, Occupied = group.Count() })
            .ToDictionaryAsync(item => item.RoomId, item => item.Occupied, cancellationToken);
        var roomLookup = rooms.ToDictionary(room => room.Id);

        var summaries = residents.Select(resident =>
        {
            var room = resident.CurrentRoomId.HasValue && roomLookup.TryGetValue(resident.CurrentRoomId.Value, out var currentRoom)
                ? RoomLabel(currentRoom.Number, currentRoom.Floor?.Name)
                : string.Empty;
            var activeContacts = resident.Contacts.Where(contact => !contact.IsDeleted).ToList();
            var primaryContact = activeContacts.FirstOrDefault(contact => contact.IsPrimary) ?? activeContacts.FirstOrDefault();

            return new ManagedResidentSummaryDto(
                resident.Id,
                resident.Code,
                resident.FullName,
                resident.PhotoPath ?? string.Empty,
                resident.MobileNumber ?? resident.MobilePhone ?? string.Empty,
                CalculateAgeYears(resident.BirthDate),
                resident.Status.ToString(),
                StatusState(resident.Status),
                room,
                resident.MedicalProfile?.BloodType ?? string.Empty,
                primaryContact?.MobileNumber ?? primaryContact?.PhoneNumber ?? string.Empty,
                residentMedicineCounts.GetValueOrDefault(resident.Id));
        }).ToList();

        var roomSummaries = rooms.Select(room =>
        {
            var occupied = occupiedByRoom.GetValueOrDefault(room.Id);
            var state = occupied >= room.Capacity ? "danger" : occupied == 0 ? "warn" : string.Empty;
            return new ManagedRoomSummaryDto(room.Id, room.Number, room.Floor?.Name ?? string.Empty, room.Capacity, occupied, state);
        }).ToList();

        return new ResidentsWorkspaceViewModel
        {
            Residents = summaries,
            Rooms = roomSummaries,
            TotalResidents = await db.Residents.CountAsync(resident => !resident.IsDeleted, cancellationToken),
            ActiveResidents = await db.Residents.CountAsync(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active, cancellationToken),
            AvailableBeds = rooms.Sum(room => room.Capacity) - occupiedByRoom.Values.Sum(),
            RoomsNeedingAttention = roomSummaries.Count(room => room.State == "danger")
        };
    }

    public async Task<ManagedResidentDetailsViewModel?> GetDetailsAsync(Guid residentId, CancellationToken cancellationToken = default)
    {
        var resident = await db.Residents.AsNoTracking()
            .Include(item => item.MedicalProfile!)
                .ThenInclude(profile => profile.Conditions.Where(condition => !condition.IsDeleted))
            .Include(item => item.Contacts.Where(contact => !contact.IsDeleted))
            .Include(item => item.Documents.Where(document => !document.IsDeleted))
            .FirstOrDefaultAsync(item => item.Id == residentId && !item.IsDeleted, cancellationToken);

        if (resident is null)
        {
            return null;
        }

        var room = resident.CurrentRoomId.HasValue
            ? await db.Rooms.AsNoTracking().Include(item => item.Floor).FirstOrDefaultAsync(item => item.Id == resident.CurrentRoomId.Value, cancellationToken)
            : null;

        var medications = await LoadResidentMedications(resident.Id, cancellationToken);
        var documents = resident.Documents
            .Where(document => !document.IsDeleted)
            .OrderBy(document => document.DocumentType)
            .ThenByDescending(document => document.CreatedAt)
            .Select(MapDocument)
            .ToList();

        var uploadedTypes = resident.Documents.Where(document => !document.IsDeleted).Select(document => document.DocumentType).Distinct().ToHashSet();
        var missingRequiredDocuments = RequiredDocumentTypes
            .Where(documentType => !uploadedTypes.Contains(documentType))
            .Select(documentType => documentType.ToString())
            .ToList();

        return new ManagedResidentDetailsViewModel
        {
            Id = resident.Id,
            Code = resident.Code,
            FullName = resident.FullName,
            FullNameArabic = resident.FullNameArabic ?? string.Empty,
            FullNameEnglish = resident.FullNameEnglish ?? string.Empty,
            PhotoPath = resident.PhotoPath ?? string.Empty,
            MobilePhone = resident.MobileNumber ?? resident.MobilePhone ?? string.Empty,
            Age = CalculateAgeYears(resident.BirthDate),
            Status = resident.Status.ToString(),
            StatusState = StatusState(resident.Status),
            Room = room is null ? string.Empty : RoomLabel(room.Number, room.Floor?.Name),
            BirthDate = resident.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            BirthPlace = resident.BirthPlace ?? string.Empty,
            Gender = resident.Gender.ToString(),
            Religion = resident.Religion ?? string.Empty,
            Denomination = resident.Denomination ?? string.Empty,
            Nationality = resident.Nationality ?? string.Empty,
            NationalId = resident.NationalId ?? string.Empty,
            PassportNumber = resident.PassportNumber ?? string.Empty,
            MaritalStatus = resident.MaritalStatus.ToString(),
            EducationLevel = resident.EducationLevel ?? string.Empty,
            PreviousJob = resident.PreviousJob ?? string.Empty,
            CurrentAddress = resident.CurrentAddress ?? string.Empty,
            PhoneNumber = resident.PhoneNumber ?? string.Empty,
            BloodType = resident.MedicalProfile?.BloodType ?? string.Empty,
            AllergiesSummary = resident.MedicalProfile?.AllergiesSummary ?? string.Empty,
            ChronicConditionsSummary = resident.MedicalProfile?.ChronicConditionsSummary ?? string.Empty,
            AllergyDetails = resident.MedicalProfile?.AllergyDetails ?? string.Empty,
            PreviousSurgeries = resident.MedicalProfile?.PreviousSurgeries ?? string.Empty,
            PreviousInjuriesOrAccidents = resident.MedicalProfile?.PreviousInjuriesOrAccidents ?? string.Empty,
            Notes = resident.Notes ?? resident.MedicalProfile?.Notes ?? string.Empty,
            AdmissionDate = resident.AdmissionDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            AdmissionStatus = resident.AdmissionStatus.ToString(),
            ResidencyType = resident.ResidencyType.ToString(),
            RoomGrade = resident.RoomGrade ?? string.Empty,
            TreatingDoctorName = resident.TreatingDoctorName ?? string.Empty,
            CompanionName = resident.CompanionName ?? string.Empty,
            AdditionalInformation = resident.AdditionalInformation ?? string.Empty,
            MedicalDeclarationConfirmed = resident.MedicalProfile?.MedicalDeclarationConfirmed ?? false,
            MedicalDeclarationConfirmedBy = resident.MedicalProfile?.MedicalDeclarationConfirmedBy ?? string.Empty,
            MedicalDeclarationDate = resident.MedicalProfile?.MedicalDeclarationDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            Contacts = resident.Contacts
                .Where(contact => !contact.IsDeleted)
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenByDescending(contact => contact.IsEmergencyContact)
                .ThenBy(contact => contact.FullName)
                .Select(MapContact)
                .ToList(),
            MedicalConditions = BuildConditionRows(resident.MedicalProfile),
            Documents = documents,
            MissingRequiredDocuments = missingRequiredDocuments,
            Medications = medications,
            Timeline =
            [
                new("CreatedAt", resident.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")),
                new("UpdatedAt", resident.UpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? string.Empty),
                new("CurrentRoom", room?.Number ?? string.Empty)
            ]
        };
    }

    public async Task<ResidentEditorViewModel> GetEditorAsync(Guid? residentId = null, CancellationToken cancellationToken = default)
    {
        var rooms = await LoadRoomOptions(cancellationToken);

        if (residentId is null)
        {
            return new ResidentEditorViewModel
            {
                Rooms = rooms,
                MedicalConditions = CreateDefaultConditionEditors()
            };
        }

        var resident = await db.Residents
            .AsNoTracking()
            .Include(item => item.MedicalProfile!)
                .ThenInclude(profile => profile.Conditions.Where(condition => !condition.IsDeleted))
            .Include(item => item.Contacts.Where(contact => !contact.IsDeleted))
            .Include(item => item.Documents.Where(document => !document.IsDeleted))
            .FirstOrDefaultAsync(item => item.Id == residentId.Value && !item.IsDeleted, cancellationToken);

        if (resident is null)
        {
            return new ResidentEditorViewModel
            {
                Rooms = rooms,
                MedicalConditions = CreateDefaultConditionEditors()
            };
        }

        return new ResidentEditorViewModel
        {
            Id = resident.Id,
            Code = resident.Code,
            FullName = resident.FullName,
            FullNameArabic = resident.FullNameArabic ?? string.Empty,
            FullNameEnglish = resident.FullNameEnglish ?? string.Empty,
            PhotoPath = resident.PhotoPath ?? string.Empty,
            MobilePhone = resident.MobileNumber ?? resident.MobilePhone ?? string.Empty,
            PhoneNumber = resident.PhoneNumber ?? string.Empty,
            BirthDate = resident.BirthDate,
            BirthPlace = resident.BirthPlace ?? string.Empty,
            Gender = resident.Gender.ToString(),
            Religion = resident.Religion ?? string.Empty,
            Denomination = resident.Denomination ?? string.Empty,
            Nationality = resident.Nationality ?? string.Empty,
            NationalId = resident.NationalId ?? string.Empty,
            PassportNumber = resident.PassportNumber ?? string.Empty,
            IdOrPassportIssueAuthority = resident.IdOrPassportIssueAuthority ?? string.Empty,
            MaritalStatus = resident.MaritalStatus.ToString(),
            EducationLevel = resident.EducationLevel ?? string.Empty,
            PreviousJob = resident.PreviousJob ?? string.Empty,
            CurrentAddress = resident.CurrentAddress ?? string.Empty,
            Status = resident.Status.ToString(),
            AdmissionStatus = resident.AdmissionStatus.ToString(),
            ResidencyType = resident.ResidencyType.ToString(),
            FloorId = resident.CurrentFloorId,
            RoomId = resident.CurrentRoomId,
            AdmissionDate = resident.AdmissionDate,
            RoomGrade = resident.RoomGrade ?? string.Empty,
            TreatingDoctorName = resident.TreatingDoctorName ?? string.Empty,
            CompanionName = resident.CompanionName ?? string.Empty,
            ResponsiblePersonName = resident.ResponsiblePersonName ?? string.Empty,
            ResponsiblePersonRelationship = resident.ResponsiblePersonRelationship ?? string.Empty,
            ResponsiblePersonAddress = resident.ResponsiblePersonAddress ?? string.Empty,
            ResponsiblePersonPhone = resident.ResponsiblePersonPhone ?? string.Empty,
            ResponsiblePersonMobile = resident.ResponsiblePersonMobile ?? string.Empty,
            ResponsiblePersonWorkAddress = resident.ResponsiblePersonWorkAddress ?? string.Empty,
            SecondResponsiblePersonName = resident.SecondResponsiblePersonName ?? string.Empty,
            SecondResponsiblePersonRelationship = resident.SecondResponsiblePersonRelationship ?? string.Empty,
            SecondResponsiblePersonAddress = resident.SecondResponsiblePersonAddress ?? string.Empty,
            SecondResponsiblePersonPhone = resident.SecondResponsiblePersonPhone ?? string.Empty,
            SecondResponsiblePersonMobile = resident.SecondResponsiblePersonMobile ?? string.Empty,
            SecondResponsiblePersonWorkAddress = resident.SecondResponsiblePersonWorkAddress ?? string.Empty,
            AdditionalInformation = resident.AdditionalInformation ?? string.Empty,
            BloodType = resident.MedicalProfile?.BloodType ?? string.Empty,
            AllergiesSummary = resident.MedicalProfile?.AllergiesSummary ?? string.Empty,
            ChronicConditionsSummary = resident.MedicalProfile?.ChronicConditionsSummary ?? string.Empty,
            AllergyDetails = resident.MedicalProfile?.AllergyDetails ?? string.Empty,
            PreviousSurgeries = resident.MedicalProfile?.PreviousSurgeries ?? string.Empty,
            PreviousInjuriesOrAccidents = resident.MedicalProfile?.PreviousInjuriesOrAccidents ?? string.Empty,
            MedicalDeclarationConfirmed = resident.MedicalProfile?.MedicalDeclarationConfirmed ?? false,
            MedicalDeclarationConfirmedBy = resident.MedicalProfile?.MedicalDeclarationConfirmedBy ?? string.Empty,
            MedicalDeclarationDate = resident.MedicalProfile?.MedicalDeclarationDate,
            Notes = resident.Notes ?? resident.MedicalProfile?.Notes ?? string.Empty,
            Contacts = resident.Contacts
                .Where(contact => !contact.IsDeleted)
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenBy(contact => contact.FullName)
                .Select(MapContact)
                .ToList(),
            MedicalConditions = MergeConditionEditors(resident.MedicalProfile?.Conditions),
            Documents = resident.Documents.Where(document => !document.IsDeleted).OrderByDescending(document => document.CreatedAt).Select(MapDocument).ToList(),
            Rooms = rooms
        };
    }

    public async Task<Guid> CreateAsync(ResidentUpsertDto dto, CancellationToken cancellationToken = default)
    {
        EnsureResidentIsValid(dto);
        var tenantId = await EnsureTenantAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var resident = new Resident
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            FullName = dto.FullName.Trim(),
            FullNameArabic = dto.FullNameArabic.Trim().NullIfEmpty(),
            FullNameEnglish = dto.FullNameEnglish.Trim().NullIfEmpty(),
            BirthDate = dto.BirthDate,
            BirthPlace = dto.BirthPlace.Trim().NullIfEmpty(),
            Gender = ParseEnum(dto.Gender, ResidentGender.Male),
            Religion = dto.Religion.Trim().NullIfEmpty(),
            Denomination = dto.Denomination.Trim().NullIfEmpty(),
            Nationality = dto.Nationality.Trim().NullIfEmpty(),
            NationalId = dto.NationalId.Trim().NullIfEmpty(),
            PassportNumber = dto.PassportNumber.Trim().NullIfEmpty(),
            IdOrPassportIssueAuthority = dto.IdOrPassportIssueAuthority.Trim().NullIfEmpty(),
            MaritalStatus = ParseEnum(dto.MaritalStatus, MaritalStatus.Unknown),
            EducationLevel = dto.EducationLevel.Trim().NullIfEmpty(),
            PreviousJob = dto.PreviousJob.Trim().NullIfEmpty(),
            CurrentAddress = dto.CurrentAddress.Trim().NullIfEmpty(),
            PhoneNumber = dto.PhoneNumber.Trim().NullIfEmpty(),
            MobilePhone = dto.MobilePhone.Trim(),
            MobileNumber = dto.MobilePhone.Trim().NullIfEmpty(),
            Status = ParseEnum(dto.Status, ResidentStatus.Active),
            AdmissionDate = dto.AdmissionDate,
            AdmissionStatus = ParseEnum(dto.AdmissionStatus, AdmissionStatus.Pending),
            ResidencyType = ParseEnum(dto.ResidencyType, ResidencyType.Permanent),
            CurrentRoomId = dto.RoomId,
            RoomGrade = dto.RoomGrade.Trim().NullIfEmpty(),
            TreatingDoctorName = dto.TreatingDoctorName.Trim().NullIfEmpty(),
            CompanionName = dto.CompanionName.Trim().NullIfEmpty(),
            ResponsiblePersonName = dto.ResponsiblePersonName.Trim().NullIfEmpty(),
            ResponsiblePersonRelationship = dto.ResponsiblePersonRelationship.Trim().NullIfEmpty(),
            ResponsiblePersonAddress = dto.ResponsiblePersonAddress.Trim().NullIfEmpty(),
            ResponsiblePersonPhone = dto.ResponsiblePersonPhone.Trim().NullIfEmpty(),
            ResponsiblePersonMobile = dto.ResponsiblePersonMobile.Trim().NullIfEmpty(),
            ResponsiblePersonWorkAddress = dto.ResponsiblePersonWorkAddress.Trim().NullIfEmpty(),
            SecondResponsiblePersonName = dto.SecondResponsiblePersonName.Trim().NullIfEmpty(),
            SecondResponsiblePersonRelationship = dto.SecondResponsiblePersonRelationship.Trim().NullIfEmpty(),
            SecondResponsiblePersonAddress = dto.SecondResponsiblePersonAddress.Trim().NullIfEmpty(),
            SecondResponsiblePersonPhone = dto.SecondResponsiblePersonPhone.Trim().NullIfEmpty(),
            SecondResponsiblePersonMobile = dto.SecondResponsiblePersonMobile.Trim().NullIfEmpty(),
            SecondResponsiblePersonWorkAddress = dto.SecondResponsiblePersonWorkAddress.Trim().NullIfEmpty(),
            AdditionalInformation = dto.AdditionalInformation.Trim().NullIfEmpty(),
            Notes = dto.Notes.Trim().NullIfEmpty(),
            CreatedAt = now,
            MedicalProfile = new ResidentMedicalProfile
            {
                TenantId = tenantId,
                BloodType = dto.BloodType.Trim().NullIfEmpty(),
                AllergiesSummary = dto.AllergiesSummary.Trim().NullIfEmpty(),
                ChronicConditionsSummary = dto.ChronicConditionsSummary.Trim().NullIfEmpty(),
                AllergyDetails = dto.AllergyDetails.Trim().NullIfEmpty(),
                PreviousSurgeries = dto.PreviousSurgeries.Trim().NullIfEmpty(),
                PreviousInjuriesOrAccidents = dto.PreviousInjuriesOrAccidents.Trim().NullIfEmpty(),
                MedicalDeclarationConfirmed = dto.MedicalDeclarationConfirmed,
                MedicalDeclarationConfirmedBy = dto.MedicalDeclarationConfirmedBy.Trim().NullIfEmpty(),
                MedicalDeclarationDate = dto.MedicalDeclarationDate,
                Notes = dto.Notes.Trim().NullIfEmpty(),
                CreatedAt = now
            }
        };

        foreach (var contact in NormalizeContacts(dto.Contacts))
        {
            resident.Contacts.Add(new ResidentContact
            {
                TenantId = tenantId,
                FullName = contact.FullName,
                Relationship = contact.Relationship,
                Job = contact.Job,
                Address = contact.Address,
                PhoneNumber = contact.PhoneNumber,
                MobileNumber = contact.MobileNumber,
                IsPrimary = contact.IsPrimary,
                IsEmergencyContact = contact.IsEmergencyContact,
                Notes = contact.Notes,
                CreatedAt = now
            });
        }

        foreach (var condition in NormalizeConditions(dto.MedicalConditions))
        {
            resident.MedicalProfile.Conditions.Add(new ResidentMedicalCondition
            {
                TenantId = tenantId,
                ConditionCode = ParseEnum(condition.ConditionCode, MedicalConditionCode.Anemia),
                ConditionName = condition.ConditionName,
                HasCondition = condition.HasCondition,
                Notes = condition.Notes,
                CreatedAt = now
            });
        }

        db.Residents.Add(resident);
        if (dto.RoomId.HasValue)
        {
            db.RoomOccupancies.Add(new RoomOccupancy
            {
                TenantId = tenantId,
                RoomId = dto.RoomId.Value,
                ResidentId = resident.Id,
                StartsOn = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = now
            });
        }

        AddAudit(tenantId, "CreateResident", nameof(Resident), resident.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return resident.Id;
    }

    public async Task UpdateAsync(Guid residentId, ResidentUpsertDto dto, CancellationToken cancellationToken = default)
    {
        EnsureResidentIsValid(dto);
        var resident = await db.Residents
            .Include(item => item.MedicalProfile!)
            .FirstOrDefaultAsync(item => item.Id == residentId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Resident not found.");

        var now = DateTimeOffset.UtcNow;
        var oldRoomId = resident.CurrentRoomId;
        resident.Code = dto.Code.Trim();
        resident.FullName = dto.FullName.Trim();
        resident.FullNameArabic = dto.FullNameArabic.Trim().NullIfEmpty();
        resident.FullNameEnglish = dto.FullNameEnglish.Trim().NullIfEmpty();
        resident.BirthDate = dto.BirthDate;
        resident.BirthPlace = dto.BirthPlace.Trim().NullIfEmpty();
        resident.Gender = ParseEnum(dto.Gender, ResidentGender.Male);
        resident.Religion = dto.Religion.Trim().NullIfEmpty();
        resident.Denomination = dto.Denomination.Trim().NullIfEmpty();
        resident.Nationality = dto.Nationality.Trim().NullIfEmpty();
        resident.NationalId = dto.NationalId.Trim().NullIfEmpty();
        resident.PassportNumber = dto.PassportNumber.Trim().NullIfEmpty();
        resident.IdOrPassportIssueAuthority = dto.IdOrPassportIssueAuthority.Trim().NullIfEmpty();
        resident.MaritalStatus = ParseEnum(dto.MaritalStatus, MaritalStatus.Unknown);
        resident.EducationLevel = dto.EducationLevel.Trim().NullIfEmpty();
        resident.PreviousJob = dto.PreviousJob.Trim().NullIfEmpty();
        resident.CurrentAddress = dto.CurrentAddress.Trim().NullIfEmpty();
        resident.PhoneNumber = dto.PhoneNumber.Trim().NullIfEmpty();
        resident.MobilePhone = dto.MobilePhone.Trim();
        resident.MobileNumber = dto.MobilePhone.Trim().NullIfEmpty();
        resident.Status = ParseEnum(dto.Status, ResidentStatus.Active);
        resident.AdmissionDate = dto.AdmissionDate;
        resident.AdmissionStatus = ParseEnum(dto.AdmissionStatus, AdmissionStatus.Pending);
        resident.ResidencyType = ParseEnum(dto.ResidencyType, ResidencyType.Permanent);
        resident.CurrentRoomId = dto.RoomId;
        resident.RoomGrade = dto.RoomGrade.Trim().NullIfEmpty();
        resident.TreatingDoctorName = dto.TreatingDoctorName.Trim().NullIfEmpty();
        resident.CompanionName = dto.CompanionName.Trim().NullIfEmpty();
        resident.ResponsiblePersonName = dto.ResponsiblePersonName.Trim().NullIfEmpty();
        resident.ResponsiblePersonRelationship = dto.ResponsiblePersonRelationship.Trim().NullIfEmpty();
        resident.ResponsiblePersonAddress = dto.ResponsiblePersonAddress.Trim().NullIfEmpty();
        resident.ResponsiblePersonPhone = dto.ResponsiblePersonPhone.Trim().NullIfEmpty();
        resident.ResponsiblePersonMobile = dto.ResponsiblePersonMobile.Trim().NullIfEmpty();
        resident.ResponsiblePersonWorkAddress = dto.ResponsiblePersonWorkAddress.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonName = dto.SecondResponsiblePersonName.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonRelationship = dto.SecondResponsiblePersonRelationship.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonAddress = dto.SecondResponsiblePersonAddress.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonPhone = dto.SecondResponsiblePersonPhone.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonMobile = dto.SecondResponsiblePersonMobile.Trim().NullIfEmpty();
        resident.SecondResponsiblePersonWorkAddress = dto.SecondResponsiblePersonWorkAddress.Trim().NullIfEmpty();
        resident.AdditionalInformation = dto.AdditionalInformation.Trim().NullIfEmpty();
        resident.Notes = dto.Notes.Trim().NullIfEmpty();
        resident.UpdatedAt = now;

        resident.MedicalProfile ??= new ResidentMedicalProfile
        {
            TenantId = resident.TenantId,
            ResidentId = resident.Id,
            CreatedAt = now
        };

        resident.MedicalProfile.BloodType = dto.BloodType.Trim().NullIfEmpty();
        resident.MedicalProfile.AllergiesSummary = dto.AllergiesSummary.Trim().NullIfEmpty();
        resident.MedicalProfile.ChronicConditionsSummary = dto.ChronicConditionsSummary.Trim().NullIfEmpty();
        resident.MedicalProfile.AllergyDetails = dto.AllergyDetails.Trim().NullIfEmpty();
        resident.MedicalProfile.PreviousSurgeries = dto.PreviousSurgeries.Trim().NullIfEmpty();
        resident.MedicalProfile.PreviousInjuriesOrAccidents = dto.PreviousInjuriesOrAccidents.Trim().NullIfEmpty();
        resident.MedicalProfile.MedicalDeclarationConfirmed = dto.MedicalDeclarationConfirmed;
        resident.MedicalProfile.MedicalDeclarationConfirmedBy = dto.MedicalDeclarationConfirmedBy.Trim().NullIfEmpty();
        resident.MedicalProfile.MedicalDeclarationDate = dto.MedicalDeclarationDate;
        resident.MedicalProfile.Notes = dto.Notes.Trim().NullIfEmpty();
        resident.MedicalProfile.UpdatedAt = now;

        await ReplaceContactsAsync(resident.Id, resident.TenantId, NormalizeContacts(dto.Contacts), now, cancellationToken);
        await ReplaceConditionsAsync(resident.MedicalProfile.Id, resident.TenantId, NormalizeConditions(dto.MedicalConditions), now, cancellationToken);

        if (oldRoomId != dto.RoomId)
        {
            var activeOccupancy = await db.RoomOccupancies
                .Where(item => item.ResidentId == resident.Id && item.EndsOn == null)
                .OrderByDescending(item => item.StartsOn)
                .FirstOrDefaultAsync(cancellationToken);
            if (activeOccupancy is not null)
            {
                activeOccupancy.EndsOn = DateOnly.FromDateTime(DateTime.Today);
                activeOccupancy.UpdatedAt = now;
            }

            if (dto.RoomId.HasValue)
            {
                db.RoomOccupancies.Add(new RoomOccupancy
                {
                    TenantId = resident.TenantId,
                    RoomId = dto.RoomId.Value,
                    ResidentId = resident.Id,
                    StartsOn = DateOnly.FromDateTime(DateTime.Today),
                    CreatedAt = now
                });
            }
        }

        AddAudit(resident.TenantId, "UpdateResident", nameof(Resident), resident.Id, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UploadPhotoAsync(Guid residentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var resident = await db.Residents.FirstOrDefaultAsync(item => item.Id == residentId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Resident not found.");

        var now = DateTimeOffset.UtcNow;
        var path = await fileStorage.SaveAsync(content, fileName, contentType, cancellationToken);
        resident.PhotoPath = path;
        resident.UpdatedAt = now;
        AddAudit(resident.TenantId, "UploadResidentPhoto", nameof(Resident), resident.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return path;
    }

    public async Task<ResidentDocumentViewModel> UploadDocumentAsync(Guid residentId, ResidentDocumentUploadDto dto, Stream content, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
    {
        var resident = await db.Residents
            .Include(item => item.Documents)
            .FirstOrDefaultAsync(item => item.Id == residentId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Resident not found.");

        var now = DateTimeOffset.UtcNow;
        var path = await fileStorage.SaveAsync(content, fileName, contentType, cancellationToken);
        var document = new ResidentDocument
        {
            TenantId = resident.TenantId,
            ResidentId = resident.Id,
            DocumentType = ParseEnum(dto.DocumentType, ResidentDocumentType.Other),
            Title = dto.Title.Trim(),
            FileName = fileName,
            FilePath = path,
            ContentType = contentType,
            FileSize = fileSize,
            ExpiryDate = dto.ExpiryDate,
            UploadedBy = "system",
            Notes = dto.Notes.Trim().NullIfEmpty(),
            IsConfidential = dto.IsConfidential,
            CreatedAt = now
        };

        resident.Documents.Add(document);
        AddAudit(resident.TenantId, "UploadResidentDocument", nameof(ResidentDocument), document.Id, now);
        await db.SaveChangesAsync(cancellationToken);
        return MapDocument(document);
    }

    public async Task DeleteDocumentAsync(Guid residentId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await db.ResidentDocuments
            .FirstOrDefaultAsync(item => item.Id == documentId && item.ResidentId == residentId && !item.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var now = DateTimeOffset.UtcNow;
        document.IsDeleted = true;
        document.DeletedAt = now;
        document.UpdatedAt = now;

        AddAudit(document.TenantId, "DeleteResidentDocument", nameof(ResidentDocument), document.Id, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ResidentContactEditorDto MapContact(ResidentContact contact) =>
        new(
            contact.Id,
            contact.FullName,
            contact.Relationship,
            contact.Job ?? string.Empty,
            contact.Address ?? string.Empty,
            contact.PhoneNumber ?? string.Empty,
            contact.MobileNumber ?? string.Empty,
            contact.IsPrimary,
            contact.IsEmergencyContact,
            contact.Notes ?? string.Empty);

    private static ResidentDocumentViewModel MapDocument(ResidentDocument document) =>
        new(
            document.Id,
            document.DocumentType.ToString(),
            document.Title,
            document.FileName,
            document.FilePath,
            document.ContentType,
            document.FileSize,
            document.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            document.ExpiryDate?.ToString("yyyy-MM-dd"),
            document.Notes ?? string.Empty,
            document.IsConfidential);

    private static IReadOnlyList<ResidentMedicalConditionEditorDto> BuildConditionRows(ResidentMedicalProfile? profile)
    {
        return MergeConditionEditors(profile?.Conditions);
    }

    private static IReadOnlyList<ResidentMedicalConditionEditorDto> MergeConditionEditors(IEnumerable<ResidentMedicalCondition>? conditions)
    {
        var existing = conditions?
            .Where(condition => !condition.IsDeleted)
            .ToDictionary(condition => condition.ConditionCode) ?? [];

        return Enum.GetValues<MedicalConditionCode>()
            .Select(code =>
            {
                if (existing.TryGetValue(code, out var condition))
                {
                    return new ResidentMedicalConditionEditorDto(condition.Id, code.ToString(), condition.ConditionName, condition.HasCondition, condition.Notes ?? string.Empty);
                }

                return new ResidentMedicalConditionEditorDto(null, code.ToString(), code.ToString(), false, string.Empty);
            })
            .ToList();
    }

    private static IReadOnlyList<ResidentMedicalConditionEditorDto> CreateDefaultConditionEditors() =>
        Enum.GetValues<MedicalConditionCode>()
            .Select(code => new ResidentMedicalConditionEditorDto(null, code.ToString(), code.ToString(), false, string.Empty))
            .ToList();

    private static IReadOnlyList<ResidentContactEditorDto> NormalizeContacts(IReadOnlyList<ResidentContactEditorDto> contacts)
    {
        var normalizedContacts = contacts
            .Where(contact =>
                !string.IsNullOrWhiteSpace(contact.FullName) ||
                !string.IsNullOrWhiteSpace(contact.Relationship) ||
                !string.IsNullOrWhiteSpace(contact.Job) ||
                !string.IsNullOrWhiteSpace(contact.Address) ||
                !string.IsNullOrWhiteSpace(contact.PhoneNumber) ||
                !string.IsNullOrWhiteSpace(contact.MobileNumber) ||
                !string.IsNullOrWhiteSpace(contact.Notes))
            .Select(contact => contact with
            {
                FullName = contact.FullName.Trim(),
                Relationship = contact.Relationship.Trim(),
                Job = contact.Job.Trim(),
                Address = contact.Address.Trim(),
                PhoneNumber = contact.PhoneNumber.Trim(),
                MobileNumber = contact.MobileNumber.Trim(),
                Notes = contact.Notes.Trim()
            })
            .ToList();

        if (normalizedContacts.Count > 0 && normalizedContacts.All(contact => !contact.IsPrimary))
        {
            normalizedContacts[0] = normalizedContacts[0] with { IsPrimary = true };
        }

        if (normalizedContacts.Count(contact => contact.IsPrimary) > 1)
        {
            var foundPrimary = false;
            normalizedContacts = normalizedContacts.Select(contact =>
            {
                if (!contact.IsPrimary)
                {
                    return contact;
                }

                if (!foundPrimary)
                {
                    foundPrimary = true;
                    return contact;
                }

                return contact with { IsPrimary = false };
            }).ToList();
        }

        return normalizedContacts;
    }

    private static IReadOnlyList<ResidentMedicalConditionEditorDto> NormalizeConditions(IReadOnlyList<ResidentMedicalConditionEditorDto> conditions) =>
        conditions
            .Where(condition => condition.HasCondition || !string.IsNullOrWhiteSpace(condition.Notes))
            .Select(condition => condition with
            {
                ConditionName = condition.ConditionName.Trim(),
                Notes = condition.Notes.Trim()
            })
            .ToList();

    private async Task ReplaceContactsAsync(
        Guid residentId,
        Guid tenantId,
        IReadOnlyList<ResidentContactEditorDto> incomingContacts,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var currentContacts = await db.ResidentContacts
            .Where(contact => contact.ResidentId == residentId && !contact.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var contact in currentContacts)
        {
            contact.IsDeleted = true;
            contact.DeletedAt = now;
            contact.UpdatedAt = now;
        }

        foreach (var incoming in incomingContacts)
        {
            db.ResidentContacts.Add(new ResidentContact
            {
                TenantId = tenantId,
                ResidentId = residentId,
                FullName = incoming.FullName,
                Relationship = incoming.Relationship,
                Job = incoming.Job.NullIfEmpty(),
                Address = incoming.Address.NullIfEmpty(),
                PhoneNumber = incoming.PhoneNumber.NullIfEmpty(),
                MobileNumber = incoming.MobileNumber.NullIfEmpty(),
                IsPrimary = incoming.IsPrimary,
                IsEmergencyContact = incoming.IsEmergencyContact,
                Notes = incoming.Notes.NullIfEmpty(),
                CreatedAt = now
            });
        }
    }

    private async Task ReplaceConditionsAsync(
        Guid medicalProfileId,
        Guid tenantId,
        IReadOnlyList<ResidentMedicalConditionEditorDto> incomingConditions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var currentConditions = await db.ResidentMedicalConditions
            .Where(condition => condition.ResidentMedicalProfileId == medicalProfileId && !condition.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var condition in currentConditions)
        {
            condition.IsDeleted = true;
            condition.DeletedAt = now;
            condition.UpdatedAt = now;
        }

        foreach (var incoming in incomingConditions)
        {
            db.ResidentMedicalConditions.Add(new ResidentMedicalCondition
            {
                TenantId = tenantId,
                ResidentMedicalProfileId = medicalProfileId,
                ConditionCode = ParseEnum(incoming.ConditionCode, MedicalConditionCode.Anemia),
                ConditionName = incoming.ConditionName,
                HasCondition = incoming.HasCondition,
                Notes = incoming.Notes.NullIfEmpty(),
                CreatedAt = now
            });
        }
    }

    private async Task<IReadOnlyList<ResidentEditorOptionDto>> LoadRoomOptions(CancellationToken cancellationToken)
    {
        var rooms = await db.Rooms.AsNoTracking()
            .Include(room => room.Floor)
            .OrderBy(room => room.Floor!.Number)
            .ThenBy(room => room.Number)
            .ToListAsync(cancellationToken);

        return rooms.Select(room => new ResidentEditorOptionDto(room.Id, RoomLabel(room.Number, room.Floor?.Name))).ToList();
    }

    private async Task<List<MedicationRowDto>> LoadResidentMedications(Guid residentId, CancellationToken cancellationToken)
    {
        var rows = await (
            from residentMedicine in db.ResidentMedicines.AsNoTracking()
            join medicine in db.Medicines.AsNoTracking() on residentMedicine.MedicineId equals medicine.Id
            join schedule in db.MedicineSchedules.AsNoTracking() on residentMedicine.Id equals schedule.ResidentMedicineId
            where residentMedicine.ResidentId == residentId && residentMedicine.IsActive
            orderby schedule.DoseTime
            select new
            {
                Medicine = medicine.Name + " " + medicine.Strength,
                schedule.Quantity,
                schedule.DoseTime,
                residentMedicine.Instructions,
                schedule.Timing,
                residentMedicine.IsActive
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new MedicationRowDto(
            string.Empty,
            row.Medicine.Trim(),
            row.Quantity.ToString("0.##"),
            row.DoseTime.ToString("HH:mm"),
            row.Timing.ToString(),
            string.Empty,
            row.IsActive ? "Active" : "Inactive",
            row.IsActive ? string.Empty : "warn")).ToList();
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
            Name = "Saint Stephen Care",
            LegalName = "Saint Stephen Care",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }

    private static void EnsureResidentIsValid(ResidentUpsertDto dto)
    {
        var issues = ResidentUpsertBusinessRules.Validate(dto);
        if (issues.Count == 0)
        {
            return;
        }

        throw new ValidationException(string.Join(Environment.NewLine, issues.Select(issue => issue.DefaultMessage)));
    }

    private void AddAudit(Guid tenantId, string operation, string entityName, Guid entityId, DateTimeOffset now)
    {
        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenantId,
            ActorId = "system",
            Operation = operation,
            EntityName = entityName,
            EntityId = entityId,
            OccurredAt = now
        });
    }

    private static string CalculateAgeYears(DateOnly? birthDate)
    {
        if (birthDate is null)
        {
            return string.Empty;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var years = today.Year - birthDate.Value.Year;
        if (birthDate.Value > today.AddYears(-years))
        {
            years--;
        }

        return years.ToString();
    }

    private static string StatusState(ResidentStatus status) => status switch
    {
        ResidentStatus.Active => string.Empty,
        ResidentStatus.TemporarilyAway => "warn",
        _ => "danger"
    };

    private static string RoomLabel(string number, string? floor) =>
        string.IsNullOrWhiteSpace(floor) ? number : $"{number} - {floor}";

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, out var parsed) ? parsed : fallback;
}

internal static class ResidentManagementExtensions
{
    public static string? NullIfEmpty(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
