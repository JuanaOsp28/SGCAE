using System.Linq.Expressions;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Employees;

public sealed record EmployeeDto(
    Guid Id,
    string Identification,
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area,
    EmploymentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? DeactivatedAtUtc)
{
    /// <summary>Proyección traducible a SQL, usada por las consultas.</summary>
    public static readonly Expression<Func<Employee, EmployeeDto>> Projection = e => new EmployeeDto(
        e.Id, e.Identification.Value, e.Name.FirstNames, e.Name.LastNames, e.Email.Value,
        e.Position, e.Area, e.Status, e.CreatedAtUtc, e.DeactivatedAtUtc);

    private static readonly Func<Employee, EmployeeDto> Map = Projection.Compile();

    public static EmployeeDto From(Employee employee) => Map(employee);
}
