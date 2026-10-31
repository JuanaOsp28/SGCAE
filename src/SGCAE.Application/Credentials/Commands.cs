using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Application.Employees;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Credentials;

// ---------- Asignar credencial ----------

/// <param name="Code">Código de la credencial. Si se omite se genera uno automáticamente.</param>
public sealed record IssueCredentialCommand(Guid EmployeeId, string? Code) : ICommand<CredentialDto>;

internal sealed class IssueCredentialHandler(
    IEmployeeRepository employees, ICredentialRepository credentials,
    IUnitOfWork unitOfWork, IReadDbContext db, TimeProvider clock)
    : ICommandHandler<IssueCredentialCommand, CredentialDto>
{
    public async Task<Result<CredentialDto>> Handle(IssueCredentialCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return EmployeeErrors.NotFound(command.EmployeeId);

        var code = string.IsNullOrWhiteSpace(command.Code) ? CredentialCode.Generate() : CredentialCode.Create(command.Code);
        if (await credentials.ExistsWithCodeAsync(code, cancellationToken))
            return CredentialErrors.DuplicateCode;

        var hasCurrent = await credentials.EmployeeHasCurrentCredentialAsync(employee.Id, cancellationToken);
        var credential = Credential.Issue(employee, code, hasCurrent, clock.GetUtcNow().UtcDateTime);

        credentials.Add(credential);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await CredentialReadModel.Project(db.Credentials.Where(c => c.Id == credential.Id), db).FirstAsync(cancellationToken);
    }
}

// ---------- Bloquear / desbloquear / revocar ----------

public sealed record BlockCredentialCommand(Guid CredentialId, string Reason) : ICommand<CredentialDto>;

public sealed record UnblockCredentialCommand(Guid CredentialId) : ICommand<CredentialDto>;

public sealed record RevokeCredentialCommand(Guid CredentialId, string Reason) : ICommand<CredentialDto>;

internal sealed class ChangeCredentialStatusHandler(
    ICredentialRepository credentials, IUnitOfWork unitOfWork, IReadDbContext db, TimeProvider clock)
    : ICommandHandler<BlockCredentialCommand, CredentialDto>,
      ICommandHandler<UnblockCredentialCommand, CredentialDto>,
      ICommandHandler<RevokeCredentialCommand, CredentialDto>
{
    public Task<Result<CredentialDto>> Handle(BlockCredentialCommand command, CancellationToken cancellationToken) =>
        Change(command.CredentialId, (c, now) => c.Block(command.Reason, now), cancellationToken);

    public Task<Result<CredentialDto>> Handle(UnblockCredentialCommand command, CancellationToken cancellationToken) =>
        Change(command.CredentialId, (c, now) => c.Unblock(now), cancellationToken);

    public Task<Result<CredentialDto>> Handle(RevokeCredentialCommand command, CancellationToken cancellationToken) =>
        Change(command.CredentialId, (c, now) => c.Revoke(command.Reason, now), cancellationToken);

    private async Task<Result<CredentialDto>> Change(
        Guid credentialId, Action<Credential, DateTime> transition, CancellationToken cancellationToken)
    {
        var credential = await credentials.GetByIdAsync(credentialId, cancellationToken);
        if (credential is null)
            return CredentialErrors.NotFound(credentialId);

        transition(credential, clock.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await CredentialReadModel.Project(db.Credentials.Where(c => c.Id == credentialId), db).FirstAsync(cancellationToken);
    }
}
