using SGCAE.Application.Common;

namespace SGCAE.Application.Abstractions;

/// <summary>Operación que modifica el estado del sistema (lado de escritura de CQRS).</summary>
public interface ICommand<TResponse>;

/// <summary>Operación de solo lectura (lado de lectura de CQRS).</summary>
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}

/// <summary>Punto único de entrada para ejecutar comandos y consultas desde la capa de presentación.</summary>
public interface IDispatcher
{
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
