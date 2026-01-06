using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Helpers;

/// <summary>
/// Manager class for Delivery entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class DeliveryManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

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
            throw Tools.ConvertDalException(ex, "Convert BO Delivery to DO Delivery");
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
            throw Tools.ConvertDalException(ex, "Convert DO Delivery to BO Delivery");
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
            throw Tools.ConvertDalException(ex, "Convert BO Delivery to DeliveryInList");
        }
    }
}
