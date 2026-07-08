namespace FathersCare.Application.Abstractions;

public static class MedicationNavigationHelper
{
    public static string BuildRoute(Guid? residentId)
    {
        return residentId is null ? "/medications" : $"/medications/{residentId}";
    }
}
