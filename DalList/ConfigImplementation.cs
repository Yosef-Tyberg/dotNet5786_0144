using DalApi;

namespace Dal;

/// <summary>
/// DAL implementation of <see cref="DalApi.IConfig"/> that forwards to the internal Config store.
/// </summary>
public class ConfigImplementation : IConfig
{
    /// <inheritdoc />
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <inheritdoc />
    public int AdminId { get => Config.AdminId; set => Config.AdminId = value; }

    /// <inheritdoc />
    public string AdminPassword { get => Config.AdminPassword; set => Config.AdminPassword = value; }

    /// <inheritdoc />
    public double AvgCarSpeedKmh { get => Config.AvgCarSpeedKmh; set => Config.AvgCarSpeedKmh = value; }

    /// <inheritdoc />
    public double AvgMotorcycleSpeedKmh { get => Config.AvgMotorcycleSpeedKmh; set => Config.AvgMotorcycleSpeedKmh = value; }

    /// <inheritdoc />
    public double AvgBicycleSpeedKmh { get => Config.AvgBicycleSpeedKmh; set => Config.AvgBicycleSpeedKmh = value; }

    /// <inheritdoc />
    public double AvgWalkingSpeedKmh { get => Config.AvgWalkingSpeedKmh; set => Config.AvgWalkingSpeedKmh = value; }

    /// <inheritdoc />
    public double? MaxGeneralDeliveryDistanceKm { get => Config.MaxGeneralDeliveryDistanceKm; set => Config.MaxGeneralDeliveryDistanceKm = value; }

    /// <inheritdoc />
    public TimeSpan MaxDeliveryTimeSpan { get => Config.MaxDeliveryTimeSpan; set => Config.MaxDeliveryTimeSpan = value; }

    /// <inheritdoc />
    public TimeSpan RiskRange { get => Config.RiskRange; set => Config.RiskRange = value; }

    /// <inheritdoc />
    public TimeSpan InactivityRange { get => Config.InactivityRange; set => Config.InactivityRange = value; }

    /// <inheritdoc />
    public string? CompanyFullAddress { get => Config.CompanyFullAddress; set => Config.CompanyFullAddress = value; }

    /// <inheritdoc />
    public double? Latitude { get => Config.Latitude; set => Config.Latitude = value; }

    /// <inheritdoc />
    public double? Longitude { get => Config.Longitude; set => Config.Longitude = value;  }


    /// <summary>
    /// Resets the underlying <see cref="Config"/> to its defaults.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}
