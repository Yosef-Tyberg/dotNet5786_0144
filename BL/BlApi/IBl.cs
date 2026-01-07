namespace BlApi;

/// <summary>
/// Main interface for the Business Logic layer.
/// Acts as a factory and entry point for accessing all logical entities and functionalities.
/// </summary>
public interface IBl
{
    /// <summary>
    /// Gets the interface for managing Courier entities.
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Gets the interface for managing Order entities.
    /// </summary>
    IOrder Order { get; }

    /// <summary>
    /// Gets the interface for managing Delivery entities.
    /// </summary>
    IDelivery Delivery { get; }

    /// <summary>
    /// Gets the interface for retrieving real-time tracking information for deliveries.
    /// </summary>
    IDeliveryTracking DeliveryTracking { get; }
}
