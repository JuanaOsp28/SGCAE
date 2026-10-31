using SGCAE.Application.Abstractions;
using SGCAE.Domain.Credentials;

namespace SGCAE.Application.Credentials;

public sealed record CredentialDto(
    Guid Id,
    string Code,
    Guid EmployeeId,
    string EmployeeIdentification,
    string EmployeeName,
    CredentialStatus Status,
    bool CanBeUsed,
    DateTime IssuedAtUtc,
    DateTime? StatusChangedAtUtc,
    string? StatusReason);

internal static class CredentialReadModel
{
    /// <summary>
    /// Proyecta las credenciales (ya filtradas) junto con los datos del empleado titular,
    /// de la más reciente a la más antigua.
    /// </summary>
    public static IQueryable<CredentialDto> Project(IQueryable<Credential> credentials, IReadDbContext db) =>
        from c in credentials
        join e in db.Employees on c.EmployeeId equals e.Id
        orderby c.IssuedAtUtc descending
        select new CredentialDto(
            c.Id, c.Code.Value, c.EmployeeId, e.Identification.Value,
            e.Name.FirstNames + " " + e.Name.LastNames,
            c.Status, c.Status == CredentialStatus.Active,
            c.IssuedAtUtc, c.StatusChangedAtUtc, c.StatusReason);
}
