using SGCAE.Domain.Common;
using SGCAE.Domain.Employees;

namespace SGCAE.Domain.Tests;

public class EmployeeTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    public static Employee NewEmployee(string identification = "1035123456") =>
        Employee.Register(
            Identification.Create(identification),
            FullName.Create("Laura", "Gómez"),
            Email.Create($"{identification}@empresa.com"),
            "Analista", "Bodega", Now);

    [Fact]
    public void Register_creates_active_employee_and_raises_event()
    {
        var employee = NewEmployee();

        Assert.True(employee.IsActive);
        Assert.Equal(EmploymentStatus.Active, employee.Status);
        Assert.Single(employee.DomainEvents.OfType<EmployeeRegistered>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("12-34")]
    [InlineData("123")]
    public void Identification_rejects_invalid_values(string value) =>
        Assert.Throws<DomainException>(() => Identification.Create(value));

    [Fact]
    public void Identification_is_normalized_and_compared_by_value()
    {
        Assert.Equal(Identification.Create(" ab12345 "), Identification.Create("AB12345"));
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("a@b")]
    [InlineData("  ")]
    public void Email_rejects_invalid_values(string value) =>
        Assert.Throws<DomainException>(() => Email.Create(value));

    [Fact]
    public void Email_is_normalized_to_lowercase()
    {
        Assert.Equal("laura@empresa.com", Email.Create("Laura@Empresa.COM").Value);
    }

    [Fact]
    public void Register_requires_position_and_area()
    {
        Assert.Throws<DomainException>(() => Employee.Register(
            Identification.Create("12345678"), FullName.Create("A", "B"), Email.Create("a@b.co"), " ", "Bodega", Now));
    }

    [Fact]
    public void Deactivate_marks_employee_inactive()
    {
        var employee = NewEmployee();

        employee.Deactivate(Now);

        Assert.False(employee.IsActive);
        Assert.Equal(Now, employee.DeactivatedAtUtc);
    }

    [Fact]
    public void Inactive_employee_cannot_be_updated_or_deactivated_again()
    {
        var employee = NewEmployee();
        employee.Deactivate(Now);

        Assert.Throws<BusinessRuleException>(() => employee.Deactivate(Now));
        Assert.Throws<BusinessRuleException>(() =>
            employee.UpdateInformation(FullName.Create("X", "Y"), Email.Create("x@y.co"), "Cargo", "Area"));
    }

    [Fact]
    public void Reactivate_restores_active_status()
    {
        var employee = NewEmployee();
        employee.Deactivate(Now);

        employee.Reactivate();

        Assert.True(employee.IsActive);
        Assert.Null(employee.DeactivatedAtUtc);
        Assert.Throws<BusinessRuleException>(employee.Reactivate);
    }
}
