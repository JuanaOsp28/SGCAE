using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Application.Credentials;
using SGCAE.Domain.Credentials;

namespace SGCAE.Api.Controllers;

/// <summary>RF-02. Gestión de credenciales.</summary>
[Route("api/credentials")]
public sealed class CredentialsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Asigna una credencial a un empleado activo. Si no se envía código se genera uno.</summary>
    [HttpPost]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Issue(IssueCredentialCommand command, CancellationToken cancellationToken) =>
        ExecuteCreated(command, nameof(GetById), c => new { id = c.Id }, cancellationToken);

    /// <summary>Lista credenciales con filtros y paginación.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<CredentialDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(
        [FromQuery] Guid? employeeId, [FromQuery] CredentialStatus? status,
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken) =>
        Execute(new ListCredentialsQuery(employeeId, status, page, pageSize), cancellationToken);

    /// <summary>Consulta una credencial por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Execute(new GetCredentialByIdQuery(id), cancellationToken);

    /// <summary>Consulta el estado de una credencial a partir de su código.</summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken) =>
        Execute(new GetCredentialByCodeQuery(code), cancellationToken);

    /// <summary>Bloquea temporalmente una credencial vigente.</summary>
    [HttpPost("{id:guid}/block")]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Block(Guid id, ReasonRequest request, CancellationToken cancellationToken) =>
        Execute(new BlockCredentialCommand(id, request.Reason), cancellationToken);

    /// <summary>Desbloquea una credencial bloqueada.</summary>
    [HttpPost("{id:guid}/unblock")]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken) =>
        Execute(new UnblockCredentialCommand(id), cancellationToken);

    /// <summary>Revoca definitivamente una credencial.</summary>
    [HttpPost("{id:guid}/revoke")]
    [ProducesResponseType<CredentialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Revoke(Guid id, ReasonRequest request, CancellationToken cancellationToken) =>
        Execute(new RevokeCredentialCommand(id, request.Reason), cancellationToken);
}

public sealed record ReasonRequest(string Reason);
