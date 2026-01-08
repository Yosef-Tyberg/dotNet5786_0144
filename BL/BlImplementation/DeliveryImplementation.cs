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
        return DeliveryManager.ReadAllDeliveriesForList(filter);
    }

    /// <inheritdoc />
    public BO.Delivery Read(int deliveryId)
    {
        return DeliveryManager.ReadDelivery(deliveryId);
    }

    /// <inheritdoc />
    public void PickUp(int courierId)
    {
        DeliveryManager.PickUp(courierId);
    }

    /// <inheritdoc />
    public void Deliver(int courierId)
    {
        DeliveryManager.Deliver(courierId);
    }

    /// <inheritdoc />
    public BO.Delivery GetMyCurrentDelivery(int courierId)
    {
        return DeliveryManager.GetMyCurrentDelivery(courierId);
    }
}
