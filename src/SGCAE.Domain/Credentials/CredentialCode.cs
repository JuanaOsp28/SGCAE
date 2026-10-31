using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.Credentials;

/// <summary>
/// Código único impreso o grabado en la credencial (tarjeta, carné, tag).
/// </summary>
public sealed partial class CredentialCode : ValueObject
{
    public const int MaxLength = 30;

    public string Value { get; }

    private CredentialCode(string value) => Value = value;

    public static CredentialCode Create(string? value)
    {
        var normalized = Guard.Required(value, "CodigoCredencial", MaxLength).ToUpperInvariant();
        if (!Pattern().IsMatch(normalized))
            throw new DomainException("CodigoCredencial.Invalid",
                "El código de la credencial solo admite letras, números y guiones (entre 4 y 30 caracteres).");
        return new CredentialCode(normalized);
    }

    public static bool TryCreate(string? value, out CredentialCode code)
    {
        try
        {
            code = Create(value);
            return true;
        }
        catch (DomainException)
        {
            code = null!;
            return false;
        }
    }

    /// <summary>Genera un código aleatorio con el formato CRED-XXXXXXXX.</summary>
    public static CredentialCode Generate() =>
        new($"CRED-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}");

    [GeneratedRegex("^[A-Z0-9-]{4,30}$")]
    private static partial Regex Pattern();

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;
}
