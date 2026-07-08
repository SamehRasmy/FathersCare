using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Queries;

public sealed class GetResidentMedicinesQueryHandler(IMedicationRepository repository) : IQueryHandler<GetResidentMedicinesQuery, IReadOnlyList<ResidentMedicineDto>>
{
    public async Task<IReadOnlyList<ResidentMedicineDto>> Handle(GetResidentMedicinesQuery request, CancellationToken cancellationToken)
    {
        var residentMedicines = await repository.GetResidentMedicinesAsync(request.ResidentId, cancellationToken);
        return residentMedicines.Select(rm => new ResidentMedicineDto(
            rm.Id,
            rm.MedicineId,
            rm.Medicine?.Name ?? "",
            rm.Medicine?.Strength,
            rm.Instructions,
            rm.IsActive)).ToList();
    }
}