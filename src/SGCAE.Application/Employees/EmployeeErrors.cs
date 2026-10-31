using SGCAE.Application.Common;

namespace SGCAE.Application.Employees;

public static class EmployeeErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Empleado.NoEncontrado", $"No existe un empleado con id '{id}'.");

    public static readonly Error DuplicateIdentification =
        Error.Conflict("Empleado.IdentificacionDuplicada", "Ya existe un empleado registrado con esa identificación.");

    public static readonly Error DuplicateEmail =
        Error.Conflict("Empleado.CorreoDuplicado", "Ya existe un empleado registrado con ese correo electrónico.");
}
