using SGCAE.Domain.Common;

namespace SGCAE.Domain.AccessRecords;

/// <summary>
/// Registro inmutable de un intento de entrada o salida. Se guardan tanto los
/// movimientos autorizados como los rechazados para garantizar la trazabilidad.
/// Los datos solicitados (códigos) se conservan aunque no correspondan a ninguna
/// credencial o punto de acceso existente.
/// </summary>
public sealed class AccessRecord : AggregateRoot
{
    public const int RequestedCodeMaxLength = 50;

    public string RequestedCredentialCode { get; private set; } = null!;
    public string RequestedAccessPointCode { get; private set; } = null!;
    public Guid? CredentialId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Guid? AccessPointId { get; private set; }
    public MovementType MovementType { get; private set; }
    public AccessResult Result { get; private set; }
    public AccessRejectionReason? RejectionReason { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AccessRecord() { }

    internal static AccessRecord Create(
        string requestedCredentialCode, string requestedAccessPointCode,
        Guid? credentialId, Guid? employeeId, Guid? accessPointId,
        MovementType movementType, AccessRejectionReason? rejectionReason, DateTime nowUtc)
    {
        if (!Enum.IsDefined(movementType))
            throw new DomainException("Movimiento.TipoInvalido", "El tipo de movimiento debe ser Entry o Exit.");

        return new AccessRecord
        {
            Id = Guid.NewGuid(),
            RequestedCredentialCode = Truncate(requestedCredentialCode),
            RequestedAccessPointCode = Truncate(requestedAccessPointCode),
            CredentialId = credentialId,
            EmployeeId = employeeId,
            AccessPointId = accessPointId,
            MovementType = movementType,
            Result = rejectionReason is null ? AccessResult.Authorized : AccessResult.Rejected,
            RejectionReason = rejectionReason,
            OccurredAtUtc = nowUtc
        };
    }

    public bool IsAuthorized => Result == AccessResult.Authorized;

    private static string Truncate(string value)
    {
        var trimmed = (value ?? string.Empty).Trim().ToUpperInvariant();
        return trimmed.Length <= RequestedCodeMaxLength ? trimmed : trimmed[..RequestedCodeMaxLength];
    }
}
