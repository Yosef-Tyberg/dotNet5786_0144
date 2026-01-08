namespace BO;

/// <summary>
/// Business Object representing the tracking details of an order, including its status and delivery history.
/// </summary>
public class OrderTracking
{
    /// <summary>
    /// Unique identifier of the order.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// The courier currently assigned to the order, if applicable.
    /// </summary>
    public CourierInList? AssignedCourier { get; set; }

    /// <summary>
    /// Collection of all delivery attempts associated with this order, ordered by creation time.
    /// </summary>
    public IEnumerable<DeliveryInList>? DeliveryHistory { get; set; }

    /// <summary>
    /// Short description of the order contents.
    /// </summary>
    public string? VerbalDescription { get; set; }

    /// <summary>
    /// Full name of the customer who placed the order.
    /// </summary>
    public string? CustomerFullName { get; set; }

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    public DateTime? OrderOpenTime { get; set; }

    /// <summary>
    /// The estimated time of delivery based on distance and courier speed.
    /// </summary>
    public DateTime? ExpectedDeliveryTime { get; set; }

    /// <summary>
    /// The maximum allowable time for delivery based on the order creation time and company policy.
    /// </summary>
    public DateTime? MaxDeliveryTime { get; set; }

    /// <summary>
    /// Returns a string representation of the OrderTracking entity.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}