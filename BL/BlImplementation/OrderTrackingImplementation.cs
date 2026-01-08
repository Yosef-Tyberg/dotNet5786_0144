namespace BlImplementation;

/// <summary>
/// Implementation of the IOrderTracking interface.
/// </summary>
internal sealed class OrderTrackingImplementation : BlApi.IOrderTracking
{
    /// <inheritdoc />
    public BO.OrderTracking Track(int orderId) => Helpers.OrderTrackingManager.Track(orderId);
}