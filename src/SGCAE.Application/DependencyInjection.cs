using Microsoft.Extensions.DependencyInjection;
using SGCAE.Application.Abstractions;

namespace SGCAE.Application;

public static class DependencyInjection
{
    /// <summary>Registra el despachador y todos los manejadores de comandos y consultas del ensamblado.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        var handlerInterfaces = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };
        var registrations =
            from type in typeof(DependencyInjection).Assembly.GetTypes()
            where type is { IsClass: true, IsAbstract: false }
            from contract in type.GetInterfaces()
            where contract.IsGenericType && handlerInterfaces.Contains(contract.GetGenericTypeDefinition())
            select (contract, type);

        foreach (var (contract, implementation) in registrations)
            services.AddScoped(contract, implementation);

        return services;
    }
}
