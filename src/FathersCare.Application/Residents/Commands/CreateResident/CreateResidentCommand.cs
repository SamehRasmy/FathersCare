namespace FathersCare.Application.Residents.Commands.CreateResident;

public sealed record CreateResidentCommand(string Code, string FullName, DateOnly? BirthDate);
