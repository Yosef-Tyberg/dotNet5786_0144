using System;
using System.Linq;

namespace Helpers;

/// <summary>
/// Provides helper methods for delivery tracking logic.
/// </summary>
internal static class DeliveryTrackingManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    public static BO.DeliveryTracking Read(int deliveryId)
    {
        try
        {
            var doDelivery = s_dal.Delivery.Read(deliveryId);
            return CreateDeliveryTracking(doDelivery);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found.", ex);
        }
    }
    
    public static BO.DeliveryTracking ReadActiveForCourier(int courierId)
    {
        var doDelivery = s_dal.Delivery.ReadAll().FirstOrDefault(d => d.CourierId == courierId && d.DeliveryEndTime == null);
        if (doDelivery == null)
        {
            return null;
        }
        return CreateDeliveryTracking(doDelivery);
    }

    private static BO.DeliveryTracking CreateDeliveryTracking(DO.Delivery doDelivery)
    {
        var boDelivery = DeliveryManager.ReadDelivery(doDelivery.Id);

        // This is a placeholder for real tracking logic.
        // In a real system, this would involve GPS data, traffic APIs, etc.
        var tracking = new BO.DeliveryTracking
        {
            Id = boDelivery.Id,
            CourierId = boDelivery.CourierId,
            CourierName = "TBD", // To be implemented
            CurrentLatitude = 0, // Placeholder
            CurrentLongitude = 0, // Placeholder
            DistanceToPickup = 1, // Placeholder
            DistanceToDropoff = 1, // Placeholder
            TotalDistance = 2, // Placeholder
            EstimatedTimeOfArrival = DateTime.Now.AddHours(1), // Placeholder
            ProgressUpdates = Enumerable.Empty<BO.ProgressUpdate>() // Placeholder
        };

        if(boDelivery.DeliveryEndTime is not null)
        {
            tracking.Status = BO.DeliveryStatus.Delivered;
        } 
        else if (boDelivery.DeliveryStartTime > AdminManager.Now)
        {
            tracking.Status = BO.DeliveryStatus.Assigned;
        }
        else 
        {
            tracking.Status = BO.DeliveryStatus.PickedUp;
        }


        return tracking;
    }
}