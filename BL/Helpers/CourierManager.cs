using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Helpers;

/// <summary>
/// Manager class for Courier entity operations, providing conversion methods
/// between data layer (DO) and business layer (BO) representations.
/// </summary>
internal static class CourierManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Validates a business layer Courier entity.
    /// </summary>
    /// <param name="boCourier">The BO Courier to validate.</param>
    /// <exception cref="BO.BlInvalidNullInputException">Thrown if the courier is null.</exception>
    /// <exception cref="BO.BlInvalidIdException">Thrown if the courier ID is invalid.</exception>
    /// <exception cref="BO.BlInvalidInputException">Thrown if required fields are empty.</exception>
    /// <exception cref="BO.BlInvalidEmailException">Thrown if the email format is invalid.</exception>
    public static void CourierValidation(BO.Courier boCourier)
    {
        if (boCourier == null)
            throw new BO.BlInvalidNullInputException("Courier object cannot be null.");
        //0 is for dummy deliveries
        if ((boCourier.Id < 100000000 || boCourier.Id > 999999999) && boCourier.Id != 0)
            throw new BO.BlInvalidIdException($"Courier ID '{boCourier.Id}' is not valid. It must be a 9-digit number.");

        if (string.IsNullOrWhiteSpace(boCourier.FullName))
            throw new BO.BlInvalidInputException("Courier full name cannot be empty.");

        if (string.IsNullOrWhiteSpace(boCourier.Password))
            throw new BO.BlInvalidInputException("Courier password cannot be empty.");

        if (!string.IsNullOrWhiteSpace(boCourier.Email) && !Regex.IsMatch(boCourier.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BO.BlInvalidEmailException($"Courier email '{boCourier.Email}' is not a valid format.");
    }

    public static BO.Courier ReadCourier(int courierId)
    {
        try
        {
            DO.Courier doCourier = s_dal.Courier.Read(courierId);
            BO.Courier boCourier = ConvertDoToBo(doCourier);

            boCourier.EmploymentDuration = AdminManager.Now - boCourier.EmploymentStartTime;
            boCourier.YearsEmployed = boCourier.EmploymentDuration.TotalDays / 365.25;

            return boCourier;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.", ex);
        }
    }

    public static IEnumerable<BO.CourierInList> ReadAllCouriersForList(Func<BO.CourierInList, bool>? filter = null)
    {
        var doCouriers = s_dal.Courier.ReadAll();
        var boCouriers = doCouriers.Select(doCourier => ReadCourier(doCourier.Id));
        var courierInList = boCouriers.Select(ConvertBoToCourierInList);

        return filter == null ? courierInList : courierInList.Where(filter);
    }

    public static void CreateCourier(BO.Courier courier)
    {
        CourierValidation(courier);
        try
        {
            s_dal.Courier.Create(ConvertBoToDo(courier));
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Courier with ID {courier.Id} already exists.", ex);
        }
    }

    public static void DeleteCourier(int courierId)
    {
        try
        {
            // We might want to check if the courier has active deliveries before deleting
            s_dal.Courier.Delete(courierId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.", ex);
        }
    }

    public static void UpdateCourier(BO.Courier courier)
    {
        CourierValidation(courier);
        try
        {
            s_dal.Courier.Read(courier.Id); // Check for existence
            s_dal.Courier.Update(ConvertBoToDo(courier));
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courier.Id} not found.", ex);
        }
    }

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
            throw Tools.ConvertDalException(ex);
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
            throw Tools.ConvertDalException(ex);
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
            throw Tools.ConvertDalException(ex);
        }
    }


    public static void PeriodicCouriersUpdate(DateTime oldClock, DateTime newClock)
    {
        // The logic for this method is currently on hold per user request.
    }
}
