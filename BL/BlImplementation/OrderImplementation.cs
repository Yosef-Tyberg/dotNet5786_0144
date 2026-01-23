using System;
using System.Collections.Generic;
using System.Linq;
using BlApi;
using Helpers;

namespace BlImplementation;

/// <summary>
/// Implementation of the IOrder interface.
/// </summary>
internal sealed class OrderImplementation : IOrder
{
    /// <inheritdoc />
    public IEnumerable<BO.OrderInList> ReadAll(Func<BO.OrderInList, bool>? filter = null)
    {
        var orders = OrderManager.ReadAll().Select(OrderManager.ConvertBoToOrderInList);
        return filter == null ? orders : orders.Where(filter);
    }

    /// <inheritdoc />
    public BO.Order Read(int orderId) => OrderManager.Read(orderId);

    /// <inheritdoc />
    public void Update(BO.Order updatedOrder) => OrderManager.Update(updatedOrder);

    /// <inheritdoc />
    public void Create(BO.Order newOrder) => OrderManager.Create(newOrder);


    public void Delete(int orderId) => OrderManager.Delete(orderId);

    /// <inheritdoc />
    public void Cancel(int orderId) => OrderManager.Cancel(orderId);
    
    /// <inheritdoc />
    public BO.OrderTracking GetOrderTracking(int orderId) => OrderManager.GetOrderTracking(orderId);

    #region Stage 5 
    public void AddObserver(Action listObserver) =>
       OrderManager.Observers.AddListObserver(listObserver); //stage 5 
    public void AddObserver(int id, Action observer) =>
        OrderManager.Observers.AddObserver(id, observer); //stage 5 
    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observers.RemoveListObserver(listObserver); //stage 5 
    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observers.RemoveObserver(id, observer); //stage 5 
    #endregion Stage 5 
}
