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
            OrderType = OrderTypes.Groceries,
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
        var createdOrder = OrderManager.ReadAll().FirstOrDefault(o => o.CustomerFullName == "Test Customer");
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
            OrderType = OrderTypes.Groceries,
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
            OrderType = OrderTypes.Groceries,
            VerbalDescription = "A valid test order",
            Weight = 5,
            Volume = 2,
            Height = 1,
            Width = 1
        };
        
        // Act
        _bl.Order.Create(invalidOrder);
    }

    #endregion

    #region Read and Update Tests

    [TestMethod]
    public void Test_ReadOrder_Success()
    {
        // Arrange
        var orderId = OrderManager.ReadAll().First().Id;

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
    public void Test_UpdateOrder_Success()
    {
        // Arrange
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
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
        var courier = CourierManager.ReadAll(c => c.Active).First();
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
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
        var order = _bl.Order.Read(OrderManager.ReadAll().First().Id);
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

    #endregion



    #region Delete and Cancel Tests

    [TestMethod]
    public void Test_DeleteOrder_Success()
    {
        // Arrange
        var orderToDelete = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).First();

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
        var courier = CourierManager.ReadAll(c => c.Active).First();
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
        _bl.Delivery.PickUp(courier.Id, order.Id);

        // Act
        _bl.Order.Delete(order.Id);
    }

    [TestMethod]
    public void Test_CancelOrder_Open_Success()
    {
        // Arrange
        var orderToCancel = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
        
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
        var courier = CourierManager.ReadAll(c => c.Active).First();
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
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
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
        Assert.AreEqual(OrderStatus.Open, order.OrderStatus, "Order should start as Open.");
        
        // Act 1: Pick up
        var courier = CourierManager.ReadAll(c => c.Active).First();
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
        var courier = CourierManager.ReadAll(c => c.Active).First();
        var order = OrderManager.ReadAll(o => OrderManager.ConvertDoToBo(o).OrderStatus == OrderStatus.Open).Select(OrderManager.ConvertDoToBo).First();
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

    #endregion
}