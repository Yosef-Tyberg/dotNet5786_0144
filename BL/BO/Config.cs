using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO;

public class Config
{
    /// <inheritdoc />
    public int AdminId { get; set; }

    /// <inheritdoc />
    public string AdminPassword { get; set; }

    /// <inheritdoc />
    public double AvgCarSpeedKmh { get; set; }

    /// <inheritdoc />
    public double AvgMotorcycleSpeedKmh { get; set; }

    /// <inheritdoc />
    public double AvgBicycleSpeedKmh { get; set; }

    /// <inheritdoc />
    public double AvgWalkingSpeedKmh { get; set; }

    /// <inheritdoc />
    public double? MaxGeneralDeliveryDistanceKm { get; set; }

    /// <inheritdoc />
    public TimeSpan MaxDeliveryTimeSpan { get; set; }

    /// <inheritdoc />
    public TimeSpan RiskRange { get; set; }

    /// <inheritdoc />
    public TimeSpan InactivityRange { get; set; }

    /// <inheritdoc />
    public string? CompanyFullAddress { get; set; }

    public override string ToString() => Helpers.Tools.ToStringProperty(this);

}
