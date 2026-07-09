using System.ComponentModel.DataAnnotations;
using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Validation;

namespace FathersCare.Application.Residents.Commands;

internal static class ResidentCommandValidation
{
    public static void EnsureValid(ResidentUpsertDto resident)
    {
        var issues = ResidentUpsertBusinessRules.Validate(resident);
        if (issues.Count > 0)
        {
            throw new ValidationException(string.Join(Environment.NewLine, issues.Select(issue => issue.DefaultMessage)));
        }
    }

    public static void EnsureExistingResidentId(Guid residentId)
    {
        if (residentId == Guid.Empty)
        {
            throw new ValidationException("Resident id is required.");
        }
    }
}
