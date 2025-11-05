namespace DO;

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
