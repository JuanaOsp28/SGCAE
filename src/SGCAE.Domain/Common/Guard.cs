namespace SGCAE.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new DomainException($"{field}.Required", $"El campo '{field}' es obligatorio.");
        if (trimmed.Length > maxLength)
            throw new DomainException($"{field}.TooLong", $"El campo '{field}' no puede superar {maxLength} caracteres.");
        return trimmed;
    }
}
