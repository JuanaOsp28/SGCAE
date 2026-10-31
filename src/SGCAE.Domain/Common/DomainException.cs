namespace SGCAE.Domain.Common;

/// <summary>
/// Se lanza cuando una operación viola una regla o invariante del dominio.
/// </summary>
public class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>
/// Violación de una regla de negocio que depende del estado actual
/// (p. ej. bloquear una credencial revocada). No es un error de formato del dato.
/// </summary>
public sealed class BusinessRuleException(string code, string message) : DomainException(code, message);
