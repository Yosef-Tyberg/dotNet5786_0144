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
                OrderOpenTime: boOrder.OrderOpenTime
            );
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert BO Order to DO Order");
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
                TimeOpenedDuration = AdminManager.Now - doOrder.OrderOpenTime,
                PackageDensity = doOrder.Volume > 0 ? doOrder.Weight / doOrder.Volume : 0
            };

            var lastDelivery = s_dal.Delivery.ReadAll().Where(d => d.OrderId == doOrder.Id)
                .OrderByDescending(d => d.DeliveryEndTime).FirstOrDefault();

            if (lastDelivery == null)
            {
                boOrder.OrderStatus = BO.OrderStatus.Open;
            }
            else if (lastDelivery.DeliveryEndTime == null)
            {
                boOrder.OrderStatus = BO.OrderStatus.InProgress;
            }
            else
            {
                boOrder.OrderStatus = lastDelivery.DeliveryEndType switch
                {
                    DO.DeliveryEndTypes.Delivered => BO.OrderStatus.Delivered,
                    DO.DeliveryEndTypes.CustomerRefused => BO.OrderStatus.Refused,
                    DO.DeliveryEndTypes.Cancelled => BO.OrderStatus.Cancelled,
                    _ => BO.OrderStatus.InProgress
                };
            }
            
            return boOrder;
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert DO Order to BO Order");
        }
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
                OrderOpenTime = boOrder.OrderOpenTime
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert BO Order to OrderInList");
        }
    }
    
    /// <summary>
    /// Retrieves all orders from the data layer and converts them to a list of BO Orders.
    /// </summary>
    /// <param name="filter">An optional filter to apply to the orders.</param>
    /// <returns>An IEnumerable of BO.OrderInList.</returns>
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
    /// <returns>The ID of the newly created order.</returns>
    public static int Create(BO.Order newOrder)
    {
        OrderValidation(newOrder);
        try
        {
            return s_dal.Order.Create(ConvertBoToDo(newOrder));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="updatedOrder">A BO.Order object with the new details.</param>
    public static void Update(int orderId, BO.Order updatedOrder)
    {
        OrderValidation(updatedOrder);
        try
        {
            // Verify that the order has not been assigned to a courier
            if (DeliveryManager.IsOrderTaken(orderId))
                throw new BO.BlCannotUpdateException(
                    $"Order ID '{orderId}' cannot be updated as it is already assigned to a courier.");
            s_dal.Order.Update(orderId, ConvertBoToDo(updatedOrder));
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
    /// <returns>An IEnumerable of BO.OrderInList that the courier can deliver.</returns>
    public static IEnumerable<BO.Order> GetAvailableOrders(int courierId)
    {
        try
        {
            // The courier must exist to get available orders.
            BO.Courier boCourier = CourierManager.Read(courierId);

            return ReadAll(order => !DeliveryManager.IsOrderTaken(order.Id));
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
    
    /// <summary>
    /// Allows a courier to take/assign an order for delivery.
    /// This will create a new Delivery entity.
    /// </summary>
    /// <param name="orderId">The ID of the order to take.</param>
    /// <param name="courierId">The ID of the courier taking the order.</param>
    public static void TakeOrder(int orderId, int courierId)
    {
        try
        {
            // Verify that the order exists and is not already taken
            Read(orderId);
            if (DeliveryManager.IsOrderTaken(orderId))
                throw new BO.BlCannotUpdateException(
                    $"Order ID '{orderId}' is already assigned and cannot be taken again.");
            
            // Create a new delivery
            DeliveryManager.Create(orderId, courierId);
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex);
        }
    }
}
