using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Queries;

public sealed class GetMedicineSchedulesQueryHandler(IMedicationRepository repository) : IQueryHandler<GetMedicineSchedulesQuery, IReadOnlyList<MedicineScheduleDto>>
{
    public async Task<IReadOnlyList<MedicineScheduleDto>> Handle(GetMedicineSchedulesQuery request, CancellationToken cancellationToken)
    {
        var schedules = await repository.GetMedicineSchedulesAsync(request.ResidentMedicineId, cancellationToken);
        return schedules.Select(s => new MedicineScheduleDto(
            s.Id,
            s.DoseTime,
            s.Quantity,
            s.Timing)).ToList();
    }
}