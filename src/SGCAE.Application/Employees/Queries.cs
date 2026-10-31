using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Employees;

// ---------- Consultar empleado por id ----------

public sealed record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeDto>;

internal sealed class GetEmployeeByIdHandler(IReadDbContext db) : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .Where(e => e.Id == query.EmployeeId)
            .Select(EmployeeDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return employee is null ? EmployeeErrors.NotFound(query.EmployeeId) : employee;
    }
}

// ---------- Listar empleados (paginado) ----------

/// <param name="Search">Texto a buscar en identificación, nombres, apellidos o correo.</param>
public sealed record ListEmployeesQuery(
    string? Search, string? Area, EmploymentStatus? Status, int? Page, int? PageSize)
    : IQuery<PagedResult<EmployeeDto>>;

internal sealed class ListEmployeesHandler(IReadDbContext db) : IQueryHandler<ListEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<Result<PagedResult<EmployeeDto>>> Handle(ListEmployeesQuery query, CancellationToken cancellationToken)
    {
        var employees = db.Employees;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            employees = employees.Where(e =>
                e.Identification.Value.Contains(term) ||
                e.Name.FirstNames.Contains(term) ||
                e.Name.LastNames.Contains(term) ||
                e.Email.Value.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.Area))
            employees = employees.Where(e => e.Area == query.Area.Trim());
        if (query.Status is not null)
            employees = employees.Where(e => e.Status == query.Status);

        return await employees
            .OrderBy(e => e.Name.LastNames).ThenBy(e => e.Name.FirstNames)
            .Select(EmployeeDto.Projection)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
