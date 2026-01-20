namespace Dal;

/// <summary>
/// Provides system-wide configuration and default values for IDs, admin credentials,
/// company location, operational speeds, and delivery constraints.
/// </summary>
internal static class Config
{
    /// <summary>
    /// Starting value for the order ID sequence generator.
    /// </summary>
    internal const int startOrderId = 1;

    private static int nextOrderId = startOrderId;

    /// <summary>
    /// Generates the next unique Order ID.
    /// </summary>
    internal static int NextOrderId { get => nextOrderId++; }

    /// <summary>
    /// Starting value for the delivery ID sequence generator.
    /// </summary>
    internal const int startDeliveryId = 1;

    private static int nextDeliveryId = startDeliveryId;

    /// <summary>
    /// Generates the next unique Delivery ID.
    /// </summary>
    internal static int NextDeliveryId { get => nextDeliveryId++; }

    /// <summary>
    /// Represents the system clock used for simulation or time tracking.
    /// Defaults to current system time.
    /// </summary>
    internal static DateTime Clock { get; set; } = DateTime.Now;

    /// <summary>
    /// Stored Admin user ID for system access.
    /// </summary>
    internal static int AdminId { get; set; }

    /// <summary>
    /// Stored Admin password for system access. Default: "Admin".
    /// </summary>
    internal static string AdminPassword { get; set; } = "Admin";

    /// <summary>
    /// Company's full address used for delivery calculations.
    /// </summary>
    internal static string? CompanyFullAddress { get; set; }

    /// <summary>
    /// Latitude coordinate of the company headquarters.
    /// </summary>
    internal static double? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the company headquarters.
    /// </summary>
    internal static double? Longitude { get; set; }

    /// <summary>
    /// Average car speed in km/h for delivery time estimation.
    /// </summary>
    internal static double AvgCarSpeedKmh { get; set; }

    /// <summary>
    /// Average motorcycle speed in km/h for delivery time estimation.
    /// </summary>
    internal static double AvgMotorcycleSpeedKmh { get; set; }

    /// <summary>
    /// Average bicycle speed in km/h for delivery time estimation.
    /// </summary>
    internal static double AvgBicycleSpeedKmh { get; set; }

    /// <summary>
    /// Average walking speed in km/h for delivery time estimation.
    /// </summary>
    internal static double AvgWalkingSpeedKmh { get; set; }

    /// <summary>
    /// Maximum allowed delivery distance in km for a standard delivery.
    /// </summary>
    internal static double? MaxGeneralDeliveryDistanceKm { get; set; }

    /// <summary>
    /// Maximum allowed delivery duration before it is considered late.
    /// </summary>
    internal static TimeSpan MaxDeliveryTimeSpan { get; set; }

    /// <summary>
    /// Time range considered as "risky" or nearing late delivery.
    /// </summary>
    internal static TimeSpan RiskRange { get; set; }

    /// <summary>
    /// Time period without deliveries after which a courier is considered inactive.
    /// </summary>
    internal static TimeSpan InactivityRange { get; set; }

    /// <summary>
    /// Resets all configuration values to their defaults.
    /// </summary>
    internal static void Reset()
    {
        nextOrderId = startOrderId;
        nextDeliveryId = startDeliveryId;
        Clock = DateTime.Now;
        AdminId = 0;
        AdminPassword = "Admin";
        CompanyFullAddress = null;
        Latitude = null;
        Longitude = null;
        MaxGeneralDeliveryDistanceKm = null;
        MaxDeliveryTimeSpan = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        InactivityRange = TimeSpan.Zero;
    }
}
