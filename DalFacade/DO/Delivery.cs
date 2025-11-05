namespace DO;
/// <summary>
/// Delivery Entity represents the delivery details for a specific order.
/// </summary>
/// <param name="Id">Unique identifier for the delivery</param>
/// <param name="OrderId">ID of the associated order being delivered</param>
/// <param name="CourierId">ID of the courier assigned to the delivery</param>
/// <param name="DeliveryType">Type of delivery method used</param>
/// <param name="DeliveryStartTime">Timestamp indicating when the delivery began</param>
/// <param name="ActualDistance">Actual distance traveled during delivery, if known</param>
/// <param name="DeliveryEndType">Result of the delivery (e.g., Delivered, Failed, Returned)</param>
/// <param name="DeliveryEndTime">Timestamp when the delivery was completed or closed</param>
public record Delivery
(
    int Id,
    int OrderId,
    int CourierId,
    DeliveryTypes DeliveryType,
    DateTime DeliveryStartTime,
    double? ActualDistance = null,
    DeliveryEndTypes? DeliveryEndType = null,
    DateTime? DeliveryEndTime = null
)
{
    public Delivery() : this (0, 0, 0, DeliveryTypes.OnFoot, DateTime.Now) {}
}
