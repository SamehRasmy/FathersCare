using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Queries;

public sealed class GetDoseAdministrationsQueryHandler(IMedicationRepository repository) : IQueryHandler<GetDoseAdministrationsQuery, IReadOnlyList<DoseAdministrationDto>>
{
    public async Task<IReadOnlyList<DoseAdministrationDto>> Handle(GetDoseAdministrationsQuery request, CancellationToken cancellationToken)
    {
        var doses = await repository.GetDoseAdministrationsAsync(
            request.ResidentMedicineId,
            request.Date,
            cancellationToken);

        return doses.Select(d => new DoseAdministrationDto(
            d.Id,
            d.MedicineScheduleId,
            d.DoseDate,
            d.Status,
            d.AdministeredAt,
            d.AdministeredBy,
            d.Notes)).ToList();
    }
}
