namespace BO;

/// <summary>
/// Business layer DeliverySession entity representing an active delivery work session for a courier.
/// This entity has no direct equivalent in the data layer.
/// </summary>
public class DeliverySession
{
    /// <summary>
    /// Unique identifier for the delivery session.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The courier performing deliveries in this session.
    /// </summary>
    public BO.Courier Courier { get; set; }

    /// <summary>
    /// Timestamp when the delivery session started.
    /// </summary>
    public DateTime SessionStartTime { get; set; }

    /// <summary>
    /// Timestamp when the delivery session ended.
    /// Null if the session is still in progress.
    /// </summary>
    public DateTime? SessionEndTime { get; set; }

    /// <summary>
    /// Collection of deliveries performed during this session.
    /// </summary>
    public List<BO.DeliveryInList> Deliveries { get; set; } = new();

    /// <summary>
    /// Total distance covered during this delivery session (in km).
    /// </summary>
    public double TotalDistanceCovered { get; set; }

    /// <summary>
    /// Number of successful deliveries completed in this session.
    /// </summary>
    public int SuccessfulDeliveriesCount { get; set; }

    /// <summary>
    /// Number of failed delivery attempts in this session.
    /// </summary>
    public int FailedDeliveriesCount { get; set; }

    /// <summary>
    /// Duration of the delivery session from start to end (or until now if still active).
    /// </summary>
    public TimeSpan SessionDuration
    {
        get => (SessionEndTime ?? DateTime.Now) - SessionStartTime;
    }

    /// <summary>
    /// Indicates whether the delivery session is currently in progress.
    /// </summary>
    public bool IsActive
    {
        get => SessionEndTime == null;
    }

    /// <summary>
    /// Returns a string representation of the DeliverySession entity.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}