using System.Linq;

namespace Helpers;

/// <summary>
/// Manager class for OrderTracking operations.
/// </summary>
internal static class OrderTrackingManager
{
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Generates a tracking report for a specific order.
    /// </summary>
    public static BO.OrderTracking Track(int orderId)
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

        var doDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId);

        // Calculate Status
        BO.OrderStatus status = BO.OrderStatus.Open;
        DO.Delivery? activeDelivery = doDeliveries.FirstOrDefault(d => d.DeliveryEndTime == null);

        if (activeDelivery != null)
        {
            status = BO.OrderStatus.InProgress;
        }
        else if (doDeliveries.Any())
        {
            var lastEnded = doDeliveries.OrderByDescending(d => d.DeliveryEndTime).First();
            status = lastEnded.DeliveryEndType switch
            {
                DO.DeliveryEndTypes.Delivered => BO.OrderStatus.Delivered,
                DO.DeliveryEndTypes.CustomerRefused => BO.OrderStatus.Refused,
                DO.DeliveryEndTypes.Cancelled => BO.OrderStatus.Cancelled,
                // Failed or RecipientNotFound implies the order is back to Open
                _ => BO.OrderStatus.Open
            };
        }

        // Get Assigned Courier (only if InProgress)
        BO.CourierInList? assignedCourier = null;
        if (status == BO.OrderStatus.InProgress && activeDelivery != null)
        {
            try
            {
                var doCourier = s_dal.Courier.Read(activeDelivery.CourierId);
                assignedCourier = CourierManager.ConvertBoToCourierInList(CourierManager.ConvertDoToBo(doCourier));
            }
            catch (DO.DalDoesNotExistException) { /* Courier might have been deleted, ignore */ }
        }

        // Map Delivery History
        var history = doDeliveries
            .OrderBy(d => d.DeliveryStartTime)
            .Select(d => DeliveryManager.ConvertBoToDeliveryInList(DeliveryManager.ConvertDoToBo(d)))
            .ToList();

        // Calculate Timing Properties
        var config = AdminManager.GetConfig();
        DateTime? maxDeliveryTime = doOrder.OrderOpenTime.Add(config.MaxDeliveryTimeSpan);
        DateTime? expectedDeliveryTime = null;

        if (status == BO.OrderStatus.InProgress && activeDelivery != null && assignedCourier != null)
        {
            double distance;
            switch (assignedCourier.DeliveryType)
            {
                case BO.DeliveryTypes.Car:
                case BO.DeliveryTypes.Motorcycle:
                    distance = Helpers.Tools.GetDrivingDistance((double)config.Latitude, (double)config.Longitude, doOrder.Latitude, doOrder.Longitude);
                    break;
                case BO.DeliveryTypes.Bicycle:
                case BO.DeliveryTypes.OnFoot:
                    distance = Helpers.Tools.GetWalkingDistance((double)config.Latitude, (double)config.Longitude, doOrder.Latitude, doOrder.Longitude);
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
                expectedDeliveryTime = activeDelivery.DeliveryStartTime.AddHours(travelHours);
            }
        }

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
            MaxDeliveryTime = maxDeliveryTime
        };
    }

}