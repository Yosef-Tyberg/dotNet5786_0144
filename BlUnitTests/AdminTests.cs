using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using BlApi;
using System;
using System.Linq;

namespace BlUnitTests;

[TestClass]
public class AdminTests
{
    private readonly IBl _bl = Factory.Get();

    [TestInitialize]
    public void TestInitialize()
    {
        // Reset DB before every test to ensure isolation
        AdminManager.InitializeDB();
    }

    // Stage 1: Initialization and Reset

    [TestMethod]
    public void Test_InitializeDB_InitializesData()
    {
        // Arrange & Act
        AdminManager.InitializeDB();

        // Assert
        var couriers = CourierManager.ReadAll();
        Assert.IsTrue(couriers.Any(), "InitializeDB should populate the database with some data.");
    }

    [TestMethod]
    public void Test_ResetDB_ClearsData()
    {
        // Arrange
        AdminManager.InitializeDB(); // Ensure there is data
        Assert.IsTrue(CourierManager.ReadAll().Any(), "Pre-condition failed: DB should have data before reset.");

        // Act
        AdminManager.ResetDB();

        // Assert
        var couriers = CourierManager.ReadAll();
        Assert.IsFalse(couriers.Any(), "ResetDB should clear all data.");
    }

    // Stage 2: Clock Management

    [TestMethod]
    public void Test_GetClock_ReturnsCurrentTime()
    {
        // Arrange
        var initialTime = AdminManager.Now;

        // Act
        System.Threading.Thread.Sleep(10); // Wait a moment
        var laterTime = AdminManager.Now;

        // Assert
        Assert.AreEqual(initialTime, laterTime, "Clock should not advance on its own without UpdateClock.");
    }

    [TestMethod]
    public void Test_UpdateClock_UpdatesTime()
    {
        // Arrange
        var initialTime = AdminManager.Now;
        var newTime = initialTime.AddHours(1);

        // Act
        AdminManager.UpdateClock(newTime);

        // Assert
        Assert.AreEqual(newTime, AdminManager.Now, "UpdateClock should set the clock to the new time.");
    }

    [TestMethod]
    public void Test_ForwardClock_UpdatesTimeCorrectly()
    {
        // Arrange
        var initialTime = _bl.Admin.GetClock();
        var timeToAdd = TimeSpan.FromMinutes(90);

        // Act
        _bl.Admin.ForwardClock(timeToAdd);
        var newTime = _bl.Admin.GetClock();

        // Assert
        Assert.AreEqual(initialTime.Add(timeToAdd), newTime, "ForwardClock should advance the clock by the given TimeSpan.");
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_ForwardClock_NegativeTimeSpan_ThrowsException()
    {
        // Arrange
        var timeToSubtract = TimeSpan.FromMinutes(-30);

        // Act
        _bl.Admin.ForwardClock(timeToSubtract); // Should throw BlInvalidInputException
    }

    // Stage 3 & 4: Configuration Management

    [TestMethod]
    public void Test_GetConfig_ReturnsValidData()
    {
        // Act
        var config = AdminManager.GetConfig();

        // Assert
        Assert.IsNotNull(config);
        Assert.IsTrue(config.AdminId <= 0, "Admin ID must be positive.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(config.AdminPassword), "Default Admin Password should not be empty");
        Assert.IsTrue(config.AvgCarSpeedKmh > 0, "Default car speed should be positive");
        Assert.IsTrue(config.AvgMotorcycleSpeedKmh > 0, "Default motorcycle speed should be positive");
        Assert.IsTrue(config.AvgBicycleSpeedKmh > 0, "Default bicycle speed should be positive");
        Assert.IsTrue(config.AvgWalkingSpeedKmh > 0, "Default walking speed should be positive");
        Assert.IsTrue(config.MaxDeliveryTimeSpan > TimeSpan.Zero, "Default max delivery time span should be positive");
    }

    [TestMethod]
    public void Test_SetConfig_ValidConfig_UpdatesSuccessfully()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminId = 123456789;
        config.AvgCarSpeedKmh = 120.5;
        config.AdminPassword = "newPassword123";

        // Act
        AdminManager.SetConfig(config);
        var updatedConfig = AdminManager.GetConfig();

        // Assert
        Assert.AreEqual(120.5, updatedConfig.AvgCarSpeedKmh, "SetConfig should update the car speed.");
        Assert.AreEqual("newPassword123", updatedConfig.AdminPassword, "SetConfig should update the admin password.");
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_InvalidAdminId_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminId = 0;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_EmptyAdminPassword_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminPassword = "  ";

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NegativeCarSpeed_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AvgCarSpeedKmh = -10;

        // Act
        AdminManager.SetConfig(config);
    }
    
    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NegativeMotorcycleSpeed_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AvgMotorcycleSpeedKmh = -10;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NegativeBicycleSpeed_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AvgBicycleSpeedKmh = -5;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NegativeWalkingSpeed_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AvgWalkingSpeedKmh = -5;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NegativeMaxDeliveryDistance_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.MaxGeneralDeliveryDistanceKm = -100;

        // Act
        AdminManager.SetConfig(config);
    }



    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_ZeroMaxDeliveryTimeSpan_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.MaxDeliveryTimeSpan = TimeSpan.Zero;

        // Act
        AdminManager.SetConfig(config);
    }
    
    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_ZeroRiskRange_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.RiskRange = TimeSpan.Zero;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_ZeroInactivityRange_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.InactivityRange = TimeSpan.Zero;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidAddressException))]
    public void Test_SetConfig_EmptyCompanyAddress_ThrowsBlInvalidAddressException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminId = 111111111;
        config.CompanyFullAddress = ""; // Invalid address

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NullConfigObject_ThrowsException()
    {
        // Act
        AdminManager.SetConfig(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidInputException))]
    public void Test_SetConfig_NullAdminPassword_ThrowsException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminPassword = null!;

        // Act
        AdminManager.SetConfig(config);
    }

    [TestMethod]
    [ExpectedException(typeof(BO.BlInvalidAddressException))]
    public void Test_SetConfig_NullCompanyAddress_ThrowsBlInvalidAddressException()
    {
        // Arrange
        var config = AdminManager.GetConfig();
        config.AdminId = 111111111;
        config.CompanyFullAddress = null;

        // Act
        AdminManager.SetConfig(config);
    }
}
