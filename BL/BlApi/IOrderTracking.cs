namespace BlApi;

/// <summary>
/// Interface for tracking order status and history.
/// </summary>
public interface IOrderTracking
{
    /// <summary>
    /// Retrieves the full tracking details of a specific order, including status, courier, and delivery history.
    /// </summary>
    /// <param name="orderId">The ID of the order to track.</param>
    /// <returns>A BO.OrderTracking object.</returns>
    BO.OrderTracking Track(int orderId);
}