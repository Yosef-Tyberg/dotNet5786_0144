namespace BO;

/// <summary>
/// Minimal view of an Order entity for use in lists or summary displays.
/// Contains only essential identifying and customer information.
/// </summary>
public class OrderInList
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
    /// Full name of the customer making the order.
    /// </summary>
    public string CustomerFullName { get; set; }

    /// <summary>
    /// Timestamp when the order was created and opened.
    /// </summary>
    public DateTime OrderOpenTime { get; set; }

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public OrderStatus OrderStatus { get; set; }

    /// <summary>
    /// Returns a string representation of the OrderInList entity.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}