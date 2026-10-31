using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.AccessRecords;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessRecords;

namespace SGCAE.Api.Controllers;

/// <summary>RF-03 y RF-04. Registro de entradas/salidas y consulta del historial.</summary>
[Route("api/access-records")]
public sealed class AccessRecordsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>
    /// Procesa una solicitud de acceso. El intento siempre queda registrado: el campo
    /// <c>result</c> indica si fue autorizado y <c>rejectionReason</c> el motivo de rechazo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<AccessRecordDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Register(RegisterAccessCommand command, CancellationToken cancellationToken) =>
        ExecuteCreated(command, nameof(GetById), r => new { id = r.Id }, cancellationToken);

    /// <summary>Historial de accesos filtrable y paginado.</summary>
    /// <param name="employeeId">Empleado.</param>
    /// <param name="fromUtc">Fecha/hora inicial (UTC, inclusiva). Ej: 2026-10-01T00:00:00Z</param>
    /// <param name="toUtc">Fecha/hora final (UTC, inclusiva).</param>
    /// <param name="accessPointId">Punto de acceso.</param>
    /// <param name="result">Authorized o Rejected.</param>
    /// <param name="movementType">Entry o Exit.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<AccessRecordDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> List(
        [FromQuery] Guid? employeeId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc,
        [FromQuery] Guid? accessPointId, [FromQuery] AccessResult? result, [FromQuery] MovementType? movementType,
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken) =>
        Execute(new ListAccessRecordsQuery(
            employeeId, ToUtc(fromUtc), ToUtc(toUtc), accessPointId, result, movementType, page, pageSize), cancellationToken);

    /// <summary>Consulta un registro de acceso por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccessRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Execute(new GetAccessRecordByIdQuery(id), cancellationToken);

    private static DateTime? ToUtc(DateTime? value) => value?.Kind switch
    {
        DateTimeKind.Local => value.Value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
        _ => value
    };
}
