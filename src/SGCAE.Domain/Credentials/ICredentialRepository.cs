namespace SGCAE.Domain.Credentials;

public interface ICredentialRepository
{
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Credential?> GetByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithCodeAsync(CredentialCode code, CancellationToken cancellationToken = default);

    /// <summary>Indica si el empleado tiene alguna credencial que no ha sido revocada.</summary>
    Task<bool> EmployeeHasCurrentCredentialAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>Credenciales no revocadas del empleado.</summary>
    Task<IReadOnlyList<Credential>> GetCurrentByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

    void Add(Credential credential);
}
