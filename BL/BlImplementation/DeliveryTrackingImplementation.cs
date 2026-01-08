using System;
using BlApi;
using BL.Helpers;

namespace BlImplementation;

/// <summary>
/// Implementation of the IDeliveryTracking interface.
/// </summary>
internal sealed class DeliveryTrackingImplementation : IDeliveryTracking
{
    /// <inheritdoc />
    public BO.DeliveryTracking Read(int deliveryId)
    {
        return DeliveryTrackingManager.Read(deliveryId);
    }

    /// <inheritdoc />
    public BO.DeliveryTracking ReadActiveForCourier(int courierId)
    {
        return DeliveryTrackingManager.ReadActiveForCourier(courierId);
    }
}
