using System;
using System.Collections.Generic;
using BlApi;
using Helpers;

namespace BlImplementation;

/// <summary>
/// Implementation of the IDelivery interface.
/// </summary>
internal sealed class DeliveryImplementation : IDelivery
{
    /// <inheritdoc />
    public IEnumerable<BO.DeliveryInList> ReadAll(Func<BO.DeliveryInList, bool>? filter = null)
    {
        var deliveries = DeliveryManager.ReadAll().Select(DeliveryManager.ConvertBoToDeliveryInList);
        return filter == null ? deliveries : deliveries.Where(filter);
    }

    /// <inheritdoc />
    public BO.Delivery Read(int deliveryId)
    {
        return DeliveryManager.ReadDelivery(deliveryId);
    }

    /// <inheritdoc />
    public void PickUp(int courierId, int orderId)
    {
        DeliveryManager.PickUp(courierId, orderId);
    }

    /// <inheritdoc />
    public void Deliver(int courierId, BO.DeliveryEndTypes endType)
    {
        DeliveryManager.Deliver(courierId, endType);
    }

    /// <inheritdoc />
    public BO.Delivery GetMyCurrentDelivery(int courierId)
    {
        return DeliveryManager.GetMyCurrentDelivery(courierId);
    }
}
