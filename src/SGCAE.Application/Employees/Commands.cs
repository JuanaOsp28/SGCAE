using SGCAE.Application.Abstractions;
using SGCAE.Application.Common;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Employees;

// ---------- Registrar empleado ----------

public sealed record RegisterEmployeeCommand(
    string Identification, string FirstNames, string LastNames,
    string Email, string Position, string Area) : ICommand<EmployeeDto>;

internal sealed class RegisterEmployeeHandler(
    IEmployeeRepository employees, IUnitOfWork unitOfWork, TimeProvider clock)
    : ICommandHandler<RegisterEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(RegisterEmployeeCommand command, CancellationToken cancellationToken)
    {
        var identification = Identification.Create(command.Identification);
        var name = FullName.Create(command.FirstNames, command.LastNames);
        var email = Email.Create(command.Email);

        if (await employees.ExistsWithIdentificationAsync(identification, cancellationToken))
            return EmployeeErrors.DuplicateIdentification;
        if (await employees.ExistsWithEmailAsync(email, null, cancellationToken))
            return EmployeeErrors.DuplicateEmail;

        var employee = Employee.Register(identification, name, email, command.Position, command.Area, clock.GetUtcNow().UtcDateTime);
        employees.Add(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeDto.From(employee);
    }
}

// ---------- Actualizar empleado ----------

/// <summary>La identificación no se modifica: es la identidad de negocio del empleado.</summary>
public sealed record UpdateEmployeeCommand(
    Guid EmployeeId, string FirstNames, string LastNames,
    string Email, string Position, string Area) : ICommand<EmployeeDto>;

internal sealed class UpdateEmployeeHandler(IEmployeeRepository employees, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return EmployeeErrors.NotFound(command.EmployeeId);

        var name = FullName.Create(command.FirstNames, command.LastNames);
        var email = Email.Create(command.Email);
        if (await employees.ExistsWithEmailAsync(email, employee.Id, cancellationToken))
            return EmployeeErrors.DuplicateEmail;

        employee.UpdateInformation(name, email, command.Position, command.Area);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeDto.From(employee);
    }
}

// ---------- Desactivar empleado ----------

/// <summary>
/// Desactiva el vínculo laboral y revoca las credenciales vigentes o bloqueadas del empleado
/// en la misma transacción, de modo que no pueda volver a ingresar.
/// </summary>
public sealed record DeactivateEmployeeCommand(Guid EmployeeId) : ICommand<EmployeeDto>;

internal sealed class DeactivateEmployeeHandler(
    IEmployeeRepository employees, ICredentialRepository credentials, IUnitOfWork unitOfWork, TimeProvider clock)
    : ICommandHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(DeactivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return EmployeeErrors.NotFound(command.EmployeeId);

        var now = clock.GetUtcNow().UtcDateTime;
        employee.Deactivate(now);

        foreach (var credential in await credentials.GetCurrentByEmployeeAsync(employee.Id, cancellationToken))
            credential.Revoke("Desactivación del empleado", now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return EmployeeDto.From(employee);
    }
}

// ---------- Reactivar empleado ----------

public sealed record ReactivateEmployeeCommand(Guid EmployeeId) : ICommand<EmployeeDto>;

internal sealed class ReactivateEmployeeHandler(IEmployeeRepository employees, IUnitOfWork unitOfWork)
    : ICommandHandler<ReactivateEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(ReactivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
            return EmployeeErrors.NotFound(command.EmployeeId);

        employee.Reactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return EmployeeDto.From(employee);
    }
}
