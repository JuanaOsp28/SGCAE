using Microsoft.EntityFrameworkCore;
using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;
using SGCAE.Domain.Services;

namespace SGCAE.Application.AccessRecords;

/// <summary>
/// Solicitud de acceso enviada por un lector/torniquete. Siempre genera un registro,
/// tanto si el movimiento se autoriza como si se rechaza.
/// </summary>
public sealed record RegisterAccessCommand(string CredentialCode, string AccessPointCode, MovementType MovementType)
    : ICommand<AccessRecordDto>;

internal sealed class RegisterAccessHandler(
    ICredentialRepository credentials,
    IEmployeeRepository employees,
    IAccessPointRepository accessPoints,
    IAccessRecordRepository accessRecords,
    IUnitOfWork unitOfWork,
    IReadDbContext db,
    TimeProvider clock)
    : ICommandHandler<RegisterAccessCommand, AccessRecordDto>
{
    public async Task<Result<AccessRecordDto>> Handle(RegisterAccessCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CredentialCode) || string.IsNullOrWhiteSpace(command.AccessPointCode))
            return Error.Validation("Acceso.DatosIncompletos", "El código de la credencial y el del punto de acceso son obligatorios.");
        if (!Enum.IsDefined(command.MovementType))
            return Error.Validation("Acceso.MovimientoInvalido", "El tipo de movimiento debe ser Entry o Exit.");

        // Un código con formato inválido simplemente no corresponde a ninguna credencial:
        // el intento se registra como rechazado en lugar de descartarse.
        var credential = CredentialCode.TryCreate(command.CredentialCode, out var code)
            ? await credentials.GetByCodeAsync(code, cancellationToken)
            : null;
        var employee = credential is null ? null : await employees.GetByIdAsync(credential.EmployeeId, cancellationToken);
        var accessPoint = await accessPoints.GetByCodeAsync(command.AccessPointCode.Trim().ToUpperInvariant(), cancellationToken);

        MovementType? lastMovement = employee is not null && accessPoint is not null
            ? await accessRecords.GetLastAuthorizedMovementAsync(employee.Id, accessPoint.Id, cancellationToken)
            : null;

        var record = AccessControlService.Evaluate(
            command.CredentialCode, command.AccessPointCode,
            credential, employee, accessPoint,
            command.MovementType, lastMovement, clock.GetUtcNow().UtcDateTime);

        accessRecords.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await AccessRecordReadModel
            .Project(db.AccessRecords.Where(r => r.Id == record.Id), db)
            .FirstAsync(cancellationToken);
    }
}
