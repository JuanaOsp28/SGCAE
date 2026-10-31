namespace SGCAE.Domain.AccessPoints;

public interface IAccessPointRepository
{
    Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccessPoint?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithCodeAsync(string code, CancellationToken cancellationToken = default);
    void Add(AccessPoint accessPoint);
}
