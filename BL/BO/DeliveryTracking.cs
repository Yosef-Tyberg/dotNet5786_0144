

namespace BO;

/// <summary>
/// A business object that provides a detailed, real-time snapshot of an ongoing delivery.
/// This is a read-only object populated by the Business Logic layer.
/// </summary>
public class DeliveryTracking
{
    /// <summary>
    /// The ID of the delivery being tracked.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The current status of the delivery.
    /// </summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// The ID of the courier assigned to the delivery.
    /// </summary>
    public int CourierId { get; set; }
    
    /// <summary>
    /// The name of the courier.
    /// </summary>
    public string? CourierName { get; set; }

    /// <summary>
    /// The courier's current location (latitude).
    /// </summary>
    public double CurrentLatitude { get; set; }

    /// <summary>
    /// The courier's current location (longitude).
    /// </summary>
    public double CurrentLongitude { get; set; }

    /// <summary>
    /// The calculated distance from the courier to the pickup location.
    /// </summary>
    public double DistanceToPickup { get; set; }

    /// <summary>
    /// The calculated distance from the pickup location to the dropoff location.
    /// </summary>
    public double DistanceToDropoff { get; set; }

    /// <summary>
    /// The total expected travel distance for the delivery.
    /// </summary>
    public double TotalDistance { get; set; }

    /// <summary>
    /// The calculated Estimated Time of Arrival.
    /// </summary>
    public DateTime EstimatedTimeOfArrival { get; set; }

    /// <summary>
    /// A historical log of all progress updates for this delivery.
    /// </summary>
    public IEnumerable<ProgressUpdate>? ProgressUpdates { get; set; }
    
    /// <summary>
    /// Returns a string representation of the DeliveryTracking entity.
    /// </summary>
    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}
