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
        var delivery = DeliveryManager.ReadAll().First();

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

    #endregion

    #region PickUp Tests

    [TestMethod]
    public void Test_PickUp_Success()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        var courier = CourierManager.ReadAll(c => c.Active && DeliveryManager.GetMyCurrentDelivery(c.Id) == null && c.PersonalMaxDeliveryDistance > 0).First();
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id))
                                .Select(OrderManager.ConvertDoToBo)
                                .First(o => CourierManager.IsOrderInCourierRange(o, courier, (double)config.Latitude, (double)config.Longitude));
        
        // Act
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Assert
        var delivery = _bl.Delivery.GetMyCurrentDelivery(courier.Id);
        Assert.IsNotNull(delivery, "Delivery should be created and active.");
        Assert.AreEqual(order.Id, delivery.OrderId);
        Assert.AreEqual(courier.Id, delivery.CourierId);
    }
    
    [TestMethod]
    [ExpectedException(typeof(BlOrderAlreadyAssignedException))]
    public void Test_PickUp_OrderAlreadyInProgress_ThrowsException()
    {
        // Arrange: Pick up an order
        var courier1 = CourierManager.ReadAll(c => c.Active && c.Id != 0).First();
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();
        _bl.Delivery.PickUp(courier1.Id, order.Id);

        // Arrange: Find another courier to try and pick it up again
        var courier2 = CourierManager.ReadAll(c => c.Active && c.Id != courier1.Id).First();

        // Act
        _bl.Delivery.PickUp(courier2.Id, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierAlreadyHasDeliveryException))]
    public void Test_PickUp_CourierAlreadyHasDelivery_ThrowsException()
    {
        // Arrange: A courier picks up one order
        var courier = CourierManager.ReadAll(c => c.Active && c.Id != 0).First();
        var order1 = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();
         _bl.Delivery.PickUp(courier.Id, order1.Id);
        
        // Arrange: Find a second order for them to try and pick up
        var order2 = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();

        // Act
        _bl.Delivery.PickUp(courier.Id, order2.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_PickUp_OrderOutOfRange_ThrowsException()
    {
        // Arrange: Find a courier and set their range to be very small
        var courier = _bl.Courier.Read(CourierManager.ReadAll(c => c.Active).First().Id);
        courier.PersonalMaxDeliveryDistance = 0.1; // 100 meters
        _bl.Courier.Update(courier);
        
        // Arrange: Find an order that is far away (most will be)
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();

        // Act
        _bl.Delivery.PickUp(courier.Id, order.Id);
    }

    #endregion

    #region Deliver Tests

    [TestMethod]
    public void Test_Deliver_Success()
    {
        // Arrange: A courier picks up an order
        var courier = CourierManager.ReadAll(c => c.Active && c.Id != 0).First();
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        
        // Act
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);

        // Assert
        var delivery = _bl.Delivery.Read(order.Id);
        Assert.IsNotNull(delivery.DeliveryEndTime, "DeliveryEndTime should be set.");
        Assert.AreEqual(DeliveryEndTypes.Delivered, delivery.DeliveryEndType);
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierHasNoActiveDeliveryException))]
    public void Test_Deliver_CourierHasNoActiveDelivery_ThrowsException()
    {
        // Arrange: A courier that has NOT picked up an order
        var courier = CourierManager.ReadAll(c => c.Active && DeliveryManager.GetMyCurrentDelivery(c.Id) == null).First();
        
        // Act
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);
    }

    #endregion

    #region GetMyCurrentDelivery Tests

    [TestMethod]
    public void Test_GetMyCurrentDelivery_ReturnsActiveDelivery()
    {
        // Arrange: A courier picks up an order
        var courier = CourierManager.ReadAll(c => c.Active && c.Id != 0).First();
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        
        // Act
        var currentDelivery = _bl.Delivery.GetMyCurrentDelivery(courier.Id);

        // Assert
        Assert.IsNotNull(currentDelivery);
        Assert.AreEqual(order.Id, currentDelivery.OrderId);
    }

    [TestMethod]
    public void Test_GetMyCurrentDelivery_NoActiveDelivery_ReturnsNull()
    {
        // Arrange: A courier with no active delivery
        var courier = CourierManager.ReadAll(c => DeliveryManager.GetMyCurrentDelivery(c.Id) == null).First();

        // Act
        var currentDelivery = _bl.Delivery.GetMyCurrentDelivery(courier.Id);

        // Assert
        Assert.IsNull(currentDelivery);
    }

    #endregion

    #region Schedule Status Tests

    [TestMethod]
    public void Test_ScheduleStatus_ChangesOverTime()
    {
        // Arrange: A courier picks up an order
        var courier = CourierManager.ReadAll(c => c.Active && c.Id != 0).First();
        var order = OrderManager.ReadAll(o => !DeliveryManager.IsOrderTaken(o.Id)).Select(OrderManager.ConvertDoToBo).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);

        var delivery = _bl.Delivery.Read(order.Id);
        
        // Assert 1: Initially OnTime
        Assert.AreEqual(ScheduleStatus.OnTime, delivery.ScheduleStatus, "Delivery should start as OnTime.");
        
        // Act 2: Forward clock to be within the 'at risk' range
        var config = _bl.Admin.GetConfig();
        var timeToMaximum = delivery.MaximumDeliveryTime - AdminManager.Now;
        var timeToForward = timeToMaximum - (config.RiskRange / 2); // Go to middle of risk range
        
        _bl.Admin.ForwardClock(timeToForward);
        
        // Assert 2: Now AtRisk
        delivery = _bl.Delivery.Read(order.Id);
        Assert.AreEqual(ScheduleStatus.AtRisk, delivery.ScheduleStatus, "Delivery should become AtRisk.");
        
        // Act 3: Forward clock past the maximum delivery time
        _bl.Admin.ForwardClock(config.RiskRange); // Go past the max time
        
        // Assert 3: Now Late
        delivery = _bl.Delivery.Read(order.Id);
        Assert.AreEqual(ScheduleStatus.Late, delivery.ScheduleStatus, "Delivery should become Late.");
    }

    #endregion
}
