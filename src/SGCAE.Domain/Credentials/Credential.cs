using SGCAE.Domain.Common;
using SGCAE.Domain.Employees;

namespace SGCAE.Domain.Credentials;

/// <summary>
/// Agregado Credencial. Ciclo de vida:
///   Active  -- Block   --> Blocked
///   Blocked -- Unblock --> Active
///   Active | Blocked -- Revoke --> Revoked (estado final)
/// Solo una credencial Active puede utilizarse para acceder.
/// </summary>
public sealed class Credential : AggregateRoot
{
    public const int ReasonMaxLength = 250;

    public CredentialCode Code { get; private set; } = null!;
    public Guid EmployeeId { get; private set; }
    public CredentialStatus Status { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? StatusChangedAtUtc { get; private set; }
    public string? StatusReason { get; private set; }

    private Credential() { }

    public bool CanBeUsed => Status == CredentialStatus.Active;

    /// <summary>
    /// Asigna una credencial a un empleado. El empleado debe estar activo y no tener
    /// otra credencial vigente o bloqueada (debe revocarse antes de emitir una nueva).
    /// </summary>
    public static Credential Issue(Employee employee, CredentialCode code, bool employeeHasCurrentCredential, DateTime nowUtc)
    {
        if (!employee.IsActive)
            throw new BusinessRuleException("Credencial.EmpleadoInactivo",
                "No se pueden asignar credenciales a un empleado inactivo.");
        if (employeeHasCurrentCredential)
            throw new BusinessRuleException("Credencial.YaAsignada",
                "El empleado ya tiene una credencial vigente o bloqueada. Revóquela antes de asignar una nueva.");

        var credential = new Credential
        {
            Id = Guid.NewGuid(),
            Code = code,
            EmployeeId = employee.Id,
            Status = CredentialStatus.Active,
            IssuedAtUtc = nowUtc
        };
        credential.Raise(new CredentialIssued(credential.Id, employee.Id, nowUtc));
        return credential;
    }

    public void Block(string? reason, DateTime nowUtc)
    {
        if (Status != CredentialStatus.Active)
            throw new BusinessRuleException("Credencial.NoBloqueable", "Solo se pueden bloquear credenciales vigentes.");
        ChangeStatus(CredentialStatus.Blocked, Guard.Required(reason, "Motivo", ReasonMaxLength), nowUtc);
    }

    public void Unblock(DateTime nowUtc)
    {
        if (Status != CredentialStatus.Blocked)
            throw new BusinessRuleException("Credencial.NoDesbloqueable", "Solo se pueden desbloquear credenciales bloqueadas.");
        ChangeStatus(CredentialStatus.Active, null, nowUtc);
    }

    public void Revoke(string? reason, DateTime nowUtc)
    {
        if (Status == CredentialStatus.Revoked)
            throw new BusinessRuleException("Credencial.YaRevocada", "La credencial ya fue revocada.");
        ChangeStatus(CredentialStatus.Revoked, Guard.Required(reason, "Motivo", ReasonMaxLength), nowUtc);
    }

    private void ChangeStatus(CredentialStatus status, string? reason, DateTime nowUtc)
    {
        Status = status;
        StatusReason = reason;
        StatusChangedAtUtc = nowUtc;
    }
}

public sealed record CredentialIssued(Guid CredentialId, Guid EmployeeId, DateTime OccurredOnUtc) : IDomainEvent;
