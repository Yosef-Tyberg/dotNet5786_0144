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
    public void Update(int orderId, BO.Order updatedOrder) => OrderManager.Update(orderId, updatedOrder);

    /// <inheritdoc />
    public int Create(BO.Order newOrder) => OrderManager.Create(newOrder);

    /// <inheritdoc />
    public IEnumerable<BO.OrderInList> GetAvailableOrders(int courierId) =>
        OrderManager.GetAvailableOrders(courierId).Select(OrderManager.ConvertBoToOrderInList);

    /// <inheritdoc />
    public void TakeOrder(int orderId, int courierId) => OrderManager.TakeOrder(orderId, courierId);
}
