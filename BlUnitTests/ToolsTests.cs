using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using BlApi;
using BO;
using System;
using System.Linq;

namespace BlUnitTests;

[TestClass]
public class ToolsTests
{
    // Coordinates for testing (approximate Jerusalem coordinates)
    private const double Lat1 = 31.7767;
    private const double Lon1 = 35.2345;
    private const double Lat2 = 31.7800;
    private const double Lon2 = 35.2300;

    private readonly IBl bl = Factory.Get();

    [TestInitialize]
    public void Init()
    {
        AdminManager.InitializeDB();
    }

    #region Distance Tests

    [TestMethod]
    public void Test_GetAerialDistance_SamePoint_ReturnsZero()
    {
        double distance = Tools.GetAerialDistance(Lat1, Lon1, Lat1, Lon1);
        Assert.AreEqual(0, distance, 0.001, "Distance between same points should be 0.");
    }

    [TestMethod]
    public void Test_GetAerialDistance_DifferentPoints_ReturnsPositive()
    {
        double distance = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        Assert.IsTrue(distance > 0, "Distance between different points should be positive.");
    }

    [TestMethod]
    public void Test_GetAerialDistance_Commutative()
    {
        double dist1 = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        double dist2 = Tools.GetAerialDistance(Lat2, Lon2, Lat1, Lon1);
        Assert.AreEqual(dist1, dist2, 0.001, "Distance should be the same regardless of direction.");
    }

    [TestMethod]
    public void Test_GetDrivingDistance_ReturnsValidValue()
    {
        // Driving distance should be at least the aerial distance and within a reasonable range
        double aerial = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        double driving = Tools.GetDrivingDistance(Lat1, Lon1, Lat2, Lon2);
        
        Assert.IsTrue(driving >= aerial, "Driving distance should be >= aerial distance.");
        Assert.IsTrue(driving < aerial * 5, "Driving distance should be within a reasonable range of aerial distance.");
    }

    [TestMethod]
    public void Test_GetWalkingDistance_ReturnsValidValue()
    {
        // Walking distance should be at least the aerial distance and within a reasonable range
        double aerial = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        double walking = Tools.GetWalkingDistance(Lat1, Lon1, Lat2, Lon2);
        
        Assert.IsTrue(walking >= aerial, "Walking distance should be >= aerial distance.");
        Assert.IsTrue(walking < aerial * 7, "Walking distance should be within a reasonable range of aerial distance.");
    }

    [TestMethod]
    public void Test_GetAerialDistance_KnownDistance_ReturnsCorrectValue()
    {
        // Distance calculated using online tool for the given coordinates
        const double expectedDistance = 2.6; // km
        double actualDistance = Tools.GetAerialDistance(Lat1, Lon1, 31.8000, 35.2300);
        Assert.AreEqual(expectedDistance, actualDistance, 0.1, "Distance should be close to the known value.");
    }

    #endregion

    #region Coordinate Tests

    [TestMethod]
    public void Test_GetCoordinates_ValidAddress_ReturnsCoordinates()
    {
        // Assuming the implementation can handle a dummy or real address.
        string address = "Jerusalem";
        var (lat, lon) = Tools.GetCoordinates(address);
        
        // Check that we got valid numbers (not NaN) and they are non-zero
        Assert.IsFalse(double.IsNaN(lat));
        Assert.IsFalse(double.IsNaN(lon));
        Assert.IsTrue(lat != 0 || lon != 0);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidAddressException))]
    public void Test_GetCoordinates_NullAddress_ThrowsException()
    {
        Tools.GetCoordinates(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidAddressException))]
    public void Test_GetCoordinates_EmptyAddress_ThrowsException()
    {
        Tools.GetCoordinates("");
    }
    
    [TestMethod]
    [ExpectedException(typeof(BlInvalidAddressException))]
    public void Test_GetCoordinates_WhitespaceAddress_ThrowsException()
    {
        Tools.GetCoordinates("   ");
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidAddressException))]
    public void Test_GetCoordinates_InvalidAddress_ThrowsException()
    {
        Tools.GetCoordinates("asdfasdfasdf");
    }

    #endregion

    #region Schedule & Calculation Tests

    [TestMethod]
    public void Test_GetFastestType_ReturnsCorrectType()
    {
        var config = new BO.Config
        {
            AvgCarSpeedKmh = 50,
            AvgMotorcycleSpeedKmh = 100, // Fastest
            AvgBicycleSpeedKmh = 20,
            AvgWalkingSpeedKmh = 5
        };

        var result = Tools.GetFastestType(config);
        Assert.AreEqual(DeliveryTypes.Motorcycle, result);
    }

    [TestMethod]
    public void Test_CalculateExpectedDeliveryTime_ReturnsValidTime()
    {
        var config = AdminManager.GetConfig();
        var order = new BO.Order
        {
            Id = 1,
            Latitude = (config.Latitude ?? 31.7) + 0.01,
            Longitude = (config.Longitude ?? 35.2) + 0.01,
            OrderOpenTime = AdminManager.Now
        };

        var result = Tools.CalculateExpectedDeliveryTime(DeliveryTypes.Car, order);
        Assert.IsTrue(result >= AdminManager.Now);
    }

    [TestMethod]
    public void Test_DetermineScheduleStatus_ReturnsOnTime_ForNewOrder()
    {
        var order = new BO.Order { Id = 0, CustomerFullName = "Test111", CustomerMobile = "0500000000", FullOrderAddress = "Addr", VerbalDescription = "Desc", Volume = 1, Weight = 1, Height = 1, Width = 1, OrderOpenTime = AdminManager.Now };
        bl.Order.Create(order);
        var id = OrderManager.ReadAll(o => o.CustomerFullName == "Test111").First().Id; 
        var status = Tools.DetermineScheduleStatus(id);
        Assert.AreEqual(ScheduleStatus.OnTime, status);
    }

    [TestMethod]
    public void Test_ScheduleStatus_ChangesOverTime()
    {
        
        // Arrange: Setup - Pick up an order
        var courier = bl.Courier.ReadAll(c => c.Active && c.Id != 0 && bl.Delivery.GetDeliveryByCourier(c.Id) == null).First();
        var order = bl.Order.ReadAll(o => o.OrderStatus == OrderStatus.Open).First();
        bl.Delivery.PickUp(courier.Id, order.Id);
        
        // Assert 1: Initially OnTime
        Assert.AreEqual(ScheduleStatus.OnTime, Tools.DetermineScheduleStatus(order.Id));
        
        // Act 2: Forward clock
        var config = AdminManager.GetConfig();
        var orderObj = bl.Order.Read(order.Id);
        var maxTime = orderObj.OrderOpenTime + config.MaxDeliveryTimeSpan;
        
        var timeToMaximum = maxTime - AdminManager.Now;
        var timeToForward = timeToMaximum - (config.RiskRange / 2); 
        
        bl.Admin.ForwardClock(timeToForward);
        
        // Assert 2: AtRisk
        Assert.AreEqual(ScheduleStatus.AtRisk, Tools.DetermineScheduleStatus(order.Id));
        
        // Act 3: Forward past max
        bl.Admin.ForwardClock(config.RiskRange);
        
        // Assert 3: Late
        Assert.AreEqual(ScheduleStatus.Late, Tools.DetermineScheduleStatus(order.Id));
    }

    #endregion
}
