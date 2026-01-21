
namespace BO;

/// <summary>
/// Business layer Courier entity representing a courier in the delivery system.
/// Contains data properties, some of which are populated by the Business Logic layer.
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
    /// Years the courier has been employed. Set by the BL.
    /// </summary>
    public double YearsEmployed { get; set; }

    /// <summary>
    /// Returns a string representation of the Courier entity.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}

