

namespace BO;

/// <summary>
/// Represents a single event or log entry in the history of a delivery.
/// </summary>
public class ProgressUpdate
{
    /// <summary>
    /// Timestamp of when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The status that was set at this point in time.
    /// </summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// Optional description of the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Returns a string representation of the ProgressUpdate.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}
