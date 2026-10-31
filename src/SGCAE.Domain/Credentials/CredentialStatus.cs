namespace SGCAE.Domain.Credentials;

public enum CredentialStatus
{
    /// <summary>Vigente: puede utilizarse para acceder.</summary>
    Active = 1,
    /// <summary>Bloqueada temporalmente (pérdida, sospecha de uso indebido). Puede desbloquearse.</summary>
    Blocked = 2,
    /// <summary>Revocada definitivamente. Estado final.</summary>
    Revoked = 3
}
