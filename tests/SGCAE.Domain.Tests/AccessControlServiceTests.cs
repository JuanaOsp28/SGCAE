using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;
using SGCAE.Domain.Services;

namespace SGCAE.Domain.Tests;

public class AccessControlServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly Employee _employee = EmployeeTests.NewEmployee();
    private readonly AccessPoint _point = AccessPoint.Register("RECEPCION", "Recepción", AccessPointType.Reception, null, Now);
    private readonly Credential _credential;

    public AccessControlServiceTests() =>
        _credential = Credential.Issue(_employee, CredentialCode.Create("CRED-0001"), false, Now);

    private AccessRecord Evaluate(
        MovementType movement = MovementType.Entry, MovementType? last = null,
        Credential? credential = null, Employee? employee = null, AccessPoint? point = null, bool nulls = false) =>
        AccessControlService.Evaluate(
            "CRED-0001", "RECEPCION",
            nulls ? credential : credential ?? _credential,
            nulls ? employee : employee ?? _employee,
            nulls ? point : point ?? _point,
            movement, last, Now);

    [Fact]
    public void Authorizes_valid_entry()
    {
        var record = Evaluate();

        Assert.Equal(AccessResult.Authorized, record.Result);
        Assert.Null(record.RejectionReason);
        Assert.Equal(_employee.Id, record.EmployeeId);
        Assert.Equal(_point.Id, record.AccessPointId);
    }

    [Fact]
    public void Authorizes_exit_after_entry() =>
        Assert.True(Evaluate(MovementType.Exit, MovementType.Entry).IsAuthorized);

    [Fact]
    public void Rejects_unknown_credential()
    {
        var record = Evaluate(nulls: true, employee: _employee, point: _point);

        Assert.Equal(AccessRejectionReason.CredentialNotFound, record.RejectionReason);
        Assert.Null(record.EmployeeId);
        Assert.Equal("CRED-0001", record.RequestedCredentialCode);
    }

    [Fact]
    public void Rejects_blocked_credential()
    {
        _credential.Block("Pérdida", Now);
        Assert.Equal(AccessRejectionReason.CredentialBlocked, Evaluate().RejectionReason);
    }

    [Fact]
    public void Rejects_revoked_credential()
    {
        _credential.Revoke("Retiro", Now);
        Assert.Equal(AccessRejectionReason.CredentialRevoked, Evaluate().RejectionReason);
    }

    [Fact]
    public void Rejects_inactive_employee_even_with_active_credential()
    {
        _employee.Deactivate(Now);

        var record = Evaluate();

        Assert.Equal(AccessRejectionReason.EmployeeInactive, record.RejectionReason);
        Assert.Equal(_employee.Id, record.EmployeeId);
    }

    [Fact]
    public void Rejects_unknown_access_point() =>
        Assert.Equal(AccessRejectionReason.AccessPointNotFound,
            Evaluate(nulls: true, credential: _credential, employee: _employee).RejectionReason);

    [Fact]
    public void Rejects_disabled_access_point()
    {
        _point.Disable();
        Assert.Equal(AccessRejectionReason.AccessPointDisabled, Evaluate().RejectionReason);
    }

    [Fact]
    public void Rejects_second_entry_without_exit() =>
        Assert.Equal(AccessRejectionReason.AlreadyInside, Evaluate(MovementType.Entry, MovementType.Entry).RejectionReason);

    [Theory]
    [InlineData(null)]
    [InlineData(MovementType.Exit)]
    public void Rejects_exit_without_previous_entry(MovementType? last) =>
        Assert.Equal(AccessRejectionReason.NotInside, Evaluate(MovementType.Exit, last).RejectionReason);

    [Fact]
    public void Rejects_undefined_movement_type() =>
        Assert.Throws<DomainException>(() => Evaluate((MovementType)7));
}
