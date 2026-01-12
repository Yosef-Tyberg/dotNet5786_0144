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

    /// <inheritdoc />
    public IEnumerable<BO.OrderInList> GetAvailableOrders(int courierId) =>
        OrderManager.GetAvailableOrders(courierId).Select(OrderManager.ConvertBoToOrderInList);


    public void Delete(int orderId) => OrderManager.Delete(orderId);
    
    /// <inheritdoc />
    public BO.OrderTracking GetOrderTracking(int orderId) => OrderManager.GetOrderTracking(orderId);
}
