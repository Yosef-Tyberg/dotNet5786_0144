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

    /// <summary>
    /// Retrieves a single courier and calculates their employment duration.
    /// </summary>
    /// <param name="courierId">The ID of the courier to retrieve.</param>
    /// <returns>A BO.Courier object with calculated properties.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the courier is not found.</exception>
    public static BO.Courier ReadCourier(int courierId)
    {
        try
        {
            DO.Courier doCourier = s_dal.Courier.Read(courierId);
            return ConvertDoToBo(doCourier);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.", ex);
        }
    }

    /// <summary>
    /// Retrieves a list of all couriers, converted to full business objects, with optional filtering.
    /// </summary>
    /// <param name="filter">An optional predicate to filter the BO.Courier objects.</param>
    /// <returns>An IEnumerable of BO.Courier.</returns>
    public static IEnumerable<BO.Courier> ReadAll(Func<BO.Courier, bool>? filter = null)
    {
        var boCouriers = s_dal.Courier.ReadAll().Select(ConvertDoToBo);
        return filter == null ? boCouriers : boCouriers.Where(filter);
    }

    /// <summary>
    /// Creates a new courier in the data source.
    /// </summary>
    /// <param name="courier">The courier to create.</param>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown if a courier with the same ID already exists.</exception>
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

    /// <summary>
    /// Deletes a courier from the data source.
    /// </summary>
    /// <param name="courierId">The ID of the courier to delete.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the courier is not found.</exception>
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

    /// <summary>
    /// Updates an existing courier's details.
    /// </summary>
    /// <param name="courier">The courier with updated details.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the courier is not found.</exception>
    public static void UpdateCourier(BO.Courier courier)
    {
        CourierValidation(courier);
        try
        {
            s_dal.Courier.Update(ConvertBoToDo(courier));
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courier.Id} not found.", ex);
        }
    }

    /// <summary>
    /// Updates a courier's own details.
    /// </summary>
    /// <param name="courierId">The ID of the courier to update.</param>
    /// <param name="fullName">Optional new full name.</param>
    /// <param name="phoneNum">Optional new phone number.</param>
    /// <param name="email">Optional new email.</param>
    /// <param name="password">Optional new password.</param>
    /// <param name="maxDistance">Optional new max delivery distance.</param>
    public static void UpdateMyDetails(int courierId, string? fullName = null, string? phoneNum = null, string? email = null, string? password = null, double? maxDistance = null)
    {
        try
        {
            // Read the courier once. This gives us a BO object.
            var courier = ReadCourier(courierId);

            // Update properties using the ?? operator for conciseness
            courier.FullName = fullName ?? courier.FullName;
            courier.MobilePhone = phoneNum ?? courier.MobilePhone;
            courier.Email = email ?? courier.Email;
            courier.Password = password ?? courier.Password;
            courier.PersonalMaxDeliveryDistance = maxDistance ?? courier.PersonalMaxDeliveryDistance;

            // Since we modified the BO object, we should re-validate it.
            CourierValidation(courier);
            
            // Now, call the DAL update directly.
            s_dal.Courier.Update(ConvertBoToDo(courier));
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // The exception could come from ReadCourier or the s_dal.Courier.Update call.
            // The message is appropriate for both cases.
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} not found.", ex);
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
            var boCourier = new BO.Courier
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

            boCourier.EmploymentDuration = AdminManager.Now - boCourier.EmploymentStartTime;
            boCourier.YearsEmployed = boCourier.EmploymentDuration.TotalDays / 365.25;
            
            return boCourier;
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

    /// <summary>
    /// Retrieves the delivery history for a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>An IEnumerable of the courier's deliveries.</returns>
    public static IEnumerable<BO.DeliveryInList> GetCourierDeliveryHistory(int courierId)
    {
        ReadCourier(courierId); // Validate courier exists
        var boDeliveries = DeliveryManager.ReadAll(d => d.CourierId == courierId);
        return boDeliveries.Select(DeliveryManager.ConvertBoToDeliveryInList);
    }
    
    /// <summary>
    /// Retrieves all open orders that are within a courier's maximum delivery distance.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>An IEnumerable of open orders available to the courier.</returns>
    public static IEnumerable<BO.OrderInList> GetOpenOrders(int courierId)
    {
        var courier = ReadCourier(courierId);
        var openOrders = OrderManager.ReadAll().Where(o => o.OrderStatus == BO.OrderStatus.Open);

        var config = AdminManager.GetConfig();
        var hqLatitude = (double)config.Latitude;
        var hqLongitude = (double)config.Longitude;

        return openOrders
            .Where(order => IsOrderInCourierRange(order, courier, hqLatitude, hqLongitude))
            .Select(OrderManager.ConvertBoToOrderInList);
    }

    /// <summary>
    /// Determines if an order is within a courier's maximum delivery range.
    /// </summary>
    /// <param name="order">The order to check.</param>
    /// <param name="courier">The courier.</param>
    /// <param name="hqLatitude">The latitude of the headquarters.</param>
    /// <param name="hqLongitude">The longitude of the headquarters.</param>
    /// <returns>True if the order is in range, false otherwise.</returns>
    internal static bool IsOrderInCourierRange(BO.Order order, BO.Courier courier, double hqLatitude, double hqLongitude)
    {
        if (!courier.PersonalMaxDeliveryDistance.HasValue)
        {
            return false;
        }
        var distance = Tools.GetAerialDistance(hqLatitude, hqLongitude, order.Latitude, order.Longitude);
        return distance <= courier.PersonalMaxDeliveryDistance.Value;
    }

    /// <summary>
    /// Calculates and retrieves statistics for a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A BO.CourierStatistics object.</returns>
    public static BO.CourierStatistics GetCourierStatistics(int courierId)
    {
        var deliveries = GetCourierDeliveryHistory(courierId);
        if (!deliveries.Any())
        {
            return new BO.CourierStatistics(); // Return empty stats if no deliveries
        }

        var completedDeliveries = deliveries
            .Where(d => d.ScheduleStatus is BO.ScheduleStatus.OnTime or BO.ScheduleStatus.Late)
            .Select(d => DeliveryManager.ReadDelivery(d.Id)) // Read full delivery for details
            .ToList();

        var totalDeliveries = completedDeliveries.Count;
        var onTimeDeliveries = completedDeliveries.Count(d => d.ScheduleStatus == BO.ScheduleStatus.OnTime);
        var successfulDeliveries = completedDeliveries.Count(d => d.DeliveryEndType == BO.DeliveryEndTypes.Delivered);

        return new BO.CourierStatistics
        {
            TotalDeliveries = totalDeliveries,
            TotalDistance = completedDeliveries.Sum(d => (double)d.ActualDistance),
            AverageDeliveryTime = TimeSpan.FromMinutes(completedDeliveries.Average(d => ((TimeSpan)(d.DeliveryEndTime - d.DeliveryStartTime)).TotalMinutes)),
            OnTimeDeliveries = onTimeDeliveries,
            LateDeliveries = totalDeliveries - onTimeDeliveries,
            SuccessRate = totalDeliveries > 0 ? (double)successfulDeliveries / totalDeliveries * 100 : 0
        };
    }

    /// <summary>
    /// a method for periodic updates of the courier.
    /// An active courier is deactivated if they have been inactive for a period greater
    /// than Config.InactivityRange. Inactivity is measured from the end of their last
    /// delivery, or from their employment start time if they have no delivery history.
    /// Couriers with a delivery in progress are always considered active.
    /// </summary>
    /// <param name="oldClock">The previous time the update was run.</param>
    /// <param name="newClock">The current time the update is run.</param>
    public static void PeriodicCouriersUpdate(DateTime oldClock, DateTime newClock)
    {
        var inactivityRange = AdminManager.GetConfig().InactivityRange;
        var activeCouriers = ReadAll(c => c.Active).ToList(); 
        var allDeliveries = DeliveryManager.ReadAll().ToList();

        var couriersToDeactivate = new List<BO.Courier>();

        foreach (var courier in activeCouriers)
        {
            var courierDeliveries = allDeliveries.Where(d => d.CourierId == courier.Id).ToList();

            // If the courier has a delivery in progress, they are considered active.
            if (courierDeliveries.Any(d => !d.DeliveryEndTime.HasValue))
            {
                continue; // Skip to the next courier.
            }

            DateTime? lastInvolvementDate;

            var completedDeliveries = courierDeliveries.Where(d => d.DeliveryEndTime.HasValue).ToList();

            if (completedDeliveries.Any())
            {
                lastInvolvementDate = completedDeliveries.Max(d => d.DeliveryEndTime.Value);
            }
            else
            {
                // If no completed deliveries, their last "activity" is when they were hired.
                lastInvolvementDate = courier.EmploymentStartTime;
            }

            // Now check if the inactivity period has been exceeded.
            if (lastInvolvementDate.HasValue && (AdminManager.Now - lastInvolvementDate.Value > inactivityRange))
            {
                couriersToDeactivate.Add(courier);
            }
        }

        if (couriersToDeactivate.Any())
        {
            foreach (var courier in couriersToDeactivate)
            {
                courier.Active = false;
                UpdateCourier(courier);
            }
        }
    }
}
