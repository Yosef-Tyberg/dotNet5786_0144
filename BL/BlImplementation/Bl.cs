using BlApi;

namespace BlImplementation;

/// <summary>
/// The main implementation of the IBl interface.
/// This class acts as a factory for all the business logic interfaces.
/// </summary>
internal sealed class Bl : IBl
{
    /// <inheritdoc />
    public ICourier Courier { get; } = new CourierImplementation();

    /// <inheritdoc />
    public IOrder Order { get; } = new OrderImplementation();

    /// <inheritdoc />
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <inheritdoc />
    public IOrderTracking OrderTracking { get; } = new OrderTrackingImplementation();

    /// <inheritdoc />
    public IAdmin Admin { get; } = new AdminImplementation();
}
