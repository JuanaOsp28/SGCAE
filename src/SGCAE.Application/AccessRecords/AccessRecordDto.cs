using SGCAE.Application.Abstractions;
using SGCAE.Domain.AccessRecords;

namespace SGCAE.Application.AccessRecords;

public sealed record AccessRecordDto(
    Guid Id,
    DateTime OccurredAtUtc,
    MovementType MovementType,
    AccessResult Result,
    AccessRejectionReason? RejectionReason,
    string RequestedCredentialCode,
    string RequestedAccessPointCode,
    Guid? EmployeeId,
    string? EmployeeIdentification,
    string? EmployeeName,
    Guid? AccessPointId,
    string? AccessPointName)
{
    /// <summary>Descripción legible del motivo de rechazo.</summary>
    public string? RejectionMessage => RejectionReason switch
    {
        null => null,
        AccessRejectionReason.CredentialNotFound => "La credencial no está registrada.",
        AccessRejectionReason.CredentialBlocked => "La credencial está bloqueada.",
        AccessRejectionReason.CredentialRevoked => "La credencial fue revocada.",
        AccessRejectionReason.EmployeeInactive => "El empleado no tiene un vínculo laboral activo.",
        AccessRejectionReason.AccessPointNotFound => "El punto de acceso no está registrado.",
        AccessRejectionReason.AccessPointDisabled => "El punto de acceso está deshabilitado.",
        AccessRejectionReason.AlreadyInside => "El empleado ya registró una entrada en este punto sin su salida.",
        AccessRejectionReason.NotInside => "El empleado no tiene una entrada previa en este punto.",
        _ => RejectionReason.ToString()
    };
}

internal static class AccessRecordReadModel
{
    /// <summary>
    /// Proyecta los registros (ya filtrados) con los datos del empleado y del punto de acceso,
    /// del más reciente al más antiguo. Usa left join porque los rechazados pueden no tenerlos.
    /// </summary>
    public static IQueryable<AccessRecordDto> Project(IQueryable<AccessRecord> records, IReadDbContext db) =>
        from r in records
        join e in db.Employees on r.EmployeeId equals e.Id into employees
        from e in employees.DefaultIfEmpty()
        join p in db.AccessPoints on r.AccessPointId equals p.Id into points
        from p in points.DefaultIfEmpty()
        orderby r.OccurredAtUtc descending
        select new AccessRecordDto(
            r.Id, r.OccurredAtUtc, r.MovementType, r.Result, r.RejectionReason,
            r.RequestedCredentialCode, r.RequestedAccessPointCode,
            r.EmployeeId,
            e == null ? null : e.Identification.Value,
            e == null ? null : e.Name.FirstNames + " " + e.Name.LastNames,
            r.AccessPointId,
            p == null ? null : p.Name);
}
