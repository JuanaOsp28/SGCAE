using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Application.Credentials;
using SGCAE.Application.Employees;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Api.Controllers;

/// <summary>RF-01. Gestión de empleados.</summary>
[Route("api/employees")]
public sealed class EmployeesController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Registra un nuevo empleado (queda activo).</summary>
    [HttpPost]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Register(RegisterEmployeeCommand command, CancellationToken cancellationToken) =>
        ExecuteCreated(command, nameof(GetById), e => new { id = e.Id }, cancellationToken);

    /// <summary>Lista empleados con filtros y paginación.</summary>
    /// <param name="search">Texto contenido en identificación, nombres, apellidos o correo.</param>
    /// <param name="area">Área organizacional exacta.</param>
    /// <param name="status">Active o Inactive.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<EmployeeDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(
        [FromQuery] string? search, [FromQuery] string? area, [FromQuery] EmploymentStatus? status,
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken) =>
        Execute(new ListEmployeesQuery(search, area, status, page, pageSize), cancellationToken);

    /// <summary>Consulta un empleado por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Execute(new GetEmployeeByIdQuery(id), cancellationToken);

    /// <summary>Actualiza los datos de un empleado activo (la identificación no cambia).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken) =>
        Execute(new UpdateEmployeeCommand(id, request.FirstNames, request.LastNames, request.Email, request.Position, request.Area),
            cancellationToken);

    /// <summary>Desactiva al empleado y revoca sus credenciales vigentes o bloqueadas.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        Execute(new DeactivateEmployeeCommand(id), cancellationToken);

    /// <summary>Reactiva el vínculo laboral de un empleado inactivo.</summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken) =>
        Execute(new ReactivateEmployeeCommand(id), cancellationToken);

    /// <summary>Credenciales del empleado (paginado).</summary>
    [HttpGet("{id:guid}/credentials")]
    [ProducesResponseType<PagedResult<CredentialDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Credentials(
        Guid id, [FromQuery] CredentialStatus? status, [FromQuery] int? page, [FromQuery] int? pageSize,
        CancellationToken cancellationToken) =>
        Execute(new ListCredentialsQuery(id, status, page, pageSize), cancellationToken);
}

public sealed record UpdateEmployeeRequest(string FirstNames, string LastNames, string Email, string Position, string Area);
