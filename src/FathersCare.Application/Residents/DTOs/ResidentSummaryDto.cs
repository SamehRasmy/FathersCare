namespace FathersCare.Application.Residents.DTOs;

public sealed record ResidentSummaryDto(Guid Id, string Code, string FullName, string Status);
