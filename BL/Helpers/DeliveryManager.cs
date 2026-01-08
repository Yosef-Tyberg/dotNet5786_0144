using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;

/// <summary>
/// Manager class for Delivery entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class DeliveryManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    public static IEnumerable<BO.DeliveryInList> ReadAllDeliveriesForList(Func<BO.DeliveryInList, bool>? filter = null)
    {
        var doDeliveries = s_dal.Delivery.ReadAll();
        var boDeliveries = doDeliveries.Select(d => ConvertDoToBo(d));
        var deliveryInList = boDeliveries.Select(ConvertBoToDeliveryInList);

        return filter == null ? deliveryInList : deliveryInList.Where(filter);
    }

    public static BO.Delivery ReadDelivery(int deliveryId)
    {
        try
        {
            var doDelivery = s_dal.Delivery.Read(deliveryId);
            var boDelivery = ConvertDoToBo(doDelivery);

            //add business logic
            if(boDelivery.DeliveryEndTime is not null)
            {
                boDelivery.DeliveryDuration = (TimeSpan)(boDelivery.DeliveryEndTime - boDelivery.DeliveryStartTime);
                boDelivery.AverageSpeed = boDelivery.ActualDistance is not null ? (double)boDelivery.ActualDistance / boDelivery.DeliveryDuration.TotalHours : 0;
            }
            
            var order = s_dal.Order.Read(boDelivery.OrderId);
            var config = AdminManager.GetConfig();
            var deliveryDeadline = order.OrderOpenTime + config.MaxDeliveryTimeSpan;

            if ((boDelivery.DeliveryEndTime == null && AdminManager.Now > deliveryDeadline) || (boDelivery.DeliveryEndTime != null && boDelivery.DeliveryEndTime > deliveryDeadline))
            {
                boDelivery.ScheduleStatus = BO.ScheduleStatus.Late;
            }
            else if (boDelivery.DeliveryEndTime == null && (deliveryDeadline - AdminManager.Now) < config.RiskRange)
            {
                boDelivery.ScheduleStatus = BO.ScheduleStatus.AtRisk;
            }
            else
            {
                boDelivery.ScheduleStatus = BO.ScheduleStatus.OnTime;
            }

            return boDelivery;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found.", ex);
        }
    }

    public static BO.Delivery GetMyCurrentDelivery(int courierId)
    {
        // Find an active delivery for the courier (not yet delivered)
        var delivery = s_dal.Delivery.ReadAll()
            .FirstOrDefault(d => d.CourierId == courierId && d.DeliveryEndTime == null);

        if (delivery == null)
        {
            return null; // No active delivery
        }

        return ReadDelivery(delivery.Id);
    }
    
    public static void PickUp(int courierId)
    {
        var delivery = s_dal.Delivery.ReadAll().FirstOrDefault(d => d.CourierId == courierId && d.DeliveryStartTime > AdminManager.Now && d.DeliveryEndTime == null);
        if(delivery == null)
            throw new BO.BlCourierHasNoActiveDeliveryException("Courier has no scheduled delivery to pick up.");
        
        delivery = delivery with { DeliveryStartTime = AdminManager.Now };
        
        s_dal.Delivery.Update(delivery);
    }

    public static void Deliver(int courierId)
    {
        var delivery = s_dal.Delivery.ReadAll().FirstOrDefault(d => d.CourierId == courierId && d.DeliveryStartTime < AdminManager.Now && d.DeliveryEndTime == null);
        if(delivery == null)
            throw new BO.BlCourierHasNoActiveDeliveryException("Courier has no picked-up delivery to deliver.");

        var order = s_dal.Order.Read(delivery.OrderId);
        var config = AdminManager.GetConfig();
        double distance;
        switch ((BO.DeliveryTypes)delivery.DeliveryType)
        {
            case BO.DeliveryTypes.Car:
            case BO.DeliveryTypes.Motorcycle:
                distance = Tools.GetDrivingDistance(config.Latitude, config.Longitude, order.Latitude, order.Longitude);
                break;
            case BO.DeliveryTypes.Bicycle:
            case BO.DeliveryTypes.Foot:
                distance = Tools.GetWalkingDistance(config.Latitude, config.Longitude, order.Latitude, order.Longitude);
                break;
            default:
                distance = Tools.CalculateDistance(config.Latitude, config.Longitude, order.Latitude, order.Longitude);
                break;
        }

        delivery = delivery with { DeliveryEndTime = AdminManager.Now, DeliveryEndType = DO.DeliveryEndTypes.Delivered, ActualDistance = distance };
        
        s_dal.Delivery.Update(delivery);
    }

    /// <summary>
    /// Converts a business layer Delivery entity to its data layer equivalent.
    /// </summary>
    /// <param name="boDelivery">The BO Delivery to convert.</param>
    /// <returns>An equivalent DO Delivery entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static DO.Delivery ConvertBoToDo(BO.Delivery boDelivery)
    {
        try
        {
            return new DO.Delivery(
                Id: boDelivery.Id,
                OrderId: boDelivery.OrderId,
                CourierId: boDelivery.CourierId,
                DeliveryType: (DO.DeliveryTypes)boDelivery.DeliveryType,
                DeliveryStartTime: boDelivery.DeliveryStartTime,
                ActualDistance: boDelivery.ActualDistance,
                DeliveryEndType: boDelivery.DeliveryEndType.HasValue ? (DO.DeliveryEndTypes?)boDelivery.DeliveryEndType.Value : null,
                DeliveryEndTime: boDelivery.DeliveryEndTime
            );
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Converts a data layer Delivery entity to its business layer equivalent.
    /// </summary>
    /// <param name="doDelivery">The DO Delivery to convert.</param>
    /// <returns>An equivalent BO Delivery entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.Delivery ConvertDoToBo(DO.Delivery doDelivery)
    {
        try
        {
            return new BO.Delivery
            {
                Id = doDelivery.Id,
                OrderId = doDelivery.OrderId,
                CourierId = doDelivery.CourierId,
                DeliveryType = (BO.DeliveryTypes)doDelivery.DeliveryType,
                DeliveryStartTime = doDelivery.DeliveryStartTime,
                ActualDistance = doDelivery.ActualDistance,
                DeliveryEndType = doDelivery.DeliveryEndType.HasValue ? (BO.DeliveryEndTypes?)doDelivery.DeliveryEndType.Value : null,
                DeliveryEndTime = doDelivery.DeliveryEndTime
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Converts a business layer Delivery entity to a summary list view.
    /// Summary entities are derived from full BO entities (not directly from DO)
    /// to ensure calculated properties and validations are preserved.
    /// </summary>
    /// <param name="boDelivery">The BO Delivery to convert.</param>
    /// <returns>A DeliveryInList summary entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.DeliveryInList ConvertBoToDeliveryInList(BO.Delivery boDelivery)
    {
        try
        {
            return new BO.DeliveryInList
            {
                Id = boDelivery.Id,
                OrderId = boDelivery.OrderId,
                CourierId = boDelivery.CourierId,
                DeliveryStartTime = boDelivery.DeliveryStartTime,
                DeliveryEndType = boDelivery.DeliveryEndType
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Checks if an order is already assigned to a delivery.
    /// </summary>
    /// <param name="orderId">The ID of the order to check.</param>
    /// <returns>True if the order is taken, false otherwise.</returns>
    public static bool IsOrderTaken(int orderId)
    {
        return s_dal.Delivery.ReadAll().Any(d => d.OrderId == orderId);
    }
    
    /// <summary>
    /// Creates a new delivery for a given order and courier.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <param name="courierId">The ID of the courier.</param>
    public static void Create(int orderId, int courierId)
    {
        BO.Courier courier = CourierManager.ReadCourier(courierId);

        if (courier.PersonalMaximumDistance.HasValue)
        {
            var order = s_dal.Order.Read(orderId);
            var config = AdminManager.GetConfig();
            var distance = Tools.CalculateDistance(config.Latitude, config.Longitude, order.Latitude, order.Longitude);
            if (distance > courier.PersonalMaximumDistance)
                throw new BO.BlInvalidInputException($"Order location is too far for this courier. a maximum of {courier.PersonalMaximumDistance}km is allowed");
        }

        // Determine the delivery type based on the courier's vehicle
        DO.DeliveryTypes deliveryType = (DO.DeliveryTypes)courier.DeliveryType;
        
        var newDelivery = new DO.Delivery(
            Id: 0, 
            OrderId: orderId,
            CourierId: courierId,
            DeliveryType: deliveryType,
            // The delivery starts in the future, when the courier decides to pick it up
            DeliveryStartTime: AdminManager.Now.Add(AdminManager.GetConfig().MaxDeliveryTimeSpan),
            ActualDistance: null,
            DeliveryEndType: null,
            DeliveryEndTime: null
        );

        s_dal.Delivery.Create(newDelivery);
    }
    
    public static void PeriodicDeliveriesUpdate(DateTime oldClock, DateTime newClock)
    {
        // The logic for this method is currently on hold per user request.
    }
}
