using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessRecords;

namespace SGCAE.Application.AccessRecords;

// ---------- Consultar un registro ----------

public sealed record GetAccessRecordByIdQuery(Guid AccessRecordId) : IQuery<AccessRecordDto>;

internal sealed class GetAccessRecordByIdHandler(IReadDbContext db) : IQueryHandler<GetAccessRecordByIdQuery, AccessRecordDto>
{
    public async Task<Result<AccessRecordDto>> Handle(GetAccessRecordByIdQuery query, CancellationToken cancellationToken)
    {
        var record = await AccessRecordReadModel
            .Project(db.AccessRecords.Where(r => r.Id == query.AccessRecordId), db)
            .FirstOrDefaultAsync(cancellationToken);

        return record is null
            ? Error.NotFound("Acceso.NoEncontrado", $"No existe un registro de acceso con id '{query.AccessRecordId}'.")
            : record;
    }
}

// ---------- Historial de accesos (paginado) ----------

/// <summary>
/// Historial filtrable por empleado, rango de fechas (UTC, extremos inclusivos),
/// punto de acceso, resultado de la validación y tipo de movimiento.
/// </summary>
public sealed record ListAccessRecordsQuery(
    Guid? EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? AccessPointId,
    AccessResult? Result,
    MovementType? MovementType,
    int? Page,
    int? PageSize) : IQuery<PagedResult<AccessRecordDto>>;

internal sealed class ListAccessRecordsHandler(IReadDbContext db)
    : IQueryHandler<ListAccessRecordsQuery, PagedResult<AccessRecordDto>>
{
    public async Task<Result<PagedResult<AccessRecordDto>>> Handle(ListAccessRecordsQuery query, CancellationToken cancellationToken)
    {
        if (query.FromUtc > query.ToUtc)
            return Error.Validation("Historial.RangoInvalido", "La fecha inicial no puede ser posterior a la fecha final.");

        var records = db.AccessRecords;

        if (query.EmployeeId is not null)
            records = records.Where(r => r.EmployeeId == query.EmployeeId);
        if (query.FromUtc is not null)
            records = records.Where(r => r.OccurredAtUtc >= query.FromUtc);
        if (query.ToUtc is not null)
            records = records.Where(r => r.OccurredAtUtc <= query.ToUtc);
        if (query.AccessPointId is not null)
            records = records.Where(r => r.AccessPointId == query.AccessPointId);
        if (query.Result is not null)
            records = records.Where(r => r.Result == query.Result);
        if (query.MovementType is not null)
            records = records.Where(r => r.MovementType == query.MovementType);

        return await AccessRecordReadModel
            .Project(records, db)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
