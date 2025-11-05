namespace DalApi;

/// <summary>
/// Interface allowing higher layers to interact with the DAL's configuration/settings.
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Represents the system clock used by the DAL (can be adjusted for testing/simulation).
    /// </summary>
    DateTime Clock { get; set; }

    /// <summary>
    /// Resets configuration values to their defaults.
    /// </summary>
    void Reset();

    /// <summary>
    /// Stored Admin user ID for system access.
    /// </summary>
    int AdminId { get; set; }

    /// <summary>
    /// Stored Admin password for system access.
    /// </summary>
    string AdminPassword { get; set; }

    /// <summary>
    /// Company's full address used for delivery calculations.
    /// </summary>
    string? CompanyFullAddress { get; set; }

    /// <summary>
    /// Average car speed in km/h used to estimate delivery times.
    /// </summary>
    double AvgCarSpeedKmh { get; set; }

    /// <summary>
    /// Average motorcycle speed in km/h used to estimate delivery times.
    /// </summary>
    double AvgMotorcycleSpeedKmh { get; set; }

    /// <summary>
    /// Average bicycle speed in km/h used to estimate delivery times.
    /// </summary>
    double AvgBicycleSpeedKmh { get; set; }

    /// <summary>
    /// Average walking speed in km/h used to estimate delivery times.
    /// </summary>
    double AvgWalkingSpeedKmh { get; set; }

    /// <summary>
    /// Maximum allowed delivery distance (km) for a standard delivery.
    /// </summary>
    double? MaxGeneralDeliveryDistanceKm { get; set; }

    /// <summary>
    /// Maximum allowed delivery duration before it is considered late.
    /// </summary>
    TimeSpan MaxDeliveryTimeSpan { get; set; }

    /// <summary>
    /// Time range considered "risky" or nearing a late delivery.
    /// </summary>
    TimeSpan RiskRange { get; set; }

    /// <summary>
    /// Time period without deliveries after which a courier is considered inactive.
    /// </summary>
    TimeSpan InactivityRange { get; set; }
}
