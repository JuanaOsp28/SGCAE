using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.AccessPoints;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessPoints;

namespace SGCAE.Api.Controllers;

/// <summary>RF-06. Administración de puntos de acceso.</summary>
[Route("api/access-points")]
public sealed class AccessPointsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Registra un punto de acceso físico (queda habilitado).</summary>
    [HttpPost]
    [ProducesResponseType<AccessPointDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Register(RegisterAccessPointCommand command, CancellationToken cancellationToken) =>
        ExecuteCreated(command, nameof(GetById), p => new { id = p.Id }, cancellationToken);

    /// <summary>Lista puntos de acceso; permite filtrar los habilitados.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<AccessPointDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(
        [FromQuery] bool? isEnabled, [FromQuery] AccessPointType? type,
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken) =>
        Execute(new ListAccessPointsQuery(isEnabled, type, page, pageSize), cancellationToken);

    /// <summary>Consulta un punto de acceso por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccessPointDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Execute(new GetAccessPointByIdQuery(id), cancellationToken);

    /// <summary>Habilita un punto de acceso.</summary>
    [HttpPost("{id:guid}/enable")]
    [ProducesResponseType<AccessPointDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken) =>
        Execute(new EnableAccessPointCommand(id), cancellationToken);

    /// <summary>Deshabilita un punto de acceso: los movimientos en él serán rechazados.</summary>
    [HttpPost("{id:guid}/disable")]
    [ProducesResponseType<AccessPointDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken) =>
        Execute(new DisableAccessPointCommand(id), cancellationToken);
}
