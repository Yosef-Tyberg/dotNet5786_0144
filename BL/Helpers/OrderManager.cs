using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;

/// <summary>
/// Manager class for Order entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class OrderManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Validates a business layer Order entity.
    /// </summary>
    /// <param name="boOrder">The BO Order to validate.</param>
    public static void OrderValidation(BO.Order boOrder)
    {
        if (boOrder == null)
            throw new BO.BlInvalidNullInputException("Order object cannot be null.");

        if (boOrder.Id < 0)
            throw new BO.BlInvalidIdException($"Order ID '{boOrder.Id}' cannot be negative.");

        if (string.IsNullOrWhiteSpace(boOrder.CustomerFullName))
            throw new BO.BlInvalidInputException("Order customer full name cannot be empty.");

        if (string.IsNullOrWhiteSpace(boOrder.CustomerMobile))
            throw new BO.BlInvalidInputException("Order customer mobile cannot be empty.");

        if (boOrder.Latitude < -90 || boOrder.Latitude > 90)
            throw new BO.BlInvalidInputException($"Latitude {boOrder.Latitude} is out of range.");

        if (boOrder.Longitude < -180 || boOrder.Longitude > 180)
            throw new BO.BlInvalidInputException($"Longitude {boOrder.Longitude} is out of range.");

        if (boOrder.Volume <= 0)
            throw new BO.BlInvalidInputException($"Volume {boOrder.Volume} must be positive.");

        if (boOrder.Weight <= 0)
            throw new BO.BlInvalidInputException($"Weight {boOrder.Weight} must be positive.");

        if (boOrder.Height <= 0)
            throw new BO.BlInvalidInputException($"Height {boOrder.Height} must be positive.");

        if (boOrder.Width <= 0)
            throw new BO.BlInvalidInputException($"Width {boOrder.Width} must be positive.");

        if (string.IsNullOrWhiteSpace(boOrder.FullOrderAddress))
            throw new BO.BlInvalidAddressException("Order address cannot be empty.");

        Tools.GetCoordinates(boOrder.FullOrderAddress);

        var config = AdminManager.GetConfig();
        var distance = Tools.GetAerialDistance((double)config.Latitude, (double)config.Longitude, boOrder.Latitude, boOrder.Longitude);
        if (distance > config.MaxGeneralDeliveryDistanceKm)
            throw new BO.BlInvalidInputException($"Order location is too far from the company's location. a maximum of {config.MaxGeneralDeliveryDistanceKm}km is allowed");
    }

    /// <summary>
    /// Converts a business layer Order entity to its data layer equivalent.
    /// </summary>
    /// <param name="boOrder">The BO Order to convert.</param>
    /// <returns>An equivalent DO Order entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static DO.Order ConvertBoToDo(BO.Order boOrder)
    {
        try
        {
            return new DO.Order(
                Id: boOrder.Id,
                OrderType: (DO.OrderTypes)boOrder.OrderType,
                VerbalDescription: boOrder.VerbalDescription,
                Latitude: boOrder.Latitude,
                Longitude: boOrder.Longitude,
                CustomerFullName: boOrder.CustomerFullName,
                CustomerMobile: boOrder.CustomerMobile,
                Volume: boOrder.Volume,
                Weight: boOrder.Weight,
                Fragile: boOrder.Fragile,
                Height: boOrder.Height,
                Width: boOrder.Width,
                OrderOpenTime: boOrder.OrderOpenTime,
                FullOrderAddress: boOrder.FullOrderAddress
            );
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Converts a data layer Order entity to its business layer equivalent.
    /// </summary>
    /// <param name="doOrder">The DO Order to convert.</param>
    /// <returns>An equivalent BO Order entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.Order ConvertDoToBo(DO.Order doOrder)
    {
        try
        {
            var boOrder = new BO.Order
            {
                Id = doOrder.Id,
                OrderType = (BO.OrderTypes)doOrder.OrderType,
                VerbalDescription = doOrder.VerbalDescription,
                Latitude = doOrder.Latitude,
                Longitude = doOrder.Longitude,
                CustomerFullName = doOrder.CustomerFullName,
                CustomerMobile = doOrder.CustomerMobile,
                Volume = doOrder.Volume,
                Weight = doOrder.Weight,
                Fragile = doOrder.Fragile,
                Height = doOrder.Height,
                Width = doOrder.Width,
                OrderOpenTime = doOrder.OrderOpenTime,
                FullOrderAddress = doOrder.FullOrderAddress,
                TimeOpenedDuration = AdminManager.Now - doOrder.OrderOpenTime,
                PackageDensity = doOrder.Volume > 0 ? doOrder.Weight / doOrder.Volume : 0
            };

            var deliveries = DeliveryManager.ReadAll(d => d.OrderId == doOrder.Id);
            boOrder.OrderStatus = DetermineOrderStatus(deliveries);
            
            return boOrder;
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Determines the status of an order based on its delivery history.
    /// </summary>
    /// <param name="deliveries">The list of deliveries associated with the order.</param>
    /// <returns>The calculated order status.</returns>
    private static BO.OrderStatus DetermineOrderStatus(IEnumerable<BO.Delivery> deliveries)
    {
        if (!deliveries.Any())
        {
            return BO.OrderStatus.Open;
        }

        if (deliveries.Any(d => d.DeliveryEndTime == null))
        {
            return BO.OrderStatus.InProgress;
        }

        var lastEnded = deliveries.OrderByDescending(d => d.DeliveryEndTime).First();
        return lastEnded.DeliveryEndType switch
        {
            BO.DeliveryEndTypes.Delivered => BO.OrderStatus.Delivered,
            BO.DeliveryEndTypes.CustomerRefused => BO.OrderStatus.Refused,
            BO.DeliveryEndTypes.Cancelled => BO.OrderStatus.Cancelled,
            BO.DeliveryEndTypes.RecipientNotFound => BO.OrderStatus.Open,
            BO.DeliveryEndTypes.Failed => BO.OrderStatus.Open,
            _ => BO.OrderStatus.Open
        };
    }

    /// <summary>
    /// Converts a business layer Order entity to a summary list view.
    /// Summary entities are derived from full BO entities (not directly from DO)
    /// to ensure calculated properties and validations are preserved.
    /// </summary>
    /// <param name="boOrder">The BO Order to convert.</param>
    /// <returns>An OrderInList summary entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.OrderInList ConvertBoToOrderInList(BO.Order boOrder)
    {
        try
        {
            return new BO.OrderInList
            {
                Id = boOrder.Id,
                OrderType = boOrder.OrderType,
                CustomerFullName = boOrder.CustomerFullName,
                OrderStatus = boOrder.OrderStatus,
                OrderOpenTime = boOrder.OrderOpenTime
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Retrieves all orders from the data layer and converts them to a list of BO Orders.
    /// </summary>
    /// <param name="filter">An optional filter to apply to the orders.</param>
    /// <returns>An IEnumerable of BO.Order.</returns>
    public static IEnumerable<BO.Order> ReadAll(Func<DO.Order, bool>? filter = null)
    {
        try
        {
            return s_dal.Order.ReadAll(filter).Select(ConvertDoToBo);
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Retrieves a single order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>A BO.Order object.</returns>
    public static BO.Order Read(int orderId)
    {
        try
        {
            return ConvertDoToBo(s_dal.Order.Read(orderId));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="newOrder">The BO.Order object for the new order.</param>
    public static void Create(BO.Order newOrder)
    {
        OrderValidation(newOrder);
        try
        {
            s_dal.Order.Create(ConvertBoToDo(newOrder));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="updatedOrder">A BO.Order object with the new details.</param>
    public static void Update(BO.Order updatedOrder)
    {
        OrderValidation(updatedOrder);
        try
        {
            // Verify that the order has not been assigned to a courier
            if (DeliveryManager.IsOrderTaken(updatedOrder.Id))
                throw new BO.BlOrderAlreadyAssignedException(
                    $"Order ID '{updatedOrder.Id}' cannot be updated as it is already assigned to a courier.");
            s_dal.Order.Update(ConvertBoToDo(updatedOrder));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Deletes an order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to delete.</param>
    public static void Delete(int orderId)
    {
        try
        {
            // Verify that the order has not been assigned to a courier
            if (DeliveryManager.IsOrderTaken(orderId))
                throw new BO.BlOrderAlreadyAssignedException(
                    $"Order ID '{orderId}' cannot be deleted as it is already assigned to a courier.");
            s_dal.Order.Delete(orderId);
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Retrieves a list of orders available for pickup, tailored to the courier's capabilities.
    /// </summary>
    /// <param name="courierId">The ID of the courier looking for orders.</param>
    /// <returns>An IEnumerable of BO.Order that the courier can deliver.</returns>
    public static IEnumerable<BO.Order> GetAvailableOrders(int courierId)
    {
        try
        {
            // The courier must exist to get available orders.
            BO.Courier boCourier = CourierManager.ReadCourier(courierId);

            return ReadAll(order => !DeliveryManager.IsOrderTaken(order.Id));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    

    /// <summary>
    /// gets the tracking information for an order
    /// </summary>
    /// <param name="orderId">the id of the order to get the tracking information for</param>
    /// <returns>the tracking information for the order</returns>
    public static BO.OrderTracking GetOrderTracking(int orderId)
    {
        DO.Order doOrder;
        try
        {
            doOrder = s_dal.Order.Read(orderId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID {orderId} not found.", ex);
        }

        var deliveries = DeliveryManager.ReadAll(d => d.OrderId == orderId);
        BO.OrderStatus status = DetermineOrderStatus(deliveries);
        BO.Delivery? activeDelivery = deliveries.FirstOrDefault(d => d.DeliveryEndTime == null);

        // Get Assigned Courier (only if InProgress)
        BO.CourierInList? assignedCourier = null;
        if (status == BO.OrderStatus.InProgress && activeDelivery != null)
        {
            try
            {
                var boCourier = CourierManager.ReadCourier(activeDelivery.CourierId);
                assignedCourier = CourierManager.ConvertBoToCourierInList(boCourier);
            }
            catch (DO.DalDoesNotExistException) { /* Courier might have been deleted, ignore */ }
        }

        // Map Delivery History
        IEnumerable<BO.DeliveryInList> history = deliveries
            .OrderBy(d => d.DeliveryStartTime)
            .Select(DeliveryManager.ConvertBoToDeliveryInList);

        // Calculate Timing Properties
        var config = AdminManager.GetConfig();
        DateTime? maxDeliveryTime = doOrder.OrderOpenTime.Add(config.MaxDeliveryTimeSpan);
        DateTime? expectedDeliveryTime = CalculateExpectedDeliveryTime(status, activeDelivery, assignedCourier, doOrder, config);

        return new BO.OrderTracking
        {
            Id = doOrder.Id,
            Status = status,
            AssignedCourier = assignedCourier,
            DeliveryHistory = history,
            VerbalDescription = doOrder.VerbalDescription,
            CustomerFullName = doOrder.CustomerFullName,
            OrderOpenTime = doOrder.OrderOpenTime,
            ExpectedDeliveryTime = expectedDeliveryTime,
            MaxDeliveryTime = maxDeliveryTime,
            FullOrderAddress = doOrder.FullOrderAddress
        };
    }

    /// <summary>
    /// Calculates the expected delivery time for an order that is in progress.
    /// </summary>
    /// <param name="status">The current status of the order.</param>
    /// <param name="activeDelivery">The active delivery for the order.</param>
    /// <param name="assignedCourier">The courier assigned to the delivery.</param>
    /// <param name="doOrder">The data layer order object.</param>
    /// <param name="config">The application configuration.</param>
    /// <returns>The calculated expected delivery time, or null if not applicable.</returns>
    private static DateTime? CalculateExpectedDeliveryTime(BO.OrderStatus status, BO.Delivery? activeDelivery, BO.CourierInList? assignedCourier, DO.Order doOrder, BO.Config config)
    {
        if (status != BO.OrderStatus.InProgress || activeDelivery == null || assignedCourier == null)
        {
            return null;
        }

        double distance;
        switch (assignedCourier.DeliveryType)
        {
            case BO.DeliveryTypes.Car:
            case BO.DeliveryTypes.Motorcycle:
                distance = Tools.GetDrivingDistance((double)config.Latitude, (double)config.Longitude, doOrder.Latitude, doOrder.Longitude);
                break;
            case BO.DeliveryTypes.Bicycle:
            case BO.DeliveryTypes.OnFoot:
                distance = Tools.GetWalkingDistance((double)config.Latitude, (double)config.Longitude, doOrder.Latitude, doOrder.Longitude);
                break;
            default:
                throw new BO.BlMissingPropertyException($"Invalid or missing delivery type for courier {assignedCourier.Id}");
        }

        double speed = assignedCourier.DeliveryType switch
        {
            BO.DeliveryTypes.Car => config.AvgCarSpeedKmh,
            BO.DeliveryTypes.Motorcycle => config.AvgMotorcycleSpeedKmh,
            BO.DeliveryTypes.Bicycle => config.AvgBicycleSpeedKmh,
            BO.DeliveryTypes.OnFoot => config.AvgWalkingSpeedKmh,
            _ => config.AvgCarSpeedKmh
        };

        if (speed > 0)
        {
            double travelHours = distance / speed;
            return activeDelivery.DeliveryStartTime.AddHours(travelHours);
        }

        return null;
    }

    /// <summary>
    /// a method for periodic updates of the order
    /// </summary>
    /// <param name="oldClock"></param>
    /// <param name="newClock"></param>
    public static void PeriodicOrdersUpdate(DateTime oldClock, DateTime newClock)
    {
        //implementation will be added in the future
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel.</param>
    public static void Cancel(int orderId)
    {
        try
        {
            BO.Order order = Read(orderId);

            switch (order.OrderStatus)
            {
                case BO.OrderStatus.InProgress:
                    // Find the active delivery
                    var activeDelivery = s_dal.Delivery.Read(d => d.OrderId == orderId && d.DeliveryEndTime == null);
                    if (activeDelivery != null)
                    {
                        // Cancel the delivery
                        activeDelivery = activeDelivery with
                        {
                            DeliveryEndTime = AdminManager.Now,
                            DeliveryEndType = DO.DeliveryEndTypes.Cancelled
                        };
                        s_dal.Delivery.Update(activeDelivery);
                    }
                    break;

                case BO.OrderStatus.Open:
                    // Create a dummy delivery
                    DeliveryManager.CreateCancelledDelivery(orderId);
                    break;

                case BO.OrderStatus.Delivered:
                case BO.OrderStatus.Cancelled:
                case BO.OrderStatus.Refused:
                    throw new BO.BlOrderCannotBeCancelledException(
                        $"Order ID '{orderId}' cannot be cancelled as it is already in status {order.OrderStatus}.");
            }
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
}
