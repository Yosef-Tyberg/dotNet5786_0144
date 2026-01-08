namespace BlApi;

/// <summary>
/// Defines the interface for managing Order entities in the business logic layer.
/// </summary>
public interface IOrder
{
    #region Administrator Methods

    /// <summary>
    /// Retrieves a list of all orders, with optional filtering.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the orders.</param>
    /// <returns>An IEnumerable of BO.OrderInList.</returns>
    IEnumerable<BO.OrderInList> ReadAll(Func<BO.OrderInList, bool>? filter = null);

    /// <summary>
    /// Retrieves the full details of a specific order.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>A BO.Order object.</returns>
    BO.Order Read(int orderId);

    /// <summary>
    /// Updates the details of an existing order.
    /// Can only be done before the order is assigned to a courier.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="updatedOrder">A BO.Order object with the new details.</param>
    void Update(int orderId, BO.Order updatedOrder);
    
    /// <summary>
    /// Adds a new order to the system.
    /// </summary>
    /// <param name="newOrder">The BO.Order object for the new order.</param>
    /// <returns>The ID of the newly created order.</returns>
    int Create(BO.Order newOrder);

    #endregion

    #region Courier Methods

    /// <summary>
    /// Retrieves a list of orders available for pickup, tailored to the courier's capabilities.
    /// </summary>
    /// <param name="courierId">The ID of the courier looking for orders.</param>
    /// <returns>An IEnumerable of BO.OrderInList that the courier can deliver.</returns>
    IEnumerable<BO.OrderInList> GetAvailableOrders(int courierId);

    /// <summary>
    /// Allows a courier to take/assign an order for delivery.
    /// This will create a new Delivery entity.
    /// </summary>
    /// <param name="orderId">The ID of the order to take.</param>
    /// <param name="courierId">The ID of the courier taking the order.</param>
    void TakeOrder(int orderId, int courierId);

    #endregion
}
