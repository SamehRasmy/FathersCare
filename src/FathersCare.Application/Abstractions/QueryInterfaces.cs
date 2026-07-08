namespace FathersCare.Application.Abstractions;

public interface IQuery<TResponse>
{
    Guid Id { get; }
}

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
}