using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.Employees;

/// <summary>
/// Número de identificación del empleado (cédula, pasaporte, etc.). Único en la organización.
/// </summary>
public sealed partial class Identification : ValueObject
{
    public const int MaxLength = 20;

    public string Value { get; }

    private Identification(string value) => Value = value;

    public static Identification Create(string? value)
    {
        var normalized = Guard.Required(value, "Identificacion", MaxLength).ToUpperInvariant();
        if (!Pattern().IsMatch(normalized))
            throw new DomainException("Identificacion.Invalid",
                "La identificación solo admite letras y números (entre 5 y 20 caracteres).");
        return new Identification(normalized);
    }

    [GeneratedRegex("^[A-Z0-9]{5,20}$")]
    private static partial Regex Pattern();

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;
}
