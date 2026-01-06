namespace BO;

/// <summary>
/// Business layer Courier entity representing a courier in the delivery system.
/// Contains all properties from the data layer Courier entity plus calculated properties.
/// </summary>
public class Courier
{
    /// <summary>
    /// Unique identifier of the courier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Full name of the courier.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Mobile phone number of the courier.
    /// </summary>
    public string MobilePhone { get; set; }

    /// <summary>
    /// Email address of the courier.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Login password of the courier (hashed in production).
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Indicates if the courier is currently active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Type of delivery vehicle/method used by the courier.
    /// </summary>
    public DeliveryTypes DeliveryType { get; set; }

    /// <summary>
    /// Date and time the courier began employment.
    /// </summary>
    public DateTime EmploymentStartTime { get; set; }

    /// <summary>
    /// Maximum distance (in km) the courier is willing to deliver, if limited.
    /// </summary>
    public double? PersonalMaxDeliveryDistance { get; set; }

    /// <summary>
    /// Duration the courier has been employed at the company.
    /// Calculated from EmploymentStartTime to the current date.
    /// </summary>
    public TimeSpan EmploymentDuration
    {
        get => DateTime.Now - EmploymentStartTime;
    }

    /// <summary>
    /// Years the courier has been employed at the company.
    /// Calculated from EmploymentStartTime to the current date.
    /// </summary>
    public double YearsEmployed
    {
        get => EmploymentDuration.TotalDays / 365.25;
    }

    /// <summary>
    /// Returns a string representation of the Courier entity.
    /// </summary>
    public override string ToString() => BL.Helpers.Tools.ToStringProperty(this);
}

/// <summary>
/// Minimal view of a Courier entity for use in collections or summary displays.
/// </summary>
public class CourierInList
{
    /// <summary>
    /// Unique identifier of the courier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Full name of the courier.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Type of delivery vehicle/method used by the courier.
    /// </summary>
    public DeliveryTypes DeliveryType { get; set; }

    /// <summary>
    /// Indicates if the courier is currently active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Returns a string representation of the CourierInList entity.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}