using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Commands;

namespace FathersCare.Application.Residents.Commands.UpdateResident;

public sealed class UpdateResidentCommandHandler(IResidentManagementService residents)
    : ICommandHandler<UpdateResidentCommand, bool>
{
    public async Task<bool> Handle(UpdateResidentCommand request, CancellationToken cancellationToken)
    {
        ResidentCommandValidation.EnsureExistingResidentId(request.ResidentId);
        ResidentCommandValidation.EnsureValid(request.Resident);

        await residents.UpdateAsync(request.ResidentId, request.Resident, cancellationToken);
        return true;
    }
}
