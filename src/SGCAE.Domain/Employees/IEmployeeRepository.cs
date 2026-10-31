namespace SGCAE.Domain.Employees;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithIdentificationAsync(Identification identification, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(Email email, Guid? excludingEmployeeId = null, CancellationToken cancellationToken = default);
    void Add(Employee employee);
}
