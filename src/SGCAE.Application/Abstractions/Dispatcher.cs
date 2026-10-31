using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Application.Common;
using SGCAE.Domain.Common;

namespace SGCAE.Application.Abstractions;

/// <summary>
/// Resuelve el manejador del comando o consulta desde el contenedor de dependencias y
/// traduce las excepciones de dominio a errores tipados.
/// </summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Invokers = new();

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var invoker = (IInvoker<TResponse>)Invokers.GetOrAdd(command.GetType(), type =>
            Activator.CreateInstance(typeof(CommandInvoker<,>).MakeGenericType(type, typeof(TResponse)))!);
        return Execute(() => invoker.Invoke(serviceProvider, command, cancellationToken));
    }

    public Task<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var invoker = (IInvoker<TResponse>)Invokers.GetOrAdd(query.GetType(), type =>
            Activator.CreateInstance(typeof(QueryInvoker<,>).MakeGenericType(type, typeof(TResponse)))!);
        return Execute(() => invoker.Invoke(serviceProvider, query, cancellationToken));
    }

    private static async Task<Result<TResponse>> Execute<TResponse>(Func<Task<Result<TResponse>>> action)
    {
        try
        {
            return await action();
        }
        catch (BusinessRuleException ex)
        {
            return Error.Conflict(ex.Code, ex.Message);
        }
        catch (DomainException ex)
        {
            return Error.Validation(ex.Code, ex.Message);
        }
    }

    private interface IInvoker<TResponse>
    {
        Task<Result<TResponse>> Invoke(IServiceProvider provider, object request, CancellationToken cancellationToken);
    }

    private sealed class CommandInvoker<TCommand, TResponse> : IInvoker<TResponse> where TCommand : ICommand<TResponse>
    {
        public Task<Result<TResponse>> Invoke(IServiceProvider provider, object request, CancellationToken cancellationToken) =>
            provider.GetRequiredService<ICommandHandler<TCommand, TResponse>>().Handle((TCommand)request, cancellationToken);
    }

    private sealed class QueryInvoker<TQuery, TResponse> : IInvoker<TResponse> where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Invoke(IServiceProvider provider, object request, CancellationToken cancellationToken) =>
            provider.GetRequiredService<IQueryHandler<TQuery, TResponse>>().Handle((TQuery)request, cancellationToken);
    }
}
