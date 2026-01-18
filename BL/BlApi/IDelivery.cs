namespace BlApi;

/// <summary>
/// Defines the interface for managing active Delivery entities in the business logic layer.
/// A Delivery is created when a Courier takes an Order.
/// </summary>
public interface IDelivery : IObservable
{
    #region Administrator Methods

    /// <summary>
    /// Retrieves a list of all deliveries, with optional filtering.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the deliveries.</param>
    /// <returns>An IEnumerable of BO.DeliveryInList.</returns>
    IEnumerable<BO.DeliveryInList> ReadAll(Func<BO.DeliveryInList, bool>? filter = null);

    /// <summary>
    /// Retrieves the full details of a specific delivery.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to retrieve.</param>
    /// <returns>A BO.Delivery object.</returns>
    BO.Delivery Read(int deliveryId);

    // Note: Viewing logs would likely be part of GetDeliveryDetails or a more advanced tracking entity.

    #endregion

    #region Courier Methods

    /// <summary>
    /// Allows a courier to pick up a specific order, which creates a new delivery record and marks the delivery as started.
    /// </summary>
    /// <param name="courierId">The ID of the courier performing the action.</param>
    /// <param name="orderId">The ID of the order being picked up.</param>
    void PickUp(int courierId, int orderId);

    /// <summary>
    /// Allows a courier to update the status of their current delivery to 'Delivered' or another final state.
    /// </summary>
    /// <param name="courierId">The ID of the courier performing the action.</param>
    /// <param name="endType">The final status of the delivery.</param>
    void Deliver(int courierId, BO.DeliveryEndTypes endType);

    /// <summary>
    /// Retrieves the details of the currently active delivery for a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A BO.Delivery object for the active delivery, or null if none exists.</returns>
    BO.Delivery? GetDeliveryByCourier(int courierId);

    #endregion
}
