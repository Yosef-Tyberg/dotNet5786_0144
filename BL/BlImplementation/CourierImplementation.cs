using System;
using System.Collections.Generic;
using System.Linq;
using BlApi;

using Helpers;
    
namespace BlImplementation;

/// <summary>
/// Implementation of the ICourier interface.
/// </summary>
internal sealed class CourierImplementation : ICourier
{
    /// <inheritdoc />
    public IEnumerable<BO.CourierInList> ReadAll(Func<BO.Courier, bool>? filter = null)
    {
        return CourierManager.ReadAll(filter).Select(CourierManager.ConvertBoToCourierInList);
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
    public void Update(BO.Courier newDetails)
    {
        CourierManager.UpdateCourier(newDetails);
    }

    /// <inheritdoc />
    public void UpdateMyDetails(int courierId, string? fullName = null, string? phoneNum = null, string? email = null, string? password = null, double? maxDistance = null, BO.DeliveryTypes? deliveryType = null)
    {
        CourierManager.UpdateMyDetails(courierId, fullName, phoneNum, email, password, maxDistance, deliveryType);
    }

    /// <inheritdoc />
    public IEnumerable<BO.DeliveryInList> GetCourierDeliveryHistory(int courierId)
    {
        return CourierManager.GetCourierDeliveryHistory(courierId);
    }

    /// <inheritdoc />
    public IEnumerable<BO.OrderInList> GetOpenOrders(int courierId)
    {
        return CourierManager.GetOpenOrders(courierId);
    }

    /// <inheritdoc />
    public BO.CourierStatistics GetCourierStatistics(int courierId)
    {
        return CourierManager.GetCourierStatistics(courierId);
    }

    /// <inheritdoc />
    public bool IsCourierInDelivery(int courierId)
    {
        return CourierManager.IsCourierInDelivery(courierId);
    }


#region Stage 5 

    public void AddObserver(Action listObserver) =>
       CourierManager.Observers.AddListObserver(listObserver); //stage 5 
    public void AddObserver(int id, Action observer) =>
        CourierManager.Observers.AddObserver(id, observer); //stage 5 
    public void RemoveObserver(Action listObserver) =>
        CourierManager.Observers.RemoveListObserver(listObserver); //stage 5 
    public void RemoveObserver(int id, Action observer) =>
        CourierManager.Observers.RemoveObserver(id, observer); //stage 5 
#endregion Stage 5 
}
