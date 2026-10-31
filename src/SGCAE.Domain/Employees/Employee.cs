using SGCAE.Domain.Common;

namespace SGCAE.Domain.Employees;

/// <summary>
/// Agregado Empleado. Invariantes:
/// - Identificación, nombre, correo, cargo y área siempre válidos.
/// - Un empleado inactivo no puede modificarse ni volver a desactivarse.
/// </summary>
public sealed class Employee : AggregateRoot
{
    public const int PositionMaxLength = 100;
    public const int AreaMaxLength = 100;

    public Identification Identification { get; private set; } = null!;
    public FullName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string Position { get; private set; } = null!;
    public string Area { get; private set; } = null!;
    public EmploymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    private Employee() { }

    public bool IsActive => Status == EmploymentStatus.Active;

    public static Employee Register(
        Identification identification, FullName name, Email email,
        string? position, string? area, DateTime nowUtc)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Identification = identification,
            Name = name,
            Email = email,
            Position = Guard.Required(position, "Cargo", PositionMaxLength),
            Area = Guard.Required(area, "Area", AreaMaxLength),
            Status = EmploymentStatus.Active,
            CreatedAtUtc = nowUtc
        };
        employee.Raise(new EmployeeRegistered(employee.Id, nowUtc));
        return employee;
    }

    public void UpdateInformation(FullName name, Email email, string? position, string? area)
    {
        EnsureActive();
        Name = name;
        Email = email;
        Position = Guard.Required(position, "Cargo", PositionMaxLength);
        Area = Guard.Required(area, "Area", AreaMaxLength);
    }

    public void Deactivate(DateTime nowUtc)
    {
        EnsureActive();
        Status = EmploymentStatus.Inactive;
        DeactivatedAtUtc = nowUtc;
        Raise(new EmployeeDeactivated(Id, nowUtc));
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleException("Empleado.YaActivo", "El empleado ya se encuentra activo.");
        Status = EmploymentStatus.Active;
        DeactivatedAtUtc = null;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new BusinessRuleException("Empleado.Inactivo", "La operación no se permite sobre un empleado inactivo.");
    }
}

public sealed record EmployeeRegistered(Guid EmployeeId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record EmployeeDeactivated(Guid EmployeeId, DateTime OccurredOnUtc) : IDomainEvent;
