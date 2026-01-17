namespace BO;

/// <summary>
/// Represents statistical data for a courier.
/// </summary>
public class CourierStatistics
{
    /// <summary>
    /// The total number of deliveries completed by the courier.
    /// </summary>
    public int TotalDeliveries { get; set; }

    /// <summary>
    /// The total distance traveled by the courier for all completed deliveries.
    /// </summary>
    public double TotalDistance { get; set; }

    /// <summary>
    /// The average time taken for a delivery.
    /// </summary>
    public TimeSpan AverageDeliveryTime { get; set; }

    /// <summary>
    /// The number of deliveries that were completed on time.
    /// </summary>
    public int OnTimeDeliveries { get; set; }

    /// <summary>
    /// The number of deliveries that were completed late.
    /// </summary>
    public int LateDeliveries { get; set; }

    /// <summary>
    /// The success rate of the courier's deliveries.
    /// </summary>
    public double SuccessRate { get; set; }

    public override string ToString() => Helpers.Tools.ToStringProperty(this);
}
