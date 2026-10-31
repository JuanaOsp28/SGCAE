using SGCAE.Domain.Common;

namespace SGCAE.Domain.Employees;

public sealed class FullName : ValueObject
{
    public const int MaxLength = 100;

    public string FirstNames { get; private set; } = null!;
    public string LastNames { get; private set; } = null!;

    private FullName() { }

    public static FullName Create(string? firstNames, string? lastNames) => new()
    {
        FirstNames = Guard.Required(firstNames, "Nombres", MaxLength),
        LastNames = Guard.Required(lastNames, "Apellidos", MaxLength)
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstNames;
        yield return LastNames;
    }

    public override string ToString() => $"{FirstNames} {LastNames}";
}
