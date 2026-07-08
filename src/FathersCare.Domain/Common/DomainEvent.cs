namespace FathersCare.Domain.Common;

public abstract record DomainEvent(Guid EntityId, DateTimeOffset OccurredAt);
