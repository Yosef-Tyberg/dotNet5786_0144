using System;
using System.Collections.Generic;
using System.Linq;
using BlApi;
using BL.Helpers;

namespace BlImplementation;

/// <summary>
/// Implementation of the ICourier interface.
/// </summary>
internal sealed class CourierImplementation : ICourier
{
    /// <inheritdoc />
    public IEnumerable<BO.CourierInList> ReadAll(Func<BO.CourierInList, bool>? filter = null)
    {
        return CourierManager.ReadAllCouriersForList(filter);
    }

    /// <inheritdoc />
    public BO.Courier Read(int courierId)
    {
        return CourierManager.ReadCourier(courierId);
    }

    /// <inheritdoc />
    public void Create(BO.Courier courier)
    {
        CourierManager.CreateCourier(courier);
    }

    /// <inheritdoc />
    public void Delete(int courierId)
    {
        CourierManager.DeleteCourier(courierId);
    }

    /// <inheritdoc />
    public void Update(int courierId, BO.Courier newDetails)
    {
        if (courierId != newDetails.Id)
            throw new BO.BlInvalidInputException("The ID in the URL and the ID in the body must match.");
        CourierManager.UpdateCourier(newDetails);
    }

    /// <inheritdoc />
    public void UpdateMyDetails(int courierId, string? fullName = null, string? email = null, string? password = null, double? maxDistance = null)
    {
        var courier = CourierManager.ReadCourier(courierId);

        courier.FullName = fullName ?? courier.FullName;
        courier.Email = email ?? courier.Email;
        courier.Password = password ?? courier.Password;
        courier.PersonalMaxDeliveryDistance = maxDistance ?? courier.PersonalMaxDeliveryDistance;
        
        CourierManager.UpdateCourier(courier);
    }

    /// <inheritdoc />
    public IEnumerable<BO.DeliveryInList> GetMyDeliveryHistory(int courierId)
    {
        // This requires DeliveryManager, which is not yet implemented.
        // For now, we return an empty list.
        // A proper implementation would look something like this:
        // return DeliveryManager.GetDeliveriesForCourier(courierId);
        return Enumerable.Empty<BO.DeliveryInList>();
    }
}
