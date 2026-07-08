using FathersCare.Application.Abstractions;

namespace FathersCare.UnitTests;

public class MedicationNavigationHelperTests
{
    [Fact]
    public void BuildRoute_ReturnsGeneralMedicationsRoute_WhenResidentIdMissing()
    {
        var route = MedicationNavigationHelper.BuildRoute(null);

        Assert.Equal("/medications", route);
    }

    [Fact]
    public void BuildRoute_ReturnsResidentSpecificRoute_WhenResidentIdProvided()
    {
        var residentId = Guid.NewGuid();

        var route = MedicationNavigationHelper.BuildRoute(residentId);

        Assert.Equal($"/medications/{residentId}", route);
    }
}
