namespace SGCAE.Domain.AccessRecords;

public interface IAccessRecordRepository
{
    /// <summary>
    /// Último movimiento autorizado del empleado en el punto de acceso,
    /// o null si nunca ha tenido movimientos autorizados allí.
    /// </summary>
    Task<MovementType?> GetLastAuthorizedMovementAsync(Guid employeeId, Guid accessPointId, CancellationToken cancellationToken = default);

    void Add(AccessRecord record);
}
