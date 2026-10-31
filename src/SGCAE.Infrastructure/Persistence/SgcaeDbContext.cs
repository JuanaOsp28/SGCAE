using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core sobre SQL Server. Actúa como unidad de trabajo para los comandos
/// y como modelo de lectura (sin seguimiento de cambios) para las consultas.
/// </summary>
public sealed class SgcaeDbContext(DbContextOptions<SgcaeDbContext> options)
    : DbContext(options), IUnitOfWork, IReadDbContext
{
    public DbSet<Employee> EmployeeSet => Set<Employee>();
    public DbSet<Credential> CredentialSet => Set<Credential>();
    public DbSet<AccessPoint> AccessPointSet => Set<AccessPoint>();
    public DbSet<AccessRecord> AccessRecordSet => Set<AccessRecord>();

    IQueryable<Employee> IReadDbContext.Employees => EmployeeSet.AsNoTracking();
    IQueryable<Credential> IReadDbContext.Credentials => CredentialSet.AsNoTracking();
    IQueryable<AccessPoint> IReadDbContext.AccessPoints => AccessPointSet.AsNoTracking();
    IQueryable<AccessRecord> IReadDbContext.AccessRecords => AccessRecordSet.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SgcaeDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Todas las fechas se guardan en UTC; al leerlas se marcan como UTC para serializarlas con "Z".
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        // Los eventos de dominio quedan disponibles hasta confirmar la transacción;
        // en esta versión no hay suscriptores, por lo que se descartan.
        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        return result;
    }
}
