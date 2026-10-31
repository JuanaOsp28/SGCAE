using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.AccessPoints;

/// <summary>
/// Agregado Punto de acceso: lugar físico donde se registran entradas y salidas
/// (recepción, parqueadero, bodega, áreas restringidas). Solo los habilitados admiten movimientos.
/// </summary>
public sealed partial class AccessPoint : AggregateRoot
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 100;
    public const int LocationMaxLength = 150;

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccessPointType Type { get; private set; }
    public string? Location { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AccessPoint() { }

    public static AccessPoint Register(string? code, string? name, AccessPointType type, string? location, DateTime nowUtc)
    {
        if (!Enum.IsDefined(type))
            throw new DomainException("PuntoAcceso.TipoInvalido", "El tipo de punto de acceso no es válido.");

        return new AccessPoint
        {
            Id = Guid.NewGuid(),
            Code = NormalizeCode(code),
            Name = Guard.Required(name, "Nombre", NameMaxLength),
            Type = type,
            Location = string.IsNullOrWhiteSpace(location) ? null : Guard.Required(location, "Ubicacion", LocationMaxLength),
            IsEnabled = true,
            CreatedAtUtc = nowUtc
        };
    }

    public static string NormalizeCode(string? code)
    {
        var normalized = Guard.Required(code, "CodigoPunto", CodeMaxLength).ToUpperInvariant();
        if (!CodePattern().IsMatch(normalized))
            throw new DomainException("CodigoPunto.Invalid",
                "El código del punto de acceso solo admite letras, números y guiones (entre 2 y 20 caracteres).");
        return normalized;
    }

    public void Enable()
    {
        if (IsEnabled)
            throw new BusinessRuleException("PuntoAcceso.YaHabilitado", "El punto de acceso ya está habilitado.");
        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled)
            throw new BusinessRuleException("PuntoAcceso.YaDeshabilitado", "El punto de acceso ya está deshabilitado.");
        IsEnabled = false;
    }

    [GeneratedRegex("^[A-Z0-9-]{2,20}$")]
    private static partial Regex CodePattern();
}
