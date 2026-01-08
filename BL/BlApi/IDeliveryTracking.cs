namespace BlApi;

/// <summary>
/// Defines the interface for retrieving real-time tracking information for deliveries.
/// </summary>
public interface IDeliveryTracking
{
    /// <summary>
    /// Retrieves a detailed, real-time snapshot of an ongoing delivery.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to track.</param>
    /// <returns>A BO.DeliveryTracking object with all current tracking details.</returns>
    BO.DeliveryTracking Read(int deliveryId);

    /// <summary>
    /// For a given courier, retrieves the tracking info for their currently active delivery.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A BO.DeliveryTracking object, or null if the courier has no active delivery.</returns>
    BO.DeliveryTracking ReadActiveForCourier(int courierId);
}
