using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Domain.Services;

/// <summary>
/// Servicio de dominio que decide si un movimiento se autoriza. La regla involucra
/// varios agregados (credencial, empleado, punto de acceso e historial), por eso no
/// pertenece a ninguno de ellos en particular.
///
/// Reglas, evaluadas en orden:
///  1. La credencial debe existir.
///  2. La credencial debe estar vigente (no bloqueada ni revocada).
///  3. El empleado titular debe tener vínculo laboral activo.
///  4. El punto de acceso debe existir y estar habilitado.
///  5. La secuencia debe ser coherente en el punto: no hay dos entradas seguidas
///     ni una salida sin entrada previa (evita registros duplicados).
/// </summary>
public static class AccessControlService
{
    public static AccessRecord Evaluate(
        string requestedCredentialCode,
        string requestedAccessPointCode,
        Credential? credential,
        Employee? employee,
        AccessPoint? accessPoint,
        MovementType movementType,
        MovementType? lastAuthorizedMovement,
        DateTime nowUtc)
    {
        var reason = GetRejectionReason(credential, employee, accessPoint, movementType, lastAuthorizedMovement);

        return AccessRecord.Create(
            requestedCredentialCode, requestedAccessPointCode,
            credential?.Id, credential?.EmployeeId, accessPoint?.Id,
            movementType, reason, nowUtc);
    }

    private static AccessRejectionReason? GetRejectionReason(
        Credential? credential, Employee? employee, AccessPoint? accessPoint,
        MovementType movementType, MovementType? lastAuthorizedMovement)
    {
        if (credential is null)
            return AccessRejectionReason.CredentialNotFound;
        if (credential.Status == CredentialStatus.Blocked)
            return AccessRejectionReason.CredentialBlocked;
        if (credential.Status == CredentialStatus.Revoked)
            return AccessRejectionReason.CredentialRevoked;
        if (employee is null || !employee.IsActive)
            return AccessRejectionReason.EmployeeInactive;
        if (accessPoint is null)
            return AccessRejectionReason.AccessPointNotFound;
        if (!accessPoint.IsEnabled)
            return AccessRejectionReason.AccessPointDisabled;
        if (movementType == MovementType.Entry && lastAuthorizedMovement == MovementType.Entry)
            return AccessRejectionReason.AlreadyInside;
        if (movementType == MovementType.Exit && lastAuthorizedMovement != MovementType.Entry)
            return AccessRejectionReason.NotInside;
        return null;
    }
}
