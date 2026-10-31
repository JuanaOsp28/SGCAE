using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.Credentials;

namespace SGCAE.Application.Credentials;

// ---------- Consultar credencial por id ----------

public sealed record GetCredentialByIdQuery(Guid CredentialId) : IQuery<CredentialDto>;

internal sealed class GetCredentialByIdHandler(IReadDbContext db) : IQueryHandler<GetCredentialByIdQuery, CredentialDto>
{
    public async Task<Result<CredentialDto>> Handle(GetCredentialByIdQuery query, CancellationToken cancellationToken)
    {
        var credential = await CredentialReadModel
            .Project(db.Credentials.Where(c => c.Id == query.CredentialId), db)
            .FirstOrDefaultAsync(cancellationToken);
        return credential is null ? CredentialErrors.NotFound(query.CredentialId) : credential;
    }
}

// ---------- Consultar estado de una credencial por código ----------

public sealed record GetCredentialByCodeQuery(string Code) : IQuery<CredentialDto>;

internal sealed class GetCredentialByCodeHandler(IReadDbContext db) : IQueryHandler<GetCredentialByCodeQuery, CredentialDto>
{
    public async Task<Result<CredentialDto>> Handle(GetCredentialByCodeQuery query, CancellationToken cancellationToken)
    {
        var code = (query.Code ?? string.Empty).Trim().ToUpperInvariant();
        var credential = await CredentialReadModel
            .Project(db.Credentials.Where(c => c.Code.Value == code), db)
            .FirstOrDefaultAsync(cancellationToken);
        return credential is null ? CredentialErrors.NotFound(code) : credential;
    }
}

// ---------- Listar credenciales (paginado) ----------

public sealed record ListCredentialsQuery(Guid? EmployeeId, CredentialStatus? Status, int? Page, int? PageSize)
    : IQuery<PagedResult<CredentialDto>>;

internal sealed class ListCredentialsHandler(IReadDbContext db) : IQueryHandler<ListCredentialsQuery, PagedResult<CredentialDto>>
{
    public async Task<Result<PagedResult<CredentialDto>>> Handle(ListCredentialsQuery query, CancellationToken cancellationToken)
    {
        var credentials = db.Credentials;

        if (query.EmployeeId is not null)
            credentials = credentials.Where(c => c.EmployeeId == query.EmployeeId);
        if (query.Status is not null)
            credentials = credentials.Where(c => c.Status == query.Status);

        return await CredentialReadModel
            .Project(credentials, db)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
