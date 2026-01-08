

namespace BO;

/// <summary>
/// Business layer Order entity representing a customer order in the delivery system.
/// Contains data properties, some of which are populated by the Business Logic layer.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Type/category of the order.
    /// </summary>
    public OrderTypes OrderType { get; set; }

    /// <summary>
    /// Verbal/short description of the order contents.
    /// </summary>
    public string VerbalDescription { get; set; }

    /// <summary>
    /// Latitude coordinate of the delivery destination.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the delivery destination.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Full name of the customer making the order.
    /// </summary>
    public string CustomerFullName { get; set; }

    /// <summary>
    /// Customer's mobile phone number.
    /// </summary>
    public string CustomerMobile { get; set; }

    /// <summary>
    /// Volume of the package being delivered (in cubic units).
    /// </summary>
    public double Volume { get; set; }

    /// <summary>
    /// Weight of the package being delivered (in kg).
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Indicates if the contents are fragile and require special handling.
    /// </summary>
    public bool Fragile { get; set; }

    /// <summary>
    /// Package height dimension (in cm).
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Package width dimension (in cm).
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Timestamp when the order was created and opened.
    /// </summary>
    public DateTime OrderOpenTime { get; set; }

    /// <summary>
    /// Time elapsed since the order was opened. Set by the BL.
    /// </summary>
    public TimeSpan TimeOpenedDuration { get; set; }

    /// <summary>
    /// Package density (weight/volume ratio). Set by the BL.
    /// </summary>
    public double PackageDensity { get; set; }

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public OrderStatus OrderStatus { get; set; }

    /// <summary>
    /// Returns a string representation of the Order entity.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}

