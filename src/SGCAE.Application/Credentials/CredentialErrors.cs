using SGCAE.Application.Common;

namespace SGCAE.Application.Credentials;

public static class CredentialErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Credencial.NoEncontrada", $"No existe una credencial con id '{id}'.");

    public static Error NotFound(string code) =>
        Error.NotFound("Credencial.NoEncontrada", $"No existe una credencial con código '{code}'.");

    public static readonly Error DuplicateCode =
        Error.Conflict("Credencial.CodigoDuplicado", "Ya existe una credencial con ese código.");
}
