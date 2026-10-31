using SGCAE.Domain.Common;
using SGCAE.Domain.Credentials;

namespace SGCAE.Domain.Tests;

public class CredentialTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    public static Credential NewCredential(string code = "CRED-0001") =>
        Credential.Issue(EmployeeTests.NewEmployee(), CredentialCode.Create(code), false, Now);

    [Fact]
    public void Issue_creates_active_credential()
    {
        var credential = NewCredential();

        Assert.Equal(CredentialStatus.Active, credential.Status);
        Assert.True(credential.CanBeUsed);
    }

    [Fact]
    public void Cannot_issue_to_inactive_employee()
    {
        var employee = EmployeeTests.NewEmployee();
        employee.Deactivate(Now);

        Assert.Throws<BusinessRuleException>(() =>
            Credential.Issue(employee, CredentialCode.Create("CRED-1"), false, Now));
    }

    [Fact]
    public void Cannot_issue_when_employee_already_has_current_credential()
    {
        Assert.Throws<BusinessRuleException>(() =>
            Credential.Issue(EmployeeTests.NewEmployee(), CredentialCode.Create("CRED-1"), true, Now));
    }

    [Fact]
    public void Block_and_unblock_cycle()
    {
        var credential = NewCredential();

        credential.Block("Pérdida", Now);
        Assert.Equal(CredentialStatus.Blocked, credential.Status);
        Assert.False(credential.CanBeUsed);
        Assert.Equal("Pérdida", credential.StatusReason);

        credential.Unblock(Now);
        Assert.Equal(CredentialStatus.Active, credential.Status);
        Assert.Null(credential.StatusReason);
    }

    [Fact]
    public void Block_requires_reason() =>
        Assert.Throws<DomainException>(() => NewCredential().Block("", Now));

    [Fact]
    public void Revoked_is_final_state()
    {
        var credential = NewCredential();
        credential.Revoke("Retiro", Now);

        Assert.Throws<BusinessRuleException>(() => credential.Block("x", Now));
        Assert.Throws<BusinessRuleException>(() => credential.Unblock(Now));
        Assert.Throws<BusinessRuleException>(() => credential.Revoke("x", Now));
    }

    [Fact]
    public void Blocked_credential_can_be_revoked()
    {
        var credential = NewCredential();
        credential.Block("Pérdida", Now);

        credential.Revoke("No apareció", Now);

        Assert.Equal(CredentialStatus.Revoked, credential.Status);
    }

    [Fact]
    public void Only_blocked_credential_can_be_unblocked() =>
        Assert.Throws<BusinessRuleException>(() => NewCredential().Unblock(Now));

    [Theory]
    [InlineData("AB")]
    [InlineData("CRED 01")]
    [InlineData("CRED_01")]
    public void Code_rejects_invalid_format(string code) =>
        Assert.False(CredentialCode.TryCreate(code, out _));

    [Fact]
    public void Generated_code_is_valid()
    {
        var generated = CredentialCode.Generate();

        Assert.True(CredentialCode.TryCreate(generated.Value, out var parsed));
        Assert.Equal(generated, parsed);
    }
}
