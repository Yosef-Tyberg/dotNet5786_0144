﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;

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
            // 1. Fetch initial deliveries and materialize to avoid multiple enumerations
            var doDeliveries = s_dal.Delivery.ReadAll(filter).Where(d => d != null).ToList();
            if (!doDeliveries.Any())
                return Enumerable.Empty<BO.Delivery>();

            // 2. Get unique order IDs from the delivery list
            var orderIds = doDeliveries.Select(d => d!.OrderId).Distinct();

            // 3. Fetch all required orders in a single batch
            var doOrders = s_dal.Order.ReadAll(o => orderIds.Contains(o.Id))
                                      .ToDictionary(o => o.Id);

            // 4. Project to BO using the efficient private overload
            return doDeliveries.Select(d => 
            {
                if (doOrders.TryGetValue(d!.OrderId, out var order))
                {
                    return ConvertDoToBo(d, order);
                }
                // This case should not happen in a consistent database, but throw a clear exception if it does.
                throw new BO.BlDoesNotExistException($"Associated Order with ID {d.OrderId} not found for Delivery {d.Id}");
            });
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
            var doDelivery = s_dal.Delivery.Read(deliveryId) ?? 
                throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found.");
            return ConvertDoToBo(doDelivery);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found.", ex);
        }
    }

    public static BO.Delivery? GetDeliveryByCourier(int courierId)
    {
        // Find an active delivery for the courier (not yet delivered)
        var doDelivery = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndTime == null)
            .FirstOrDefault();

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
            // --- OPTIMIZED CHECKS ---

            // 1. Verify courier exists, is active, and is not already on a delivery.
            var doCourier = s_dal.Courier.Read(courierId)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.");
            if (!doCourier.Active)
                throw new BO.BlInvalidInputException($"Courier {courierId} is not active.");
            if (s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndTime == null).Any())
                throw new BO.BlCourierAlreadyHasDeliveryException(
                    $"Courier ID '{courierId}' already has an active delivery.");

            // 2. Verify order exists and is available for pickup.
            if (s_dal.Order.Read(orderId) == null)
                 throw new BO.BlDoesNotExistException($"Order with ID {orderId} not found.");
            
            var orderDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId).ToList();
            
            // Determine order status directly from data objects
            BO.OrderStatus status = OrderManager.DetermineOrderStatus(orderDeliveries);

            if (status == BO.OrderStatus.InProgress)
                throw new BO.BlOrderAlreadyAssignedException($"Order {orderId} cannot be picked up because it is in status {status}.");

            if (status != BO.OrderStatus.Open)
                throw new BO.BlDeliveryAlreadyClosedException($"Order {orderId} cannot be picked up because it is {status}.");

            // --- END OPTIMIZED CHECKS ---

            // Create a new delivery, which now starts immediately
            Create(orderId, courierId);
        }
        catch (Exception ex)
        {
                throw Tools.ConvertDalException(ex);
        }
    }

    public static void Deliver(int courierId, BO.DeliveryEndTypes endType)
    {
        // Validate courier exists - OPTIMIZED
        if (s_dal.Courier.Read(courierId) == null)
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.");
        
        // Find the active delivery for the courier using a filtered query - OPTIMIZED
        var delivery = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryStartTime <= AdminManager.Now && d.DeliveryEndTime == null)
            .FirstOrDefault();
        
        if(delivery == null)
            throw new BO.BlCourierHasNoActiveDeliveryException("Courier has no picked-up delivery to deliver.");

        // Update the delivery record
        delivery = delivery with { DeliveryEndTime = AdminManager.Now, DeliveryEndType = (DO.DeliveryEndTypes)endType };
        
        s_dal.Delivery.Update(delivery);
    }

    /// <summary>
    /// Calculates the actual distance for a delivery.
    /// </summary>
    /// <param name="delivery">The delivery for which to calculate the distance.</param>
    /// <returns>The calculated distance.</returns>
    private static double CalculateActualDistance(DO.Delivery delivery, DO.Order order)
    {
        var config = s_dal.Config;
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
        // For single conversions, fetch the required associated order.
        var order = s_dal.Order.Read(doDelivery.OrderId) ?? 
            throw new BO.BlDoesNotExistException($"Associated Order with ID {doDelivery.OrderId} not found for Delivery {doDelivery.Id}");
        
        // The existence check for the delivery itself is removed to prevent re-querying the database.
        // It's assumed the caller (`ReadDelivery`) has already validated this.
        return ConvertDoToBo(doDelivery, order);
    }

    /// <summary>
    /// Private worker for converting a DO Delivery to its BO equivalent, using a pre-fetched order.
    /// </summary>
    /// <param name="doDelivery">The DO Delivery to convert.</param>
    /// <param name="order">The pre-fetched associated DO Order.</param>
    /// <returns>An equivalent BO Delivery entity.</returns>
    private static BO.Delivery ConvertDoToBo(DO.Delivery doDelivery, DO.Order order)
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
                    ? Math.Round((double)boDelivery.ActualDistance / boDelivery.DeliveryDuration.TotalHours, 2)
                    : 0;
            }
            
            var config = s_dal.Config;
            
            boDelivery.MaximumDeliveryTime = order.OrderOpenTime + config.MaxDeliveryTimeSpan;

            boDelivery.ExpectedDeliveryTime = Tools.CalculateExpectedDeliveryTime(doDelivery.DeliveryType, order, config, doDelivery);

            boDelivery.ScheduleStatus = Tools.DetermineScheduleStatus(order, config, doDelivery);


            return boDelivery;

        }

        catch (Exception ex)

        {

            throw Tools.ConvertDalException(ex);

        }

    }




    /// <summary>

    /// Determines the schedule status of a delivery.

    /// </summary>

    /// <param name="delivery">The delivery to check.</param>

    /// <param name="riskRange">The time span defining the risk range.</param>

    /// <returns>The calculated schedule status.</returns>

    



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

                DeliveryEndType = boDelivery.DeliveryEndType,

                DeliveryEndTime = boDelivery.DeliveryEndTime

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
        // Read DOs directly for efficiency
        var doCourier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.");
        var doOrder = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException($"Order with ID {orderId} not found.");

        var config = s_dal.Config;

        // Re-implement IsOrderInCourierRange logic with DOs
        if (doCourier.PersonalMaxDeliveryDistance.HasValue)
        {
            var distanceToOrder = Tools.GetAerialDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), doOrder.Latitude, doOrder.Longitude);
            if (distanceToOrder > doCourier.PersonalMaxDeliveryDistance.Value)
            {
                throw new BO.BlInvalidInputException($"Order location is too far for this courier. a maximum of {doCourier.PersonalMaxDeliveryDistance}km is allowed");
            }
        }

        // Determine the delivery type based on the courier's vehicle
        DO.DeliveryTypes deliveryType = doCourier.DeliveryType;
        
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
        
        // Call the helper with the already-fetched DO.Order
        double distance = Math.Round(CalculateActualDistance(tempDelivery, doOrder), 2);

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
        var config = s_dal.Config;
        var riskThreshold = config.RiskRange / 2;
        var rnd = new Random();
        var endTypes = Enum.GetValues(typeof(BO.DeliveryEndTypes));

        var activeDeliveries = s_dal.Delivery.ReadAll(d => d.DeliveryEndTime == null);
        var orderIds = activeDeliveries.Select(d => d.OrderId).Distinct();
        var orders = s_dal.Order.ReadAll(o => orderIds.Contains(o.Id)).ToDictionary(o => o.Id);

        (from d in activeDeliveries
         let order = orders.GetValueOrDefault(d.OrderId)
         where order != null
         let expectedTime = Tools.CalculateExpectedDeliveryTime(d.DeliveryType, order!, config, d)
         where newClock >= expectedTime + riskThreshold
         select d)
         .ToList()
         .ForEach(d =>
         {
             var endType = (BO.DeliveryEndTypes)endTypes.GetValue(rnd.Next(endTypes.Length))!;
             var updatedDelivery = d with
             {
                 DeliveryEndTime = newClock,
                 DeliveryEndType = (DO.DeliveryEndTypes)endType
             };
             s_dal.Delivery.Update(updatedDelivery);
         });
    }
}
