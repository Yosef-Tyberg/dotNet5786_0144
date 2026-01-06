namespace BO;

/// <summary>
/// Business layer Delivery entity representing a delivery task in the system.
/// Contains all properties from the data layer Delivery entity plus calculated properties.
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
    /// Null if distance has not been recorded or measured.
    /// </summary>
    public double? ActualDistance { get; set; }

    /// <summary>
    /// Result/status of the delivery (e.g., Delivered, Failed, Cancelled).
    /// Null if delivery is still in progress.
    /// </summary>
    public DeliveryEndTypes? DeliveryEndType { get; set; }

    /// <summary>
    /// Timestamp when the delivery was completed or closed.
    /// Null if delivery is still in progress.
    /// </summary>
    public DateTime? DeliveryEndTime { get; set; }

    /// <summary>
    /// Time elapsed since the delivery started until now or until completion.
    /// </summary>
    public TimeSpan DeliveryDuration
    {
        get => (DeliveryEndTime ?? DateTime.Now) - DeliveryStartTime;
    }

    /// <summary>
    /// Indicates whether the delivery is currently in progress (no end type set).
    /// </summary>
    public bool IsInProgress
    {
        get => DeliveryEndType == null;
    }

    /// <summary>
    /// Average speed during the delivery (in km/h) if distance and duration are available.
    /// Returns 0 if distance is unknown or duration is zero.
    /// </summary>
    public double AverageSpeed
    {
        get
        {
            if (!ActualDistance.HasValue || ActualDistance == 0 || DeliveryDuration.TotalHours == 0)
                return 0;
            return ActualDistance.Value / DeliveryDuration.TotalHours;
        }
    }

    /// <summary>
    /// Returns a string representation of the Delivery entity.
    /// </summary>
    public override string ToString() => BL.Helpers.Tools.ToStringProperty(this);
}