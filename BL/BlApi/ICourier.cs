namespace BlApi;

/// <summary>
/// Defines the interface for managing Courier entities in the business logic layer.
/// Covers actions for both administrators and couriers.
/// </summary>
public interface ICourier
{
    #region Administrator Methods

    /// <summary>
    /// Retrieves a filtered and sorted list of all couriers.
    /// </summary>
    /// <param name="filter">Optional predicate to filter couriers.</param>
    /// <returns>An IEnumerable of BO.CourierInList.</returns>
    IEnumerable<BO.CourierInList> ReadAll(Func<BO.Courier, bool>? filter = null);

    /// <summary>
    /// Retrieves the full details of a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier to retrieve.</param>
    /// <returns>A BO.Courier object.</returns>
    BO.Courier Read(int courierId);

    /// <summary>
    /// Adds a new courier to the system.
    /// </summary>
    /// <param name="courier">The BO.Courier object representing the new courier.</param>
    void Create(BO.Courier courier);

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="courierId">The ID of the courier to delete.</param>
    void Delete(int courierId);

    /// <summary>
    /// Updates the details of an existing courier.
    /// </summary>
    /// <param name="newDetails">A BO.Courier object containing the updated information.</param>
    void Update(BO.Courier newDetails);

    /// <summary>
    /// Retrieves the delivery history for a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>An IEnumerable of BO.DeliveryInList representing the courier's past deliveries.</returns>
    IEnumerable<BO.DeliveryInList> GetCourierDeliveryHistory(int courierId);

    /// <summary>
    /// Retrieves statistical data for a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A BO.CourierStatistics object.</returns>
    BO.CourierStatistics GetCourierStatistics(int courierId);

    #endregion

    #region Courier Methods

    /// <summary>
    /// Allows a courier to update their own personal details.
    /// </summary>
    /// <param name="courierId">The ID of the courier updating their details.</param>
    /// <param name="fullName">Optional new full name.</param>
    /// <param name="phoneNum">Optional new phone number.</param>
    /// <param name="email">Optional new email.</param>
    /// <param name="password">Optional new password.</param>
    /// <param name="maxDistance">Optional new max delivery distance.</param>
    void UpdateMyDetails(int courierId, string? fullName = null, string? phoneNum = null, string? email = null, string? password = null, double? maxDistance = null);
    
    /// <summary>
    /// Retrieves a list of open orders that are within the courier's allowed range.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>An IEnumerable of BO.OrderInList representing the open orders.</returns>
    IEnumerable<BO.OrderInList> GetOpenOrders(int courierId);

    #endregion
}
