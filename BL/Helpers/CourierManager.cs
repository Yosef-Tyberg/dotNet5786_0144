using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Helpers;

/// <summary>
/// Manager class for Courier entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class CourierManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Converts a business layer Courier entity to its data layer equivalent.
    /// </summary>
    /// <param name="boCourier">The BO Courier to convert.</param>
    /// <returns>An equivalent DO Courier entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static DO.Courier ConvertBoToDo(BO.Courier boCourier)
    {
        try
        {
            return new DO.Courier(
                Id: boCourier.Id,
                FullName: boCourier.FullName,
                MobilePhone: boCourier.MobilePhone,
                Email: boCourier.Email,
                Password: boCourier.Password,
                Active: boCourier.Active,
                DeliveryType: (DO.DeliveryTypes)boCourier.DeliveryType,
                EmploymentStartTime: boCourier.EmploymentStartTime,
                PersonalMaxDeliveryDistance: boCourier.PersonalMaxDeliveryDistance
            );
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert BO Courier to DO Courier");
        }
    }

    /// <summary>
    /// Converts a data layer Courier entity to its business layer equivalent.
    /// </summary>
    /// <param name="doCourier">The DO Courier to convert.</param>
    /// <returns>An equivalent BO Courier entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.Courier ConvertDoToBo(DO.Courier doCourier)
    {
        try
        {
            return new BO.Courier
            {
                Id = doCourier.Id,
                FullName = doCourier.FullName,
                MobilePhone = doCourier.MobilePhone,
                Email = doCourier.Email,
                Password = doCourier.Password,
                Active = doCourier.Active,
                DeliveryType = (BO.DeliveryTypes)doCourier.DeliveryType,
                EmploymentStartTime = doCourier.EmploymentStartTime,
                PersonalMaxDeliveryDistance = doCourier.PersonalMaxDeliveryDistance
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert DO Courier to BO Courier");
        }
    }

    /// <summary>
    /// Converts a business layer Courier entity to a summary list view.
    /// Summary entities are derived from full BO entities (not directly from DO)
    /// to ensure calculated properties and validations are preserved.
    /// </summary>
    /// <param name="boCourier">The BO Courier to convert.</param>
    /// <returns>A CourierInList summary entity.</returns>
    /// <exception cref="BO.BlInvalidInputException">Thrown when conversion fails.</exception>
    public static BO.CourierInList ConvertBoToCourierInList(BO.Courier boCourier)
    {
        try
        {
            return new BO.CourierInList
            {
                Id = boCourier.Id,
                FullName = boCourier.FullName,
                DeliveryType = boCourier.DeliveryType,
                Active = boCourier.Active
            };
        }
        catch (Exception ex)
        {
            throw Tools.ConvertDalException(ex, "Convert BO Courier to CourierInList");
        }
    }
}
