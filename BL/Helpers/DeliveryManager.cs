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

    public static IEnumerable<BO.Delivery> ReadAll(Func<DO.Delivery, bool>? filter = null)
    {
        try
        {
            return s_dal.Delivery.ReadAll(filter).Select(ConvertDoToBo);
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

    public static BO.Delivery GetMyCurrentDelivery(int courierId)
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
            OrderManager.Read(orderId);

            // Verify the order is not already taken
            if (IsOrderTaken(orderId))
                throw new BO.BlOrderAlreadyAssignedException(
                    $"Order ID '{orderId}' is already assigned and cannot be taken again.");
            
            // Verify courier doesn't have an active delivery
            if (s_dal.Delivery.ReadAll().Any(d => d.CourierId == courierId && d.DeliveryEndTime == null))
                throw new BO.BlCourierAlreadyHasDeliveryException(
                    $"Courier ID '{courierId}' already has an active delivery.");

            // Create a new delivery, which now starts immediately
            Create(orderId, courierId);
        }
        catch (Exception ex)
        {
            if (ex is BO.BlOrderAlreadyAssignedException || ex is BO.BlCourierAlreadyHasDeliveryException or BO.BlDoesNotExistException)
                throw;
            
            throw Tools.ConvertDalException(ex);
        }
    }

    public static void Deliver(int courierId, BO.DeliveryEndTypes endType)
    {
        var delivery = s_dal.Delivery.ReadAll().FirstOrDefault(d => d.CourierId == courierId && d.DeliveryStartTime < AdminManager.Now && d.DeliveryEndTime == null);
        if(delivery == null)
            throw new BO.BlCourierHasNoActiveDeliveryException("Courier has no picked-up delivery to deliver.");

        double? distance = delivery.ActualDistance; // Preserve existing distance if any
        if (endType == BO.DeliveryEndTypes.Delivered)
        {
            var order = OrderManager.Read(delivery.OrderId);
            var config = AdminManager.GetConfig();
            switch ((BO.DeliveryTypes)delivery.DeliveryType)
            {
                case BO.DeliveryTypes.Car:
                case BO.DeliveryTypes.Motorcycle:
                    distance = Tools.GetDrivingDistance((double)config.Latitude!, (double)config.Longitude!, order.Latitude, order.Longitude);
                    break;
                case BO.DeliveryTypes.Bicycle:
                case BO.DeliveryTypes.OnFoot:
                    distance = Tools.GetWalkingDistance((double)config.Latitude, (double)config.Longitude, order.Latitude, order.Longitude);
                    break;
                default:
                    throw new BO.BlMissingPropertyException($"Could not calculate properties for unrecognized delivery type: {(BO.DeliveryTypes)delivery.DeliveryType}");
            }
        }

        delivery = delivery with { DeliveryEndTime = AdminManager.Now, DeliveryEndType = (DO.DeliveryEndTypes)endType, ActualDistance = distance };
        
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
            
                        double distance;
                        double speed;
                        switch (boDelivery.DeliveryType)
                        {
                            case BO.DeliveryTypes.Car:
                                distance = Tools.GetDrivingDistance((double)config.Latitude!, (double)config.Longitude!, order.Latitude, order.Longitude);
                                speed = config.AvgCarSpeedKmh;
                                break;
                            case BO.DeliveryTypes.Motorcycle:
                                distance = Tools.GetDrivingDistance((double)config.Latitude!, (double)config.Longitude!, order.Latitude, order.Longitude);
                                speed = config.AvgMotorcycleSpeedKmh;
                                break;
                            case BO.DeliveryTypes.Bicycle:
                                distance = Tools.GetWalkingDistance((double)config.Latitude, (double)config.Longitude, order.Latitude, order.Longitude);
                                speed = config.AvgBicycleSpeedKmh;
                                break;
                            case BO.DeliveryTypes.OnFoot:
                                distance = Tools.GetWalkingDistance((double)config.Latitude, (double)config.Longitude, order.Latitude, order.Longitude);
                                speed = config.AvgWalkingSpeedKmh;
                                break;
                            default:
                                throw new BO.BlMissingPropertyException($"Could not calculate properties for unrecognized delivery type: {boDelivery.DeliveryType}");
                        }
            
                        if (speed > 0)
                        {
                            var estimatedHours = distance / speed;
                            boDelivery.ExpectedDeliveryTime = boDelivery.DeliveryStartTime.AddHours(estimatedHours);
                        }
                        else
                        {
                            boDelivery.ExpectedDeliveryTime = boDelivery.MaximumDeliveryTime;
                        }
            
                        if ((boDelivery.DeliveryEndTime == null && AdminManager.Now > boDelivery.MaximumDeliveryTime) || (boDelivery.DeliveryEndTime != null && boDelivery.DeliveryEndTime > boDelivery.MaximumDeliveryTime))
                        {
                            boDelivery.ScheduleStatus = BO.ScheduleStatus.Late;
                        }
                        else if (boDelivery.DeliveryEndTime == null && (boDelivery.MaximumDeliveryTime - AdminManager.Now) < config.RiskRange)
                        {
                            boDelivery.ScheduleStatus = BO.ScheduleStatus.AtRisk;
                        }
                        else
                        {
                            boDelivery.ScheduleStatus = BO.ScheduleStatus.OnTime;
                        }
            
                        return boDelivery;
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

        if (courier.PersonalMaxDeliveryDistance.HasValue)
        {
            var order = OrderManager.Read(orderId);
            var config = AdminManager.GetConfig();
            var distance = Tools.GetAerialDistance((double)config.Latitude, (double)config.Longitude, order.Latitude, order.Longitude);
            if (distance > courier.PersonalMaxDeliveryDistance)
                throw new BO.BlInvalidInputException($"Order location is too far for this courier. a maximum of {courier.PersonalMaxDeliveryDistance}km is allowed");
        }

        // Determine the delivery type based on the courier's vehicle
        DO.DeliveryTypes deliveryType = (DO.DeliveryTypes)courier.DeliveryType;
        
        var newDelivery = new DO.Delivery(
            Id: 0, 
            OrderId: orderId,
            CourierId: courierId,
            DeliveryType: deliveryType,
            DeliveryStartTime: AdminManager.Now,
            ActualDistance: null,
            DeliveryEndType: null,
            DeliveryEndTime: null
        );

        s_dal.Delivery.Create(newDelivery);
    }
}

