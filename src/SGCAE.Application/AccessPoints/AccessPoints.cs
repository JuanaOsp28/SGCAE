using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.Common;

namespace SGCAE.Application.AccessPoints;

public sealed record AccessPointDto(
    Guid Id, string Code, string Name, AccessPointType Type, string? Location, bool IsEnabled, DateTime CreatedAtUtc)
{
    public static readonly Expression<Func<AccessPoint, AccessPointDto>> Projection = p =>
        new AccessPointDto(p.Id, p.Code, p.Name, p.Type, p.Location, p.IsEnabled, p.CreatedAtUtc);

    private static readonly Func<AccessPoint, AccessPointDto> Map = Projection.Compile();

    public static AccessPointDto From(AccessPoint accessPoint) => Map(accessPoint);
}

public static class AccessPointErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("PuntoAcceso.NoEncontrado", $"No existe un punto de acceso con id '{id}'.");

    public static readonly Error DuplicateCode =
        Error.Conflict("PuntoAcceso.CodigoDuplicado", "Ya existe un punto de acceso con ese código.");
}

// ---------- Registrar punto de acceso ----------

public sealed record RegisterAccessPointCommand(string Code, string Name, AccessPointType Type, string? Location)
    : ICommand<AccessPointDto>;

internal sealed class RegisterAccessPointHandler(IAccessPointRepository accessPoints, IUnitOfWork unitOfWork, TimeProvider clock)
    : ICommandHandler<RegisterAccessPointCommand, AccessPointDto>
{
    public async Task<Result<AccessPointDto>> Handle(RegisterAccessPointCommand command, CancellationToken cancellationToken)
    {
        var accessPoint = AccessPoint.Register(command.Code, command.Name, command.Type, command.Location, clock.GetUtcNow().UtcDateTime);
        if (await accessPoints.ExistsWithCodeAsync(accessPoint.Code, cancellationToken))
            return AccessPointErrors.DuplicateCode;

        accessPoints.Add(accessPoint);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AccessPointDto.From(accessPoint);
    }
}

// ---------- Habilitar / deshabilitar ----------

public sealed record EnableAccessPointCommand(Guid AccessPointId) : ICommand<AccessPointDto>;

public sealed record DisableAccessPointCommand(Guid AccessPointId) : ICommand<AccessPointDto>;

internal sealed class ChangeAccessPointStatusHandler(IAccessPointRepository accessPoints, IUnitOfWork unitOfWork)
    : ICommandHandler<EnableAccessPointCommand, AccessPointDto>,
      ICommandHandler<DisableAccessPointCommand, AccessPointDto>
{
    public Task<Result<AccessPointDto>> Handle(EnableAccessPointCommand command, CancellationToken cancellationToken) =>
        Change(command.AccessPointId, p => p.Enable(), cancellationToken);

    public Task<Result<AccessPointDto>> Handle(DisableAccessPointCommand command, CancellationToken cancellationToken) =>
        Change(command.AccessPointId, p => p.Disable(), cancellationToken);

    private async Task<Result<AccessPointDto>> Change(Guid id, Action<AccessPoint> transition, CancellationToken cancellationToken)
    {
        var accessPoint = await accessPoints.GetByIdAsync(id, cancellationToken);
        if (accessPoint is null)
            return AccessPointErrors.NotFound(id);

        transition(accessPoint);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AccessPointDto.From(accessPoint);
    }
}

// ---------- Consultas ----------

public sealed record GetAccessPointByIdQuery(Guid AccessPointId) : IQuery<AccessPointDto>;

public sealed record ListAccessPointsQuery(bool? IsEnabled, AccessPointType? Type, int? Page, int? PageSize)
    : IQuery<PagedResult<AccessPointDto>>;

internal sealed class AccessPointQueriesHandler(IReadDbContext db)
    : IQueryHandler<GetAccessPointByIdQuery, AccessPointDto>,
      IQueryHandler<ListAccessPointsQuery, PagedResult<AccessPointDto>>
{
    public async Task<Result<AccessPointDto>> Handle(GetAccessPointByIdQuery query, CancellationToken cancellationToken)
    {
        var accessPoint = await db.AccessPoints
            .Where(p => p.Id == query.AccessPointId)
            .Select(AccessPointDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);
        return accessPoint is null ? AccessPointErrors.NotFound(query.AccessPointId) : accessPoint;
    }

    public async Task<Result<PagedResult<AccessPointDto>>> Handle(ListAccessPointsQuery query, CancellationToken cancellationToken)
    {
        var accessPoints = db.AccessPoints;
        if (query.IsEnabled is not null)
            accessPoints = accessPoints.Where(p => p.IsEnabled == query.IsEnabled);
        if (query.Type is not null)
            accessPoints = accessPoints.Where(p => p.Type == query.Type);

        return await accessPoints
            .OrderBy(p => p.Code)
            .Select(AccessPointDto.Projection)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
