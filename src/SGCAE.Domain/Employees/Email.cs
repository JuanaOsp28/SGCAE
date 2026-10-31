using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.Employees;

public sealed partial class Email : ValueObject
{
    public const int MaxLength = 150;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? value)
    {
        var normalized = Guard.Required(value, "Correo", MaxLength).ToLowerInvariant();
        if (!Pattern().IsMatch(normalized))
            throw new DomainException("Correo.Invalid", "El correo electrónico no tiene un formato válido.");
        return new Email(normalized);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Pattern();

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;
}
