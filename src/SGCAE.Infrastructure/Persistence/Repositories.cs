using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Infrastructure.Persistence;

internal sealed class EmployeeRepository(SgcaeDbContext db) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.EmployeeSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExistsWithIdentificationAsync(Identification identification, CancellationToken cancellationToken = default) =>
        db.EmployeeSet.AnyAsync(e => e.Identification.Value == identification.Value, cancellationToken);

    public Task<bool> ExistsWithEmailAsync(Email email, Guid? excludingEmployeeId = null, CancellationToken cancellationToken = default) =>
        db.EmployeeSet.AnyAsync(e => e.Email.Value == email.Value && e.Id != excludingEmployeeId, cancellationToken);

    public void Add(Employee employee) => db.EmployeeSet.Add(employee);
}

internal sealed class CredentialRepository(SgcaeDbContext db) : ICredentialRepository
{
    public Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.CredentialSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Credential?> GetByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default) =>
        db.CredentialSet.FirstOrDefaultAsync(c => c.Code.Value == code.Value, cancellationToken);

    public Task<bool> ExistsWithCodeAsync(CredentialCode code, CancellationToken cancellationToken = default) =>
        db.CredentialSet.AnyAsync(c => c.Code.Value == code.Value, cancellationToken);

    public Task<bool> EmployeeHasCurrentCredentialAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        db.CredentialSet.AnyAsync(c => c.EmployeeId == employeeId && c.Status != CredentialStatus.Revoked, cancellationToken);

    public async Task<IReadOnlyList<Credential>> GetCurrentByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        await db.CredentialSet
            .Where(c => c.EmployeeId == employeeId && c.Status != CredentialStatus.Revoked)
            .ToListAsync(cancellationToken);

    public void Add(Credential credential) => db.CredentialSet.Add(credential);
}

internal sealed class AccessPointRepository(SgcaeDbContext db) : IAccessPointRepository
{
    public Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.AccessPointSet.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<AccessPoint?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.AccessPointSet.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

    public Task<bool> ExistsWithCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.AccessPointSet.AnyAsync(p => p.Code == code, cancellationToken);

    public void Add(AccessPoint accessPoint) => db.AccessPointSet.Add(accessPoint);
}

internal sealed class AccessRecordRepository(SgcaeDbContext db) : IAccessRecordRepository
{
    public async Task<MovementType?> GetLastAuthorizedMovementAsync(
        Guid employeeId, Guid accessPointId, CancellationToken cancellationToken = default) =>
        await db.AccessRecordSet
            .Where(r => r.EmployeeId == employeeId && r.AccessPointId == accessPointId && r.Result == AccessResult.Authorized)
            .OrderByDescending(r => r.OccurredAtUtc)
            .Select(r => (MovementType?)r.MovementType)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(AccessRecord record) => db.AccessRecordSet.Add(record);
}
