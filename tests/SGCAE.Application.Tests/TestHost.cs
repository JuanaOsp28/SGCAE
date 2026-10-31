using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Application;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;
using SGCAE.Infrastructure.Persistence;

namespace SGCAE.Application.Tests;

/// <summary>Reloj controlable para las pruebas.</summary>
public sealed class FakeClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
    public void Advance(TimeSpan by) => Now += by;
}

/// <summary>
/// Monta la capa de aplicación con la infraestructura real (repositorios y DbContext)
/// sobre una base de datos en memoria aislada por prueba.
/// </summary>
public sealed class TestHost : IDisposable
{
    private readonly ServiceProvider _provider;

    public FakeClock Clock { get; } = new();

    public TestHost()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<SgcaeDbContext>(o => o.UseInMemoryDatabase(databaseName));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SgcaeDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<SgcaeDbContext>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<IAccessPointRepository, AccessPointRepository>();
        services.AddScoped<IAccessRecordRepository, AccessRecordRepository>();
        services.AddSingleton<TimeProvider>(Clock);
        services.AddApplication();
        _provider = services.BuildServiceProvider();
    }

    /// <summary>Cada llamada usa un scope nuevo, como una petición HTTP independiente.</summary>
    public async Task<Result<T>> Send<T>(ICommand<T> command)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().Send(command);
    }

    public async Task<Result<T>> Query<T>(IQuery<T> query)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().Query(query);
    }

    public async Task<T> SendOk<T>(ICommand<T> command)
    {
        var result = await Send(command);
        Assert.True(result.IsSuccess, result.Error?.Message);
        return result.Value;
    }

    public async Task<T> QueryOk<T>(IQuery<T> query)
    {
        var result = await Query(query);
        Assert.True(result.IsSuccess, result.Error?.Message);
        return result.Value;
    }

    public void Dispose() => _provider.Dispose();
}
