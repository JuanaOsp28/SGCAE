namespace SGCAE.Domain.AccessRecords;

public enum AccessResult
{
    Authorized = 1,
    Rejected = 2
}

public enum AccessRejectionReason
{
    CredentialNotFound = 1,
    CredentialBlocked = 2,
    CredentialRevoked = 3,
    EmployeeInactive = 4,
    AccessPointNotFound = 5,
    AccessPointDisabled = 6,
    /// <summary>Se intenta registrar una entrada cuando el empleado ya está dentro del punto.</summary>
    AlreadyInside = 7,
    /// <summary>Se intenta registrar una salida sin una entrada previa en el punto.</summary>
    NotInside = 8
}
