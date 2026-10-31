using SGCAE.Application.AccessPoints;
using SGCAE.Application.AccessRecords;
using SGCAE.Application.Common;
using SGCAE.Application.Credentials;
using SGCAE.Application.Employees;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Application.Tests;

public sealed class UseCaseTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private Task<EmployeeDto> RegisterEmployee(string identification = "1035123456", string email = "laura@empresa.com") =>
        _host.SendOk(new RegisterEmployeeCommand(identification, "Laura", "Gómez", email, "Analista", "Bodega"));

    private Task<AccessPointDto> RegisterPoint(string code = "RECEPCION") =>
        _host.SendOk(new RegisterAccessPointCommand(code, "Recepción", AccessPointType.Reception, null));

    private async Task<AccessRecordDto> Access(string credential, string point, MovementType movement)
    {
        _host.Clock.Advance(TimeSpan.FromMinutes(1));
        return await _host.SendOk(new RegisterAccessCommand(credential, point, movement));
    }

    // ---------- RF-01 ----------

    [Fact]
    public async Task Register_and_get_employee()
    {
        var created = await RegisterEmployee();

        var fetched = await _host.QueryOk(new GetEmployeeByIdQuery(created.Id));

        Assert.Equal("1035123456", fetched.Identification);
        Assert.Equal("laura@empresa.com", fetched.Email);
        Assert.Equal(EmploymentStatus.Active, fetched.Status);
    }

    [Fact]
    public async Task Duplicate_identification_and_email_are_conflicts()
    {
        await RegisterEmployee();

        var sameId = await _host.Send(new RegisterEmployeeCommand("1035123456", "A", "B", "otro@empresa.com", "C", "D"));
        var sameEmail = await _host.Send(new RegisterEmployeeCommand("99999999", "A", "B", "LAURA@empresa.com", "C", "D"));

        Assert.Equal(EmployeeErrors.DuplicateIdentification, sameId.Error);
        Assert.Equal(EmployeeErrors.DuplicateEmail, sameEmail.Error);
    }

    [Fact]
    public async Task Invalid_data_is_validation_error()
    {
        var result = await _host.Send(new RegisterEmployeeCommand("1035123456", "Laura", "Gómez", "no-es-correo", "A", "B"));

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Update_employee_information()
    {
        var employee = await RegisterEmployee();

        var updated = await _host.SendOk(new UpdateEmployeeCommand(
            employee.Id, "Laura María", "Gómez", "laura.maria@empresa.com", "Coordinadora", "Logística"));

        Assert.Equal("Laura María", updated.FirstNames);
        Assert.Equal("Logística", (await _host.QueryOk(new GetEmployeeByIdQuery(employee.Id))).Area);
    }

    [Fact]
    public async Task Unknown_employee_is_not_found()
    {
        var result = await _host.Query(new GetEmployeeByIdQuery(Guid.NewGuid()));
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task List_employees_filters_and_paginates()
    {
        for (var i = 0; i < 5; i++)
            await RegisterEmployee($"1000000{i}", $"e{i}@empresa.com");
        var other = await _host.SendOk(new RegisterEmployeeCommand("20000000", "Pedro", "Zapata", "pedro@empresa.com", "Conductor", "Transporte"));
        await _host.SendOk(new DeactivateEmployeeCommand(other.Id));

        var page = await _host.QueryOk(new ListEmployeesQuery(null, "Bodega", EmploymentStatus.Active, 2, 2));
        var inactive = await _host.QueryOk(new ListEmployeesQuery("Zapata", null, EmploymentStatus.Inactive, null, null));

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(other.Id, Assert.Single(inactive.Items).Id);
    }

    [Fact]
    public async Task Deactivating_employee_revokes_current_credentials()
    {
        var employee = await RegisterEmployee();
        var credential = await _host.SendOk(new IssueCredentialCommand(employee.Id, "CRED-0001"));

        await _host.SendOk(new DeactivateEmployeeCommand(employee.Id));

        var after = await _host.QueryOk(new GetCredentialByIdQuery(credential.Id));
        Assert.Equal(CredentialStatus.Revoked, after.Status);
        Assert.False(after.CanBeUsed);
    }

    // ---------- RF-02 ----------

    [Fact]
    public async Task Issue_credential_and_query_by_code()
    {
        var employee = await RegisterEmployee();

        var issued = await _host.SendOk(new IssueCredentialCommand(employee.Id, "cred-0001"));
        var byCode = await _host.QueryOk(new GetCredentialByCodeQuery("CRED-0001"));

        Assert.Equal("CRED-0001", issued.Code);
        Assert.Equal(issued.Id, byCode.Id);
        Assert.Equal("Laura Gómez", byCode.EmployeeName);
        Assert.True(byCode.CanBeUsed);
    }

    [Fact]
    public async Task Credential_code_is_generated_when_missing()
    {
        var employee = await RegisterEmployee();

        var issued = await _host.SendOk(new IssueCredentialCommand(employee.Id, null));

        Assert.StartsWith("CRED-", issued.Code);
    }

    [Fact]
    public async Task Credential_code_must_be_unique()
    {
        var a = await RegisterEmployee();
        var b = await RegisterEmployee("20000000", "b@empresa.com");
        await _host.SendOk(new IssueCredentialCommand(a.Id, "CRED-0001"));

        var result = await _host.Send(new IssueCredentialCommand(b.Id, "CRED-0001"));

        Assert.Equal(CredentialErrors.DuplicateCode, result.Error);
    }

    [Fact]
    public async Task New_credential_requires_revoking_the_current_one()
    {
        var employee = await RegisterEmployee();
        var first = await _host.SendOk(new IssueCredentialCommand(employee.Id, "CRED-0001"));

        var rejected = await _host.Send(new IssueCredentialCommand(employee.Id, "CRED-0002"));
        await _host.SendOk(new RevokeCredentialCommand(first.Id, "Deterioro"));
        var second = await _host.Send(new IssueCredentialCommand(employee.Id, "CRED-0002"));

        Assert.Equal(ErrorType.Conflict, rejected.Error!.Type);
        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task Invalid_status_transition_is_conflict()
    {
        var employee = await RegisterEmployee();
        var credential = await _host.SendOk(new IssueCredentialCommand(employee.Id, "CRED-0001"));

        var result = await _host.Send(new UnblockCredentialCommand(credential.Id));

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // ---------- RF-03 ----------

    [Fact]
    public async Task Entry_exit_sequence_is_validated_per_access_point()
    {
        var employee = await RegisterEmployee();
        await _host.SendOk(new IssueCredentialCommand(employee.Id, "CRED-0001"));
        await RegisterPoint("RECEPCION");
        await RegisterPoint("BODEGA");

        Assert.Equal(AccessResult.Authorized, (await Access("CRED-0001", "RECEPCION", MovementType.Entry)).Result);
        Assert.Equal(AccessRejectionReason.AlreadyInside, (await Access("CRED-0001", "RECEPCION", MovementType.Entry)).RejectionReason);
        Assert.Equal(AccessResult.Authorized, (await Access("CRED-0001", "BODEGA", MovementType.Entry)).Result);
        Assert.Equal(AccessResult.Authorized, (await Access("CRED-0001", "RECEPCION", MovementType.Exit)).Result);
        Assert.Equal(AccessRejectionReason.NotInside, (await Access("CRED-0001", "RECEPCION", MovementType.Exit)).RejectionReason);
    }

    [Fact]
    public async Task Rejected_attempts_are_recorded_with_requested_data()
    {
        await RegisterPoint();

        var record = await Access("no existe!", "RECEPCION", MovementType.Entry);
        var stored = await _host.QueryOk(new GetAccessRecordByIdQuery(record.Id));

        Assert.Equal(AccessRejectionReason.CredentialNotFound, stored.RejectionReason);
        Assert.Equal("NO EXISTE!", stored.RequestedCredentialCode);
        Assert.NotNull(stored.RejectionMessage);
    }

    [Fact]
    public async Task Blocked_credential_and_disabled_point_are_rejected()
    {
        var employee = await RegisterEmployee();
        var credential = await _host.SendOk(new IssueCredentialCommand(employee.Id, "CRED-0001"));
        var point = await RegisterPoint();

        await _host.SendOk(new BlockCredentialCommand(credential.Id, "Pérdida"));
        var blocked = await Access("CRED-0001", "RECEPCION", MovementType.Entry);
        await _host.SendOk(new UnblockCredentialCommand(credential.Id));
        await _host.SendOk(new DisableAccessPointCommand(point.Id));
        var disabled = await Access("CRED-0001", "RECEPCION", MovementType.Entry);

        Assert.Equal(AccessRejectionReason.CredentialBlocked, blocked.RejectionReason);
        Assert.Equal(AccessRejectionReason.AccessPointDisabled, disabled.RejectionReason);
    }

    [Fact]
    public async Task Undefined_movement_type_is_validation_error()
    {
        var result = await _host.Send(new RegisterAccessCommand("CRED-0001", "RECEPCION", (MovementType)9));
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    // ---------- RF-04 ----------

    [Fact]
    public async Task History_filters_by_employee_point_result_and_dates()
    {
        var laura = await RegisterEmployee();
        var pedro = await RegisterEmployee("20000000", "pedro@empresa.com");
        await _host.SendOk(new IssueCredentialCommand(laura.Id, "CRED-L"));
        await _host.SendOk(new IssueCredentialCommand(pedro.Id, "CRED-P"));
        var recepcion = await RegisterPoint("RECEPCION");
        await RegisterPoint("BODEGA");

        await Access("CRED-L", "RECEPCION", MovementType.Entry);   // 08:01 autorizado
        await Access("CRED-L", "RECEPCION", MovementType.Entry);   // 08:02 rechazado
        var fromUtc = _host.Clock.Now.UtcDateTime.AddSeconds(1);
        await Access("CRED-L", "BODEGA", MovementType.Entry);      // 08:03 autorizado
        await Access("CRED-P", "RECEPCION", MovementType.Entry);   // 08:04 autorizado
        var toUtc = _host.Clock.Now.UtcDateTime;
        await Access("CRED-P", "RECEPCION", MovementType.Exit);    // 08:05 autorizado

        var byEmployee = await _host.QueryOk(new ListAccessRecordsQuery(laura.Id, null, null, null, null, null, null, null));
        var rejected = await _host.QueryOk(new ListAccessRecordsQuery(null, null, null, null, AccessResult.Rejected, null, null, null));
        var byPoint = await _host.QueryOk(new ListAccessRecordsQuery(null, null, null, recepcion.Id, AccessResult.Authorized, null, null, null));
        var byDates = await _host.QueryOk(new ListAccessRecordsQuery(null, fromUtc, toUtc, null, null, null, null, null));
        var paged = await _host.QueryOk(new ListAccessRecordsQuery(null, null, null, null, null, null, 2, 2));

        Assert.Equal(3, byEmployee.TotalCount);
        Assert.Equal(AccessRejectionReason.AlreadyInside, Assert.Single(rejected.Items).RejectionReason);
        Assert.Equal(3, byPoint.TotalCount);
        Assert.Equal(2, byDates.TotalCount);
        Assert.Equal(5, paged.TotalCount);
        Assert.Equal(2, paged.Items.Count);
        Assert.True(paged.HasNextPage && paged.HasPreviousPage);
        Assert.True(byEmployee.Items[0].OccurredAtUtc > byEmployee.Items[^1].OccurredAtUtc, "Orden descendente por fecha");
    }

    [Fact]
    public async Task History_rejects_inverted_date_range()
    {
        var result = await _host.Query(new ListAccessRecordsQuery(
            null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), null, null, null, null, null));

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Theory]
    [InlineData(null, null, 1, Paging.DefaultPageSize)]
    [InlineData(0, 500, 1, Paging.MaxPageSize)]
    [InlineData(3, 0, 3, 1)]
    public void Paging_is_normalized(int? page, int? size, int expectedPage, int expectedSize) =>
        Assert.Equal((expectedPage, expectedSize), Paging.Normalize(page, size));

    // ---------- RF-06 ----------

    [Fact]
    public async Task Access_points_can_be_listed_by_enabled_state()
    {
        await RegisterPoint("RECEPCION");
        var bodega = await RegisterPoint("BODEGA");
        await _host.SendOk(new DisableAccessPointCommand(bodega.Id));

        var enabled = await _host.QueryOk(new ListAccessPointsQuery(true, null, null, null));
        var duplicate = await _host.Send(new RegisterAccessPointCommand("recepcion", "Otra", AccessPointType.Other, null));

        Assert.Equal("RECEPCION", Assert.Single(enabled.Items).Code);
        Assert.Equal(AccessPointErrors.DuplicateCode, duplicate.Error);
    }
}
