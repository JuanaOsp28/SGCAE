using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Domain.AccessPoints;

namespace SGCAE.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica las migraciones pendientes y, si no hay puntos de acceso, registra
    /// los puntos básicos de la sede principal.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SgcaeDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.AccessPointSet.AnyAsync(cancellationToken))
            return;

        var now = clock.GetUtcNow().UtcDateTime;
        db.AccessPointSet.AddRange(
            AccessPoint.Register("RECEPCION", "Recepción principal", AccessPointType.Reception, "Sede principal - Piso 1", now),
            AccessPoint.Register("PARQUEADERO", "Parqueadero empleados", AccessPointType.Parking, "Sede principal - Sótano", now),
            AccessPoint.Register("BODEGA-01", "Bodega de almacenamiento 1", AccessPointType.Warehouse, "Sede principal - Patio de carga", now),
            AccessPoint.Register("CUARTO-SERV", "Cuarto de servidores", AccessPointType.RestrictedArea, "Sede principal - Piso 3", now));
        await db.SaveChangesAsync(cancellationToken);
    }
}
