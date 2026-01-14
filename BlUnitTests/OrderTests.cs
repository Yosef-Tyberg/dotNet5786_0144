using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using BlApi;
using BO;
using System;
using System.Linq;

namespace BlUnitTests;

[TestClass]
public class OrderTests
{
    private readonly IBl _bl = Factory.Get();

    [TestInitialize]
    public void TestInitialize()
    {
        AdminManager.InitializeDB();
    }

    #region Create Tests

    [TestMethod]
    public void Test_CreateOrder_Success()
    {
        // Arrange
        var newOrder = new Order
        {
            Id = 0,
            CustomerFullName = "Test Customer",
            CustomerMobile = "050-111-2222",
            FullOrderAddress = "Jaffa Road 2, Jerusalem", // Valid, known address
            OrderType = OrderTypes.Pizza,
            VerbalDescription = "A valid test order",
            Weight = 5,
            Volume = 2,
            Height = 1,
            Width = 1,
            Fragile = false,
            OrderOpenTime = AdminManager.Now
        };

        // Act
        _bl.Order.Create(newOrder);

        // Assert
        var createdOrder = _bl.Order.ReadAll().FirstOrDefault(o => o.CustomerFullName == "Test Customer");
        Assert.IsNotNull(createdOrder, "Order should have been created.");
        Assert.AreEqual(OrderStatus.Open, createdOrder.OrderStatus);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_CreateOrder_InvalidData_ThrowsException()
    {
        // Arrange
        var invalidOrder = new Order
        {
            CustomerFullName = " ", // Invalid
            CustomerMobile = "050-111-2222",
            FullOrderAddress = "Jaffa Road 2, Jerusalem",
            OrderType = OrderTypes.Pizza,
            VerbalDescription = "Invalid",
            Weight = -5 // Invalid
        };

        // Act
        _bl.Order.Create(invalidOrder);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidAddressException))]
    public void Test_CreateOrder_InvalidAddress_ThrowsException()
    {
        // Arrange
        var invalidOrder = new Order
        {
            CustomerFullName = "Test Customer",
            CustomerMobile = "050-111-2222",
            FullOrderAddress = "123 Fake Street, Nowhere", // Invalid address
            OrderType = OrderTypes.Pizza,
            VerbalDescription = "A valid test order",
            Weight = 5,
            Volume = 2,
            Height = 1,
            Width = 1
        };
        
        // Act
        _bl.Order.Create(invalidOrder);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_CreateOrder_ExceedsMaxGeneralDistance_ThrowsException()
    {
        // Arrange
        // Set a small max distance in the config
        var config = _bl.Admin.GetConfig();
        config.MaxGeneralDeliveryDistanceKm = 20; // 20 km
        _bl.Admin.SetConfig(config);

        // Create an order for a location known to be further than 20km from the default company address
        var farOrder = new Order
        {
            CustomerFullName = "Far Away Customer",
            CustomerMobile = "050-999-8888",
            FullOrderAddress = "HaNessi Boulevard 1, Haifa", // ~150km from Jerusalem
            OrderType = OrderTypes.Falafel, Weight = 1, Height = 1, Width = 1, Volume = 1
        };

        // Act
        _bl.Order.Create(farOrder); // This should throw
    }

    #endregion

    #region Read and Update Tests

    [TestMethod]
    public void Test_ReadOrder_Success()
    {
        // Arrange
        var orderId = _bl.Order.ReadAll().First().Id;

        // Act
        var order = _bl.Order.Read(orderId);

        // Assert
        Assert.IsNotNull(order);
        Assert.AreEqual(orderId, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_ReadOrder_NotFound_ThrowsException()
    {
        _bl.Order.Read(999999); // Non-existent ID
    }

    [TestMethod]
    public void Test_ReadAllOrders_ReturnsData()
    {
        var orders = _bl.Order.ReadAll();
        Assert.IsTrue(orders.Any(), "ReadAll should return a list of orders.");
    }

    [TestMethod]
    public void Test_ReadAllOrders_WithFilter_ReturnsFilteredData()
    {
        var all = _bl.Order.ReadAll();
        var filtered = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open);
        Assert.IsTrue(filtered.All(o => o.OrderStatus == OrderStatus.Open));
        // Note: We assume there are some open orders from initialization
    }

    [TestMethod]
    public void Test_ReadAllOrders_IsSortedById_ByDefault()
    {
        // Arrange
        // The DB is initialized with several orders.

        // Act
        var ordersFromBl = _bl.Order.ReadAll().ToList();
        var manuallySortedOrders = ordersFromBl.OrderBy(o => o.Id).ToList();

        // Assert
        // Verify that the collection returned from the BL is already sorted by ID.
        CollectionAssert.AreEqual(manuallySortedOrders, ordersFromBl, "The ReadAll method should return orders sorted by ID by default.");
    }

    [TestMethod]
    public void Test_UpdateOrder_Success()
    {
        // Arrange
        var orderInList = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var order = _bl.Order.Read(orderInList.Id);
        order.CustomerFullName = "Updated Customer Name";

        // Act
        _bl.Order.Update(order);
        var updatedOrder = _bl.Order.Read(order.Id);

        // Assert
        Assert.AreEqual("Updated Customer Name", updatedOrder.CustomerFullName);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDeliveryInProgressException))]
    public void Test_UpdateOrder_WhileInProgress_ThrowsException()
    {
        // Arrange: Pick up an order
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var orderInList = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var order = _bl.Order.Read(orderInList.Id);
        _bl.Delivery.PickUp(courier.Id, order.Id);
        
        // Act: Try to update it
        order.VerbalDescription = "This should fail";
        _bl.Order.Update(order);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_UpdateOrder_InvalidData_ThrowsException()
    {
        // Arrange
        var order = _bl.Order.Read(_bl.Order.ReadAll().First().Id);
        order.Weight = -10; // Invalid

        // Act
        _bl.Order.Update(order);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_UpdateOrder_NotFound_ThrowsException()
    {
        var order = new Order { Id = 999999, CustomerFullName = "Ghost", FullOrderAddress = "Nowhere", CustomerMobile = "000", VerbalDescription = "None" };
        _bl.Order.Update(order);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDeliveryAlreadyClosedException))]
    public void Test_UpdateOrder_AlreadyDelivered_ThrowsException()
    {
        // Arrange: Deliver an order
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var orderInList = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var order = _bl.Order.Read(orderInList.Id);
        _bl.Delivery.PickUp(courier.Id, order.Id);
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);

        // Act: Try to update it
        var deliveredOrder = _bl.Order.Read(order.Id);
        deliveredOrder.VerbalDescription = "This update should fail";
        _bl.Order.Update(deliveredOrder);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_UpdateOrder_AddressExceedsMaxDistance_ThrowsException()
    {
        // Arrange
        // Set a small max distance in the config
        var config = _bl.Admin.GetConfig();
        config.MaxGeneralDeliveryDistanceKm = 20; // 20 km
        _bl.Admin.SetConfig(config);

        // Get an open order
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var orderToUpdate = _bl.Order.Read(order.Id);

        // Act: Update the address to a location known to be further than 20km
        orderToUpdate.FullOrderAddress = "HaNessi Boulevard 1, Haifa"; // ~150km from Jerusalem
        _bl.Order.Update(orderToUpdate); // This should throw
    }

    #endregion



    #region Delete and Cancel Tests

    [TestMethod]
    public void Test_DeleteOrder_Success()
    {
        // Arrange
        var orderToDelete = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();

        // Act
        _bl.Order.Delete(orderToDelete.Id);

        // Assert
        Assert.ThrowsException<BlDoesNotExistException>(() => _bl.Order.Read(orderToDelete.Id));
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_DeleteOrder_NotFound_ThrowsException()
    {
        _bl.Order.Delete(999999);
    }

    [TestMethod]
    [ExpectedException(typeof(BlOrderAlreadyAssignedException))]
    public void Test_DeleteOrder_AlreadyAssigned_ThrowsException()
    {
        // Arrange: Pick up an order
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Act
        _bl.Order.Delete(order.Id);
    }

    [TestMethod]
    public void Test_CancelOrder_Open_Success()
    {
        // Arrange
        var orderToCancel = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        
        // Act
        _bl.Order.Cancel(orderToCancel.Id);
        var cancelledOrder = _bl.Order.Read(orderToCancel.Id);

        // Assert
        Assert.AreEqual(OrderStatus.Cancelled, cancelledOrder.OrderStatus);
    }
    
    [TestMethod]
    public void Test_CancelOrder_InProgress_Success()
    {
        // Arrange: Pick up an order to make it InProgress
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Act
        _bl.Order.Cancel(order.Id);
        var cancelledOrder = _bl.Order.Read(order.Id);

        // Assert
        Assert.AreEqual(OrderStatus.Cancelled, cancelledOrder.OrderStatus);
    }

    [TestMethod]
    [ExpectedException(typeof(BlOrderCannotBeCancelledException))]
    public void Test_CancelOrder_Delivered_ThrowsException()
    {
        // Arrange: Deliver an order
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);

        // Act
        _bl.Order.Cancel(order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_CancelOrder_NotFound_ThrowsException()
    {
        _bl.Order.Cancel(999999);
    }

    #endregion

    #region Order Lifecycle and Tracking

    [TestMethod]
    public void Test_OrderStatus_ChangesCorrectly_Open_InProgress_Delivered()
    {
        // Arrange
        var orderInList = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var order = _bl.Order.Read(orderInList.Id);
        Assert.AreEqual(OrderStatus.Open, order.OrderStatus, "Order should start as Open.");
        
        // Act 1: Pick up
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        var inProgressOrder = _bl.Order.Read(order.Id);
        
        // Assert 1: InProgress
        Assert.AreEqual(OrderStatus.InProgress, inProgressOrder.OrderStatus, "Order should be InProgress after pickup.");

        // Act 2: Deliver
        _bl.Delivery.Deliver(courier.Id, DeliveryEndTypes.Delivered);
        var deliveredOrder = _bl.Order.Read(order.Id);

        // Assert 2: Delivered
        Assert.AreEqual(OrderStatus.Delivered, deliveredOrder.OrderStatus, "Order should be Delivered after delivery.");
    }
    
    [TestMethod]
    public void Test_GetOrderTracking_InProgressOrder_ReturnsCorrectData()
    {
        // Arrange: Pick up an order
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Act
        var trackingInfo = _bl.Order.GetOrderTracking(order.Id);

        // Assert
        Assert.IsNotNull(trackingInfo);
        Assert.AreEqual(order.Id, trackingInfo.Id);
        Assert.AreEqual(OrderStatus.InProgress, trackingInfo.Status);
        Assert.IsNotNull(trackingInfo.AssignedCourier);
        Assert.AreEqual(courier.Id, trackingInfo.AssignedCourier.Id);
        Assert.IsTrue(trackingInfo.DeliveryHistory.Any());
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_GetOrderTracking_NotFound_ThrowsException()
    {
        _bl.Order.GetOrderTracking(999999);
    }

    [TestMethod]
    public void Test_GetOrderTracking_OpenOrder_ReturnsCorrectData()
    {
        // Arrange
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();

        // Act
        var trackingInfo = _bl.Order.GetOrderTracking(order.Id);

        // Assert
        Assert.IsNotNull(trackingInfo);
        Assert.AreEqual(order.Id, trackingInfo.Id);
        Assert.AreEqual(OrderStatus.Open, trackingInfo.Status);
        Assert.IsNull(trackingInfo.AssignedCourier, "Open order should not have an assigned courier.");
        Assert.IsFalse(trackingInfo.DeliveryHistory?.Any() ?? false, "Open order should have no delivery history.");
        Assert.IsNull(trackingInfo.ExpectedDeliveryTime, "Open order should not have an expected delivery time.");
    }

    [TestMethod]
    public void Test_GetOrderTracking_DeliveredOrder_ReturnsCorrectData()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.Delivered);

        // Act
        var trackingInfo = _bl.Order.GetOrderTracking(orderId);

        // Assert
        Assert.IsNotNull(trackingInfo);
        Assert.AreEqual(orderId, trackingInfo.Id);
        Assert.AreEqual(OrderStatus.Delivered, trackingInfo.Status);
        Assert.IsNull(trackingInfo.AssignedCourier, "Delivered order should not have an active assigned courier.");
        Assert.IsTrue(trackingInfo.DeliveryHistory.Any(d => d.DeliveryEndType == DeliveryEndTypes.Delivered));
        Assert.AreEqual(1, trackingInfo.DeliveryHistory.Count());
    }

    [TestMethod]
    public void Test_GetOrderTracking_CancelledOrder_ReturnsCorrectData()
    {
        // Arrange
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        _bl.Order.Cancel(order.Id);

        // Act
        var trackingInfo = _bl.Order.GetOrderTracking(order.Id);

        // Assert
        Assert.IsNotNull(trackingInfo);
        Assert.AreEqual(order.Id, trackingInfo.Id);
        Assert.IsTrue(trackingInfo.DeliveryHistory.Any(d => d.DeliveryEndType == DeliveryEndTypes.Cancelled));
    }

    [TestMethod]
    public void Test_GetOrderTracking_RefusedOrder_ReturnsCorrectData()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.CustomerRefused);

        // Act
        var trackingInfo = _bl.Order.GetOrderTracking(orderId);

        // Assert
        Assert.IsNotNull(trackingInfo);
        Assert.AreEqual(orderId, trackingInfo.Id);
        Assert.AreEqual(OrderStatus.Refused, trackingInfo.Status);
        Assert.IsNull(trackingInfo.AssignedCourier);
        Assert.IsTrue(trackingInfo.DeliveryHistory.Any(d => d.DeliveryEndType == DeliveryEndTypes.CustomerRefused));
    }



    [TestMethod]
    public void Test_ScheduleStatus_ChangesTo_AtRisk_And_Late()
    {
        // Arrange
        var config = _bl.Admin.GetConfig();
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var courier = _bl.Courier.ReadAll(c => c.Active).First();

        // Act 1: Pick up the order and check initial state
        _bl.Delivery.PickUp(courier.Id, order.Id);
        var inProgressTracking = _bl.Order.GetOrderTracking(order.Id);
        var currentDelivery = inProgressTracking.DeliveryHistory.First();

        // Assert 1: Should be OnTime right after pickup
        Assert.AreEqual(ScheduleStatus.OnTime, currentDelivery.ScheduleStatus, "Order should be OnTime immediately after pickup.");
        Assert.IsNotNull(inProgressTracking.MaxDeliveryTime, "MaximumDeliveryTime should be calculated for an in-progress order.");

        // Arrange 2: Calculate time to enter the 'AtRisk' period
        var timeToRisk = (DateTime)inProgressTracking.MaxDeliveryTime - _bl.Admin.GetClock() - config.RiskRange;
        _bl.Admin.ForwardClock(timeToRisk.Add(TimeSpan.FromSeconds(1)));

        // Act 2: Re-read the order tracking
        var atRiskTracking = _bl.Order.GetOrderTracking(order.Id);
        var atRiskDelivery = atRiskTracking.DeliveryHistory.First();

        // Assert 2: Should now be AtRisk
        Assert.AreEqual(ScheduleStatus.AtRisk, atRiskDelivery.ScheduleStatus, "Order should be AtRisk when approaching the deadline.");

        // Arrange 3: Calculate time to become 'Late'
        var timeToLate = (DateTime)atRiskTracking.MaxDeliveryTime - _bl.Admin.GetClock();
        _bl.Admin.ForwardClock(timeToLate.Add(TimeSpan.FromSeconds(1)));

        // Act 3: Re-read the order tracking
        var lateTracking = _bl.Order.GetOrderTracking(order.Id);
        var lateDelivery = lateTracking.DeliveryHistory.First();

        // Assert 3: Should now be Late
        Assert.AreEqual(ScheduleStatus.Late, lateDelivery.ScheduleStatus, "Order should be Late after the deadline has passed.");
    }

    [TestMethod]
    public void Test_OrderStatus_RevertsToOpen_When_RecipientNotFound()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();

        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.RecipientNotFound);
        var updatedOrder = _bl.Order.Read(orderId);

        // Assert
        Assert.AreEqual(OrderStatus.Open, updatedOrder.OrderStatus, "Order status should revert to Open if recipient is not found.");
    }

    [TestMethod]
    public void Test_OrderStatus_RevertsToOpen_When_DeliveryFailed()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();

        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.Failed);
        var updatedOrder = _bl.Order.Read(orderId);

        // Assert
        Assert.AreEqual(OrderStatus.Open, updatedOrder.OrderStatus, "Order status should revert to Open if delivery fails.");
    }

    [TestMethod]
    public void Test_OrderStatus_BecomesRefused_When_CustomerRefused()
    {
        // Arrange
        var (orderId, courierId) = SetupInProgressOrder();

        // Act
        _bl.Delivery.Deliver(courierId, DeliveryEndTypes.CustomerRefused);
        var updatedOrder = _bl.Order.Read(orderId);

        // Assert
        Assert.AreEqual(OrderStatus.Refused, updatedOrder.OrderStatus, "Order status should become Refused if customer refuses delivery.");
    }

    #endregion

    #region Helper Methods

    private (int orderId, int courierId) SetupInProgressOrder()
    {
        var order = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        var courier = _bl.Courier.ReadAll(c => c.Active).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);
        return (order.Id, courier.Id);
    }

    #endregion

    #region Courier-Facing Method Tests

    [TestMethod]
    public void Test_GetAvailableOrders_ReturnsOnlyOrdersWithinRange()
    {
        // Arrange
        // Find a courier and set a specific delivery distance
        var courier = _bl.Courier.Read(_bl.Courier.ReadAll().First().Id);
        _bl.Courier.UpdateMyDetails(courier.Id, maxDistance: 10); // 10 km
        var updatedCourier = _bl.Courier.Read(courier.Id);

        // Create an order that is definitely WITHIN the 10km range.
        // Address is ~2.5km from the default company address in Jerusalem.
        var nearOrder = new Order
        {
            CustomerFullName = "Near Customer",
            CustomerMobile = "050-123-4567",
            FullOrderAddress = "King George Street 1, Jerusalem", // ~2.5km from Jaffa 24
            OrderType = OrderTypes.Pizza, Weight = 1, Height = 1, Width = 1, Volume = 1
        };
        _bl.Order.Create(nearOrder);
        var createdNearOrder = _bl.Order.ReadAll(o => o.CustomerFullName == "Near Customer").First();


        // Create an order that is definitely OUTSIDE the 10km range.
        // Address is ~50km from Jerusalem.
        var farOrder = new Order
        {
            CustomerFullName = "Far Customer",
            CustomerMobile = "050-765-4321",
            FullOrderAddress = "Rothschild Boulevard 16, Tel Aviv-Yafo", // ~50km from Jerusalem
            OrderType = OrderTypes.Pizza, Weight = 1, Height = 1, Width = 1, Volume = 1
        };
        _bl.Order.Create(farOrder);
        var createdFarOrder = _bl.Order.ReadAll(o => o.CustomerFullName == "Far Customer").First();

        // Act
        var availableOrders = _bl.Order.GetAvailableOrders(updatedCourier.Id);

        // Assert
        Assert.IsTrue(availableOrders.Any(o => o.Id == createdNearOrder.Id), "Should include the 'near' order.");
        Assert.IsFalse(availableOrders.Any(o => o.Id == createdFarOrder.Id), "Should NOT include the 'far' order.");
    }

    [TestMethod]
    public void Test_GetAvailableOrders_CourierWithNoDistance_ReturnsAll()
    {
        // Arrange
        var courier = _bl.Courier.Read(_bl.Courier.ReadAll().First().Id);
        // Ensure courier has no max distance set, meaning no limit
        _bl.Courier.UpdateMyDetails(courier.Id, maxDistance: null); 
        var updatedCourier = _bl.Courier.Read(courier.Id);

        var allAvailableOrdersCount = _bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).Count();

        // Act
        var availableOrders = _bl.Order.GetAvailableOrders(updatedCourier.Id);

        // Assert
        Assert.AreEqual(allAvailableOrdersCount, availableOrders.Count(), "A courier with no max delivery distance should be shown all available orders.");
    }

    #endregion
}