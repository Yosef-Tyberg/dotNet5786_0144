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
    private const int TEST_COURIER_ID = 555555555;

    [TestInitialize]
    public void TestInitialize()
    {
        var dal = DalApi.Factory.Get;

        // 1. Reset state to prevent cross-test interference
        dal.ResetDB();
        Tools.ClearCaches();
        Tools.SeedCoordinateCache(); // Prevent API timeouts

        // 2. Baseline initialization to set valid vehicle speeds and clock
        try { AdminManager.InitializeDB(); } catch { }

        // 3. Fix configuration leaks from other tests
        var config = _bl.Admin.GetConfig();
        config.AdminId = 1;
        config.MaxGeneralDeliveryDistanceKm = 5000;
        _bl.Admin.SetConfig(config);

        // 4. Create a permanent active courier for the test run
        dal.Courier.Create(new DO.Courier(
            Id: TEST_COURIER_ID,
            FullName: "Delivery Master", // Two-word valid name
            MobilePhone: "0559998888",
            Email: "master@delivery.com",
            Password: "password",
            Active: true,
            DeliveryType: DO.DeliveryTypes.Car,
            EmploymentStartTime: _bl.Admin.GetClock(),
            PersonalMaxDeliveryDistance: 5000.0
        ));
    }

    #region CRUD & Read Tests

    [TestMethod]
    public void Test_ReadDelivery_Success()
    {
        var order = CreateAndGetTestOrder("Read Success");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        // Must fetch the real Delivery ID assigned by the DAL
        var activeDelivery = _bl.Delivery.GetDeliveryByCourier(TEST_COURIER_ID);

        var readDelivery = _bl.Delivery.Read(activeDelivery.Id);
        Assert.IsNotNull(readDelivery);
        Assert.AreEqual(activeDelivery.Id, readDelivery.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_ReadDelivery_NotFound_ThrowsException()
    {
        _bl.Delivery.Read(99999);
    }

    [TestMethod]
    public void Test_ReadAllDeliveries_ReturnsFilteredData()
    {
        var order = CreateAndGetTestOrder("List Test");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        var all = _bl.Delivery.ReadAll();
        Assert.IsTrue(all.Any(d => d.CourierId == TEST_COURIER_ID));
    }

    #endregion

    #region Pickup Edge Cases

    [TestMethod]
    public void Test_PickUp_Success()
    {
        var order = CreateAndGetTestOrder("Pickup Success");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        var delivery = _bl.Delivery.GetDeliveryByCourier(TEST_COURIER_ID);
        Assert.IsNotNull(delivery);
        Assert.AreEqual(order.Id, delivery.OrderId);
    }

    [TestMethod]
    [ExpectedException(typeof(BlOrderAlreadyAssignedException))]
    public void Test_PickUp_OrderAlreadyInProgress_ThrowsException()
    {
        var order = CreateAndGetTestOrder("Double Pickup");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        int courier2 = 666666666;
        CreateSecondaryCourier(courier2, "Second Courier", true);
        _bl.Delivery.PickUp(courier2, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierAlreadyHasDeliveryException))]
    public void Test_PickUp_CourierBusy_ThrowsException()
    {
        var order1 = CreateAndGetTestOrder("Order One");
        var order2 = CreateAndGetTestOrder("Order Two");

        _bl.Delivery.PickUp(TEST_COURIER_ID, order1.Id);
        _bl.Delivery.PickUp(TEST_COURIER_ID, order2.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_PickUp_OrderTooFar_ThrowsException()
    {
        // Restrict courier range to 100 meters
        var courier = _bl.Courier.Read(TEST_COURIER_ID);
        courier.PersonalMaxDeliveryDistance = 0.1;
        _bl.Courier.Update(courier);

        var order = CreateAndGetTestOrder("Far Away", "Test Far Address");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_PickUp_CourierInactive_ThrowsException()
    {
        int inactiveId = 777777777;
        CreateSecondaryCourier(inactiveId, "Inactive User", false);
        var order = CreateAndGetTestOrder("Inactive Test");

        _bl.Delivery.PickUp(inactiveId, order.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDeliveryAlreadyClosedException))]
    public void Test_PickUp_OrderAlreadyDelivered_ThrowsException()
    {
        var order = CreateAndGetTestOrder("Closed Test");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);
        _bl.Delivery.Deliver(TEST_COURIER_ID, DeliveryEndTypes.Delivered);

        int courier2 = 666666666;
        CreateSecondaryCourier(courier2, "Later Courier", true);
        _bl.Delivery.PickUp(courier2, order.Id);
    }

    #endregion

    #region Finalization Logic

    [TestMethod]
    public void Test_Deliver_StatusRevertsToOpen_OnFailedDelivery()
    {
        var order = CreateAndGetTestOrder("Failed Logic");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        // RecipientNotFound should move the order status back to Open
        _bl.Delivery.Deliver(TEST_COURIER_ID, DeliveryEndTypes.RecipientNotFound);

        var updatedOrder = _bl.Order.Read(order.Id);
        Assert.AreEqual(OrderStatus.Open, updatedOrder.OrderStatus);
    }

    [TestMethod]
    public void Test_Deliver_UpdatesOrderStatus_ToRefused()
    {
        var order = CreateAndGetTestOrder("Refusal Logic");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        _bl.Delivery.Deliver(TEST_COURIER_ID, DeliveryEndTypes.CustomerRefused);

        var updatedOrder = _bl.Order.Read(order.Id);
        Assert.AreEqual(OrderStatus.Refused, updatedOrder.OrderStatus);
    }

    [TestMethod]
    [ExpectedException(typeof(BlCourierHasNoActiveDeliveryException))]
    public void Test_Deliver_NoActiveDelivery_ThrowsException()
    {
        _bl.Delivery.Deliver(TEST_COURIER_ID, DeliveryEndTypes.Delivered);
    }

    #endregion

    #region Helper Logic Tests

    [TestMethod]
    public void Test_IsOrderTaken_ReturnsTrueForInProgress()
    {
        var order = CreateAndGetTestOrder("Taken Test");
        _bl.Delivery.PickUp(TEST_COURIER_ID, order.Id);

        Assert.IsTrue(DeliveryManager.IsOrderTaken(order.Id));
    }

    [TestMethod]
    public void Test_GetDeliveryByCourier_ReturnsNullIfIdle()
    {
        Assert.IsNull(_bl.Delivery.GetDeliveryByCourier(TEST_COURIER_ID));
    }

    #endregion

    #region Private Helpers

    private Order CreateAndGetTestOrder(string customerName, string address = "Hebron Road (central), Jerusalem")
    {
        string validName = customerName.Contains(" ") ? customerName : $"{customerName} Test";
        var newOrder = new Order
        {
            CustomerFullName = validName,
            CustomerMobile = "0501112222",
            FullOrderAddress = address, // Matched in SeedCoordinateCache
            OrderType = OrderTypes.Pizza,
            VerbalDescription = "Valid Description",
            Weight = 5,
            Volume = 2,
            Height = 10,
            Width = 10,
            Fragile = false,
            OrderOpenTime = _bl.Admin.GetClock()
        };
        _bl.Order.Create(newOrder);
        return OrderManager.ReadAll().First(o => o.CustomerFullName == validName);
    }

    private void CreateSecondaryCourier(int id, string name, bool active)
    {
        DalApi.Factory.Get.Courier.Create(new DO.Courier(
            Id: id, FullName: name, MobilePhone: "0552223333", Email: $"c{id}@test.com",
            Password: "password", Active: active, DeliveryType: DO.DeliveryTypes.Bicycle,
            EmploymentStartTime: _bl.Admin.GetClock(), PersonalMaxDeliveryDistance: 1000.0
        ));
    }

    [TestMethod]
    public void PeriodicDeliveriesUpdate_ClosesOverdueDelivery_WithRandomStatus()
    {
        var _dal = DalApi.Factory.Get;

        // Arrange
        // 1. Setup Config for deterministic calculation
        var config = _dal.Config;
        config.RiskRange = TimeSpan.FromMinutes(10);
        config.AvgCarSpeedKmh = 60;
        config.Latitude = 31.766509; // Hebron Road (central), Jerusalem
        config.Longitude = 35.225938;
        config.Clock = DateTime.Now;

        // 2. Create Valid Courier (9 digits ID, valid phone, etc.)
        int courierId = 123456789;
        var courier = new DO.Courier(
            Id: courierId,
            FullName: "Test Courier",
            MobilePhone: "0501234567",
            Email: "test@courier.com",
            Password: "password",
            Active: true,
            DeliveryType: DO.DeliveryTypes.Car,
            EmploymentStartTime: DateTime.Now.AddYears(-1),
            PersonalMaxDeliveryDistance: 100
        );
        _dal.Courier.Create(courier);

        // 3. Create Valid Order
        // Address "Ben Yehuda Street, Jerusalem" is in SeedCoordinateCache (31.781500, 35.217600)
        var order = new DO.Order(
            Id: 0, // DAL assigns
            OrderType: DO.OrderTypes.Pizza,
            VerbalDescription: "Test Order",
            Latitude: 31.781500,
            Longitude: 35.217600,
            CustomerFullName: "Test Customer",
            CustomerMobile: "0509876543",
            Volume: 1,
            Weight: 1,
            Fragile: false,
            Height: 1,
            Width: 1,
            OrderOpenTime: DateTime.Now.AddHours(-2),
            FullOrderAddress: "Ben Yehuda Street, Jerusalem"
        );
        _dal.Order.Create(order);
        int orderId = _dal.Order.ReadAll(x => x.CustomerFullName == "Test Customer" && x.FullOrderAddress == "Ben Yehuda Street, Jerusalem").First().Id;

        // 4. Create Active Delivery (Started 1 hour ago)
        var deliveryStartTime = DateTime.Now.AddHours(-1);
        var delivery = new DO.Delivery(
            Id: 0,
            OrderId: orderId,
            CourierId: courierId,
            DeliveryType: DO.DeliveryTypes.Car,
            DeliveryStartTime: deliveryStartTime,
            ActualDistance: 2.0,
            DeliveryEndType: null,
            DeliveryEndTime: null
        );
        _dal.Delivery.Create(delivery);
        int deliveryId = _dal.Delivery.ReadAll(x => x.OrderId == orderId && x.CourierId == courierId).First().Id;

        // Calculate Threshold:
        // Distance ~1.8km. Speed 60km/h => ~1.8 mins travel time.
        // Expected Arrival = StartTime + ~2 mins.
        // Risk Threshold = RiskRange / 2 = 5 mins.
        // Trigger Time = Expected Arrival + 5 mins = StartTime + ~7 mins.
        // Current time (StartTime + 60 mins) is well past Trigger Time.

        var oldClock = DateTime.Now;
        var newClock = DateTime.Now.AddMinutes(10);

        // Act
        DeliveryManager.PeriodicDeliveriesUpdate(oldClock, newClock);

        // Assert
        var updatedDelivery = _dal.Delivery.Read(deliveryId);
        
        Assert.IsNotNull(updatedDelivery, "Delivery should exist.");
        Assert.IsNotNull(updatedDelivery.DeliveryEndTime, "Delivery should be closed (EndTime not null).");
        Assert.AreEqual(newClock, updatedDelivery.DeliveryEndTime, "Delivery EndTime should match the new clock.");
        Assert.IsNotNull(updatedDelivery.DeliveryEndType, "Delivery EndType should be set.");
        
        // Verify EndType is a valid enum value
        Assert.IsTrue(Enum.IsDefined(typeof(DO.DeliveryEndTypes), updatedDelivery.DeliveryEndType), 
            $"EndType {updatedDelivery.DeliveryEndType} should be a valid enum value.");
    }

    #endregion
}