namespace BO;

/// <summary>
/// Minimal view of a Courier entity for use in lists or summary displays.
/// Contains only essential identifying and status information.
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
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}