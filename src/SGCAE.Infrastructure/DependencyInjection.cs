using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Application.Abstractions;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;
using SGCAE.Infrastructure.Persistence;

namespace SGCAE.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<SgcaeDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SgcaeDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<SgcaeDbContext>());

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<IAccessPointRepository, AccessPointRepository>();
        services.AddScoped<IAccessRecordRepository, AccessRecordRepository>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
