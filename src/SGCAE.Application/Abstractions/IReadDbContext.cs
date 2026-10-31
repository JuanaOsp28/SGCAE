using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Abstractions;

/// <summary>
/// Modelo de lectura para las consultas (CQRS). Expone conjuntos sin seguimiento de
/// cambios: las consultas proyectan directamente a DTOs y nunca modifican estado.
/// </summary>
public interface IReadDbContext
{
    IQueryable<Employee> Employees { get; }
    IQueryable<Credential> Credentials { get; }
    IQueryable<AccessPoint> AccessPoints { get; }
    IQueryable<AccessRecord> AccessRecords { get; }
}
