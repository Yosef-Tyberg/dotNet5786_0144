namespace BO;

/// <summary>
/// Minimal view of a Delivery entity for use in lists or summary displays.
/// Contains only essential identifying, timing, and status information.
/// </summary>
public class DeliveryInList
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
    /// Timestamp indicating when the delivery began.
    /// </summary>
    public DateTime DeliveryStartTime { get; set; }

    /// <summary>
    /// Result/status of the delivery.
    /// Null if delivery is still in progress.
    /// </summary>
    public DeliveryEndTypes? DeliveryEndType { get; set; }

    /// <summary>
    /// Returns a string representation of the DeliveryInList entity.
    /// </summary>
    public override string ToString() => BL.Helpers.Tools.ToStringProperty(this);
}