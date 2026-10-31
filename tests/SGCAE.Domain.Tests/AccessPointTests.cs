using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.Tests;

public class AccessPointTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Register_normalizes_code_and_starts_enabled()
    {
        var point = AccessPoint.Register(" bodega-01 ", "Bodega", AccessPointType.Warehouse, "  ", Now);

        Assert.Equal("BODEGA-01", point.Code);
        Assert.True(point.IsEnabled);
        Assert.Null(point.Location);
    }

    [Fact]
    public void Register_rejects_undefined_type() =>
        Assert.Throws<DomainException>(() => AccessPoint.Register("X1", "X", (AccessPointType)42, null, Now));

    [Fact]
    public void Enable_and_disable_validate_current_state()
    {
        var point = AccessPoint.Register("P1", "Puerta", AccessPointType.Other, null, Now);

        Assert.Throws<BusinessRuleException>(point.Enable);
        point.Disable();
        Assert.False(point.IsEnabled);
        Assert.Throws<BusinessRuleException>(point.Disable);
        point.Enable();
        Assert.True(point.IsEnabled);
    }
}
