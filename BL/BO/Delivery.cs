using BL.Helpers;

namespace BO;

/// <summary>
/// Business layer Delivery entity representing a delivery task in the system.
/// Contains data properties, some of which are populated by the Business Logic layer.
/// </summary>
public class Delivery
{
    /// <summary>
    /// Unique identifier for the delivery.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Identifier of the order being delivered.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Identifier of the courier performing the delivery.
    /// </summary>
    public int CourierId { get; set; }

    /// <summary>
    /// Type of delivery method used.
    /// </summary>
    public DeliveryTypes DeliveryType { get; set; }

    /// <summary>
    /// Timestamp indicating when the delivery began.
    /// </summary>
    public DateTime DeliveryStartTime { get; set; }

    /// <summary>
    /// Actual distance traveled during delivery (in km), if known.
    /// </summary>
    public double? ActualDistance { get; set; }

    /// <summary>
    /// Result/status of the delivery (e.g., Delivered, Failed, Cancelled).
    /// </summary>
    public DeliveryEndTypes? DeliveryEndType { get; set; }

    /// <summary>
    /// Timestamp when the delivery was completed or closed.
    /// </summary>
    public DateTime? DeliveryEndTime { get; set; }

    /// <summary>
    /// Time elapsed during the delivery. Set by the BL.
    /// </summary>
    public TimeSpan DeliveryDuration { get; set; }

    /// <summary>
    /// Indicates whether the delivery is currently in progress. Set by the BL.
    /// </summary>
    public bool IsInProgress { get; set; }

    /// <summary>
    /// Average speed during the delivery (in km/h). Set by the BL.
    /// </summary>
    public double AverageSpeed { get; set; }

    /// <summary>
    /// Returns a string representation of the Delivery entity.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}