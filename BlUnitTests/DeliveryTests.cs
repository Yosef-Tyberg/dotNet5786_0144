using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using BlApi;
using BO;
using System;
using System.Linq;

namespace BlUnitTests;

[TestClass]
public class DeliveryTests
{
    private readonly IBl _bl = Factory.Get();

    [TestInitialize]
    public void TestInitialize()
    {
        // Reset and initialize DB before every test to ensure isolation
        AdminManager.InitializeDB();
    }

    #region Read Tests

    [TestMethod]
    public void Test_ReadDelivery_Success()
    {
        // Arrange: Find a delivery that exists from init
        var delivery = _bl.Delivery.ReadAll().First();

        // Act
        var readDelivery = _bl.Delivery.Read(delivery.Id);

        // Assert
        Assert.IsNotNull(readDelivery);
        Assert.AreEqual(delivery.Id, readDelivery.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_ReadDelivery_NotFound_ThrowsException()
    {
        // Act
        _bl.Delivery.Read(99999); // Non-existent ID
    }

    [TestMethod]
    public void Test_ReadAllDeliveries_ReturnsData()
    {
        var deliveries = _bl.Delivery.ReadAll();
        Assert.IsTrue(deliveries.Any(), "ReadAll should return a list of deliveries.");
    }

    [TestMethod]
    public void Test_ReadAllDeliveries_WithFilter_ReturnsFilteredData()
    {
        // Filter for delivered items (assuming some exist from init)
        var filtered = _bl.Delivery.ReadAll(d => d.DeliveryEndType == DeliveryEndTypes.Delivered);
        if (filtered.Any())
            Assert.IsTrue(filtered.All(d => d.DeliveryEndType == DeliveryEndTypes.Delivered));
    }

    #endregion

    #region PickUp Tests

    [TestMethod]
    public void Test_PickUp_Success()
    {
        // Arrange
        var courier = _bl.Courier.ReadAll(c => c.Active && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order = _bl.Order.GetAvailableOrders(courier.Id).First();
        
        // Act
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Assert
        var delivery = _bl.Delivery.GetDeliveryByCourier(courier.Id);
        Assert.IsNotNull(delivery, "Delivery should be created and active.");
        Assert.AreEqual(order.Id, delivery.OrderId);
        Assert.AreEqual(courier.Id, delivery.CourierId);
    }
    
    [TestMethod]
    [ExpectedException(typeof(BlOrderAlreadyAssignedException))]
    public void Test_PickUp_OrderAlreadyInProgress_ThrowsException()
    {
        // Arrange: Pick up an order
        var courier1 = _bl.Courier.ReadAll(c => c.Active && c.Id != 0 && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier1.Id, order.Id);

        // Arrange: Find another courier to try and pick it up again
        var courier2 = _bl.Courier.ReadAll(c => c.Active && c.Id != courier1.Id && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();

        // Act
        _bl.Delivery.PickUp(courier2.Id, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDeliveryAlreadyClosedException))]
    public void Test_PickUp_OrderAlreadyClosed_ThrowsException()
    {
        // Arrange: Deliver an order to close it
        var courier1 = _bl.Courier.ReadAll(c => c.Active && c.Id != 0 && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier1.Id, order.Id);
        _bl.Delivery.Deliver(courier1.Id, DeliveryEndTypes.Delivered);

        // Arrange: Find another courier to try and pick it up
        var courier2 = _bl.Courier.ReadAll(c => c.Active && c.Id != courier1.Id && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        
        // Act
        _bl.Delivery.PickUp(courier2.Id, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierAlreadyHasDeliveryException))]
    public void Test_PickUp_CourierAlreadyHasDelivery_ThrowsException()
    {
        // Arrange: A courier picks up one order
        var courier = _bl.Courier.ReadAll(c => c.Active && c.Id != 0 && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order1 = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
         _bl.Delivery.PickUp(courier.Id, order1.Id);
        
        // Arrange: Find a second order for them to try and pick up
        var order2 = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open && o.Id != order1.Id).First();

        // Act
        _bl.Delivery.PickUp(courier.Id, order2.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_PickUp_OrderOutOfRange_ThrowsException()
    {
        // Arrange: Find a courier and set their range to be very small
        var courier = _bl.Courier.Read(_bl.Courier.ReadAll(c => c.Active && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First().Id);
        courier.PersonalMaxDeliveryDistance = 0.1; // 100 meters
        _bl.Courier.Update(courier);
        
        // Arrange: Find an order that is far away by taking one not in the available list
        var allOpenOrders = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).Select(o => o.Id);
        var availableOrders = _bl.Order.GetAvailableOrders(courier.Id).Select(o => o.Id);
        var outOfRangeOrderId = allOpenOrders.Except(availableOrders).First();
        
        // Act
        _bl.Delivery.PickUp(courier.Id, outOfRangeOrderId);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_PickUp_CourierInactive_ThrowsException()
    {
        // Arrange
        var inactiveCourier = _bl.Courier.ReadAll(c => !c.Active).FirstOrDefault();
        if (inactiveCourier == null) Assert.Inconclusive("No inactive couriers in DB.");
        
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();

        // Act
        _bl.Delivery.PickUp(inactiveCourier.Id, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_PickUp_CourierNotFound_ThrowsException()
    {
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(999999, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_PickUp_OrderNotFound_ThrowsException()
    {
        var courier = _bl.Courier.ReadAll(c => c.Active && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        _bl.Delivery.PickUp(courier.Id, 999999);
    }

    #endregion

    #region Deliver Tests

    [TestMethod]
    public void Test_Deliver_Success()
    {
        // Arrange: A courier picks up an order
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.Delivered);

        // Assert
        var delivery = _bl.Delivery.Read(orderId);
        Assert.IsNotNull(delivery.DeliveryEndTime, "DeliveryEndTime should be set.");
        Assert.AreEqual(DeliveryEndTypes.Delivered, delivery.DeliveryEndType);
    }

    [TestMethod]
    public void Test_Deliver_WithStatus_CustomerRefused()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.CustomerRefused);

        // Assert
        var delivery = _bl.Delivery.Read(orderId);
        var order = _bl.Order.Read(orderId);
        Assert.AreEqual(DeliveryEndTypes.CustomerRefused, delivery.DeliveryEndType);
        Assert.AreEqual(OrderStatus.Refused, order.OrderStatus, "Order status should be Refused.");
    }

    [TestMethod]
    public void Test_Deliver_WithStatus_Cancelled()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.Cancelled);

        // Assert
        var delivery = _bl.Delivery.Read(orderId);
        var order = _bl.Order.Read(orderId);
        Assert.AreEqual(DeliveryEndTypes.Cancelled, delivery.DeliveryEndType);
        Assert.AreEqual(OrderStatus.Cancelled, order.OrderStatus, "Order status should be Cancelled.");
    }

    [TestMethod]
    public void Test_Deliver_WithStatus_RecipientNotFound()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.RecipientNotFound);

        // Assert
        var delivery = _bl.Delivery.Read(orderId);
        var order = _bl.Order.Read(orderId);
        Assert.AreEqual(DeliveryEndTypes.RecipientNotFound, delivery.DeliveryEndType);
        Assert.AreEqual(OrderStatus.Open, order.OrderStatus, "Order status should revert to Open.");
    }

    [TestMethod]
    public void Test_Deliver_WithStatus_Failed()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.Failed);

        // Assert
        var delivery = _bl.Delivery.Read(orderId);
        var order = _bl.Order.Read(orderId);
        Assert.AreEqual(DeliveryEndTypes.Failed, delivery.DeliveryEndType);
        Assert.AreEqual(OrderStatus.Open, order.OrderStatus, "Order status should revert to Open.");
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierHasNoActiveDeliveryException))]
    public void Test_Deliver_CourierHasNoActiveDelivery_ThrowsException()
    {
        // Arrange: A courier that has NOT picked up an order
        var courier = _bl.Courier.ReadAll(c => c.Active && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        
        // Act
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_Deliver_CourierNotFound_ThrowsException()
    {
        _bl.Delivery.Deliver(999999, DeliveryEndTypes.Delivered);
    }

    #endregion

    #region GetDeliveryByCourier Tests

    [TestMethod]
    public void Test_GetDeliveryByCourier_ReturnsActiveDelivery()
    {
        // Arrange: A courier picks up an order
        var (orderId, courierId) = SetupInProgressOrder();
        
        // Act
        var currentDelivery = _bl.Delivery.GetDeliveryByCourier(courierId);

        // Assert
        Assert.IsNotNull(currentDelivery);
        Assert.AreEqual(orderId, currentDelivery.OrderId);
    }

    [TestMethod]
    public void Test_GetDeliveryByCourier_NoActiveDelivery_ReturnsNull()
    {
        // Arrange: A courier with no active delivery
        var courier = _bl.Courier.ReadAll(c => _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();

        // Act
        var currentDelivery = _bl.Delivery.GetDeliveryByCourier(courier.Id);

        // Assert
        Assert.IsNull(currentDelivery);
    }

    #endregion

    #region Helper Methods

    private (int orderId, int courierId) SetupInProgressOrder()
    {
        var courier = _bl.Courier.ReadAll(c => c.Active && c.Id != 0 && _bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        return (order.Id, courier.Id);
    }

    #endregion

    #region Helper Methods Tests

    [TestMethod]
    public void Test_IsOrderTaken_ReturnsTrueForActiveDelivery()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();

        // Act
        bool taken = DeliveryManager.IsOrderTaken(orderId);

        // Assert
        Assert.IsTrue(taken, "Order should be taken after delivery creation");
    }

    #endregion
}