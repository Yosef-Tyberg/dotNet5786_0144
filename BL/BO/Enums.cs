namespace BO;
/// <summary>
/// Types of delivery methods available for couriers.
/// </summary>
public enum DeliveryTypes
{
    /// <summary>
    /// Delivery performed by car.
    /// </summary>
    Car,

    /// <summary>
    /// Delivery performed by motorcycle.
    /// </summary>
    Motorcycle,

    /// <summary>
    /// Delivery performed by bicycle.
    /// </summary>
    Bicycle,

    /// <summary>
    /// Delivery performed on foot.
    /// </summary>
    OnFoot
}

/// <summary>
/// Result types for how a delivery ended.
/// </summary>
public enum DeliveryEndTypes
{
    /// <summary>
    /// Delivery attempt failed due to an error or exception.
    /// </summary>
    Failed,

    /// <summary>
    /// Delivery was cancelled before completion.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Delivery completed successfully and item delivered.
    /// </summary>
    Delivered,

    /// <summary>
    /// Customer refused to accept the delivery.
    /// </summary>
    CustomerRefused,

    /// <summary>
    /// Intended recipient could not be found at destination.
    /// </summary>
    RecipientNotFound
}

/// <summary>
/// Types/categories of orders the system supports.
/// </summary>
public enum OrderTypes
{
    /// <summary>
    /// Pizza order type.
    /// </summary>
    Pizza,

    /// <summary>
    /// Falafel order type.
    /// </summary>
    Falafel
}


/// <summary>
/// Represents the scheduling status of a delivery.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>
    /// An open or active order with enough time to deliver within the maximum delivery time window.
    /// </summary>
    OnTime,
    /// <summary>
    /// An open or active order with less than the risk time window, with time left before maximum delivery time.
    /// </summary>
    AtRisk,
    /// <summary>
    /// An open or active order exceeding the maximum delivery time, or closed after the maximum delivery time.
    /// </summary>
    Late
}

/// <summary>
/// Represents the status of an order based on its latest delivery.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Not assigned to any courier, and not yet closed.
    /// </summary>
    Open,
    /// <summary>
    /// Currently handled by a courier.
    /// </summary>
    InProgress,
    /// <summary>
    /// Order closed, customer received it, last delivery ended as "Delivered".
    /// </summary>
    Delivered,
    /// <summary>
    /// Order closed, last delivery ended as "Customer Refused".
    /// </summary>
    Refused,
    /// <summary>
    /// Order closed, last delivery ended as "Cancelled".
    /// </summary>
    Cancelled
}

