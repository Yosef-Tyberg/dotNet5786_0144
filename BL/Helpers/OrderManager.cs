using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Helpers;

/// <summary>
/// Manager class for Order entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class OrderManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

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
            return new BO.Order
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
                OrderOpenTime = doOrder.OrderOpenTime
            };
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
}
