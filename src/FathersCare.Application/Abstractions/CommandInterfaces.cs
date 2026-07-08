namespace FathersCare.Application.Abstractions;

public interface ICommand<TResponse>
{
    Guid Id { get; }
}

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}