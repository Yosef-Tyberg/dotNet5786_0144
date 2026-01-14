﻿using System;
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

    public static IEnumerable<BO.Delivery> ReadAll(Func<DO.Delivery, bool>? filter = null)
    {
        try
        {
            return s_dal.Delivery.ReadAll(filter).Where(d => d != null).Select(d => ConvertDoToBo(d!));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    public static BO.Delivery ReadDelivery(int deliveryId)
    {
        try
        {
            var doDelivery = s_dal.Delivery.Read(deliveryId);
            return ConvertDoToBo(doDelivery);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found.", ex);
        }
    }

    public static BO.Delivery? GetMyCurrentDelivery(int courierId)
    {
        // Find an active delivery for the courier (not yet delivered)
        var doDelivery = s_dal.Delivery.ReadAll()
            .FirstOrDefault(d => d.CourierId == courierId && d.DeliveryEndTime == null);

        if (doDelivery == null)
        {
            return null; // No active delivery
        }

        // We already have the DO object, so just convert it.
        return ConvertDoToBo(doDelivery);
    }
    
    public static void PickUp(int courierId, int orderId)
    {
        try
        {
            // Verify that the order exists
            var order = OrderManager.Read(orderId);

            // Verify order is available for pickup
            if (order.OrderStatus == BO.OrderStatus.InProgress)
            {
                throw new BO.BlOrderAlreadyAssignedException($"Order {orderId} cannot be picked up because it is in status {order.OrderStatus}.");
            }

            if (order.OrderStatus != BO.OrderStatus.Open)
            {
                throw new BO.BlDeliveryAlreadyClosedException($"Order {orderId} cannot be picked up because it is {order.OrderStatus}.");
            }

            // Verify courier exists and is active
            var courier = CourierManager.ReadCourier(courierId);
            if (!courier.Active)
                throw new BO.BlInvalidInputException($"Courier {courierId} is not active.");
            
            // Verify courier doesn't have an active delivery
            if (s_dal.Delivery.ReadAll().Any(d => d.CourierId == courierId && d.DeliveryEndTime == null))
                throw new BO.BlCourierAlreadyHasDeliveryException(
                    $"Courier ID '{courierId}' already has an active delivery.");

            // Create a new delivery, which now starts immediately
            Create(orderId, courierId);
        }
        catch (Exception ex)
        {
            if (ex.GetType().Namespace == "DO")
                throw Tools.ConvertDalException(ex);
            
            throw;
        }
    }

    public static void Deliver(int courierId, BO.DeliveryEndTypes endType)
    {
        var delivery = s_dal.Delivery.ReadAll().FirstOrDefault(d => d.CourierId == courierId && d.DeliveryStartTime <= AdminManager.Now && d.DeliveryEndTime == null);
        if(delivery == null)
            throw new BO.BlCourierHasNoActiveDeliveryException("Courier has no picked-up delivery to deliver.");

        delivery = delivery with { DeliveryEndTime = AdminManager.Now, DeliveryEndType = (DO.DeliveryEndTypes)endType };
        
        s_dal.Delivery.Update(delivery);
    }

    /// <summary>
    /// Calculates the actual distance for a delivery.
    /// </summary>
    /// <param name="delivery">The delivery for which to calculate the distance.</param>
    /// <returns>The calculated distance.</returns>
    private static double CalculateActualDistance(DO.Delivery delivery)
    {
        var order = OrderManager.Read(delivery.OrderId);
        var config = AdminManager.GetConfig();
        return (BO.DeliveryTypes)delivery.DeliveryType switch
        {
            BO.DeliveryTypes.Car or BO.DeliveryTypes.Motorcycle => Tools.GetDrivingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude),
            BO.DeliveryTypes.Bicycle or BO.DeliveryTypes.OnFoot => Tools.GetWalkingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude),
            _ => throw new BO.BlMissingPropertyException($"Could not calculate properties for unrecognized delivery type: {(BO.DeliveryTypes)delivery.DeliveryType}"),
        };
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
            var boDelivery = new BO.Delivery
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
            
            //add business logic
            if(boDelivery.DeliveryEndTime is not null)
            {
                boDelivery.DeliveryDuration = (TimeSpan)(boDelivery.DeliveryEndTime - boDelivery.DeliveryStartTime);
                boDelivery.AverageSpeed = boDelivery.ActualDistance is not null && boDelivery.DeliveryDuration.TotalHours > 0 
                    ? (double)boDelivery.ActualDistance / boDelivery.DeliveryDuration.TotalHours 
                    : 0;
            }
            
            var order = OrderManager.Read(boDelivery.OrderId);
            var config = AdminManager.GetConfig();
            
            boDelivery.MaximumDeliveryTime = order.OrderOpenTime + config.MaxDeliveryTimeSpan;



            boDelivery.ExpectedDeliveryTime = CalculateExpectedDeliveryTime(boDelivery, order, config);



            boDelivery.ScheduleStatus = DetermineScheduleStatus(boDelivery, config.RiskRange);



            return boDelivery;

        }

        catch (Exception ex)

        {

            throw Tools.ConvertDalException(ex);

        }

    }



    /// <summary>

    /// Calculates the expected delivery time for a given delivery.

    /// </summary>

    /// <param name="delivery">The delivery.</param>

    /// <param name="order">The associated order.</param>

    /// <param name="config">The application configuration.</param>

    /// <returns>The calculated expected delivery time.</returns>

    private static DateTime CalculateExpectedDeliveryTime(BO.Delivery delivery, BO.Order order, BO.Config config)

    {

        double distance;

        double speed;

        switch (delivery.DeliveryType)

        {

            case BO.DeliveryTypes.Car:

                distance = Tools.GetDrivingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);

                speed = config.AvgCarSpeedKmh;

                break;

            case BO.DeliveryTypes.Motorcycle:

                distance = Tools.GetDrivingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);

                speed = config.AvgMotorcycleSpeedKmh;

                break;

            case BO.DeliveryTypes.Bicycle:

                distance = Tools.GetWalkingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);

                speed = config.AvgBicycleSpeedKmh;

                break;

            case BO.DeliveryTypes.OnFoot:

                distance = Tools.GetWalkingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);

                speed = config.AvgWalkingSpeedKmh;

                break;

            default:

                throw new BO.BlMissingPropertyException($"Could not calculate properties for unrecognized delivery type: {delivery.DeliveryType}");

        }

        if (speed >= 1)

        {

            var estimatedHours = distance / speed;

            return delivery.DeliveryStartTime.AddHours(estimatedHours);

        }

        throw new BO.BlMissingPropertyException($"speed must be >= 1");
    }



    /// <summary>

    /// Determines the schedule status of a delivery.

    /// </summary>

    /// <param name="delivery">The delivery to check.</param>

    /// <param name="riskRange">The time span defining the risk range.</param>

    /// <returns>The calculated schedule status.</returns>

    private static BO.ScheduleStatus DetermineScheduleStatus(BO.Delivery delivery, TimeSpan riskRange)

    {

        if ((delivery.DeliveryEndTime == null && AdminManager.Now > delivery.MaximumDeliveryTime) || (delivery.DeliveryEndTime != null && delivery.DeliveryEndTime > delivery.MaximumDeliveryTime))

        {

            return BO.ScheduleStatus.Late;

        }



        if (delivery.DeliveryEndTime == null && (delivery.MaximumDeliveryTime - AdminManager.Now) < riskRange)

        {

            return BO.ScheduleStatus.AtRisk;

        }



        return BO.ScheduleStatus.OnTime;

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

                ScheduleStatus = boDelivery.ScheduleStatus,

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
        return s_dal.Delivery.ReadAll().Any(d => d.OrderId == orderId && d.DeliveryEndTime == null);
    }
    
    /// <summary>
    /// Creates a new delivery for a given order and courier.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <param name="courierId">The ID of the courier.</param>
    public static void Create(int orderId, int courierId)
    {
        BO.Courier courier = CourierManager.ReadCourier(courierId);
        var order = OrderManager.Read(orderId);
        var config = AdminManager.GetConfig();

        if (!CourierManager.IsOrderInCourierRange(order, courier, (double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0)))
        {
            throw new BO.BlInvalidInputException($"Order location is too far for this courier. a maximum of {courier.PersonalMaxDeliveryDistance}km is allowed");
        }

        // Determine the delivery type based on the courier's vehicle
        DO.DeliveryTypes deliveryType = (DO.DeliveryTypes)courier.DeliveryType;
        
        var tempDelivery = new DO.Delivery(
            Id: 0, 
            OrderId: orderId,
            CourierId: courierId,
            DeliveryType: deliveryType,
            DeliveryStartTime: AdminManager.Now,
            ActualDistance: null,
            DeliveryEndType: null,
            DeliveryEndTime: null
        );
        
        double distance = CalculateActualDistance(tempDelivery);

        var newDelivery = tempDelivery with { ActualDistance = distance };

        s_dal.Delivery.Create(newDelivery);
    }

    /// <summary>
    /// Creates a new dummy delivery for a cancelled order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    public static void CreateCancelledDelivery(int orderId)
    {
        var dummyDelivery = new DO.Delivery
        (
            Id: 0, // Set to 0 to indicate a new delivery
            OrderId: orderId,
            CourierId: 0, // Dummy courier
            DeliveryType: DO.DeliveryTypes.OnFoot, // Or any default
            DeliveryStartTime: AdminManager.Now,
            DeliveryEndTime: AdminManager.Now,
            DeliveryEndType: DO.DeliveryEndTypes.Cancelled,
            ActualDistance: 0
        );
        s_dal.Delivery.Create(dummyDelivery);
    }

    /// <summary>
    /// a method for periodic updates of the delivery
    /// </summary>
    /// <param name="oldClock"></param>
    /// <param name="newClock"></param>
    public static void PeriodicDeliveriesUpdate(DateTime oldClock, DateTime newClock)
    {
        //implementation will be added in the future
    }
}
