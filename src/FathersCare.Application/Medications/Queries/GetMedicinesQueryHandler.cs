using FathersCare.Application.Abstractions;
using FathersCare.Domain.Medications;

namespace FathersCare.Application.Medications.Queries;

public sealed class GetMedicinesQueryHandler(IMedicationRepository repository) : IQueryHandler<GetMedicinesQuery, IReadOnlyList<MedicineDto>>
{
    public async Task<IReadOnlyList<MedicineDto>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var medicines = await repository.GetMedicinesAsync(cancellationToken);
        return medicines.Select(m => new MedicineDto(m.Id, m.Name, m.Strength, m.Form)).ToList();
    }
}