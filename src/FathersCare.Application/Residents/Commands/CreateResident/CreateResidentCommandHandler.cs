using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Validation;

namespace FathersCare.Application.Residents.Commands.CreateResident;

public sealed class CreateResidentCommandHandler(IResidentManagementService residents)
    : ICommandHandler<CreateResidentCommand, Guid>
{
    public async Task<Guid> Handle(CreateResidentCommand request, CancellationToken cancellationToken)
    {
        ResidentCommandValidation.EnsureValid(request.Resident);

        return await residents.CreateAsync(request.Resident, cancellationToken);
    }
}
