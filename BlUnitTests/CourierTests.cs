using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using BlApi;
using BO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BlUnitTests;

[TestClass]
public class CourierTests
{
    private readonly IBl _bl = Factory.Get();

    [TestInitialize]
    public void TestInitialize()
    {
        // Reset and initialize DB before every test to ensure isolation
        AdminManager.InitializeDB();
    }

    #region Create Tests

    [TestMethod]
    public void Test_CreateCourier_Success()
    {
        // Arrange
        var newCourier = new Courier
        {
            Id = 111222333,
            FullName = "New Courier",
            MobilePhone = "055-111-1111",
            Email = "new@courier.com",
            Password = "securepassword",
            Active = true,
            DeliveryType = DeliveryTypes.Car,
            EmploymentStartTime = AdminManager.Now,
            PersonalMaxDeliveryDistance = 50
        };

        // Act
        _bl.Courier.Create(newCourier);
        var createdCourier = _bl.Courier.Read(newCourier.Id);

        // Assert
        Assert.IsNotNull(createdCourier);
        Assert.AreEqual("New Courier", createdCourier.FullName);
    }

    [TestMethod]
    [ExpectedException(typeof(BlAlreadyExistsException))]
    public void Test_CreateCourier_AlreadyExists_ThrowsException()
    {
        // Arrange
        var existingCourier = CourierManager.ReadAll().First();
        var newCourierWithSameId = new Courier
        {
            Id = existingCourier.Id,
            FullName = "Duplicate Courier",
            MobilePhone = "055-222-2222",
            Email = "duplicate@courier.com",
            Password = "password",
            Active = true,
            DeliveryType = DeliveryTypes.OnFoot,
            EmploymentStartTime = AdminManager.Now
        };

        // Act
        _bl.Courier.Create(newCourierWithSameId);
    }

    [TestMethod]
    [ExpectedException(typeof(BlInvalidInputException))]
    public void Test_CreateCourier_InvalidData_ThrowsException()
    {
        // Arrange
        var invalidCourier = new Courier
        {
            Id = 999888777,
            FullName = "", // Invalid
            MobilePhone = "055-333-3333",
            Email = "invalid@courier.com",
            Password = "password",
            Active = true,
            DeliveryType = DeliveryTypes.Bicycle,
            EmploymentStartTime = AdminManager.Now
        };

        // Act
        _bl.Courier.Create(invalidCourier);
    }

    #endregion

    #region Read Tests

    [TestMethod]
    public void Test_ReadCourier_Success()
    {
        // Arrange
        var courierToRead = CourierManager.ReadAll().First();

        // Act
        var readCourier = _bl.Courier.Read(courierToRead.Id);

        // Assert
        Assert.IsNotNull(readCourier);
        Assert.AreEqual(courierToRead.Id, readCourier.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_ReadCourier_NotFound_ThrowsException()
    {
        // Act
        _bl.Courier.Read(12345); // Non-existent ID
    }

    [TestMethod]
    public void Test_ReadAllCouriers_ReturnsData()
    {
        // Act
        var couriers = _bl.Courier.ReadAll();

        // Assert
        Assert.IsTrue(couriers.Any(), "ReadAll should return a list of couriers.");
    }

    #endregion

    #region Update Tests

    [TestMethod]
    public void Test_UpdateCourier_Success()
    {
        // Arrange
        var courierToUpdate = _bl.Courier.Read(CourierManager.ReadAll().First().Id);
        courierToUpdate.FullName = "Updated Name";
        courierToUpdate.Email = "updated@email.com";

        // Act
        _bl.Courier.Update(courierToUpdate);
        var updatedCourier = _bl.Courier.Read(courierToUpdate.Id);

        // Assert
        Assert.AreEqual("Updated Name", updatedCourier.FullName);
        Assert.AreEqual("updated@email.com", updatedCourier.Email);
    }
    
    [TestMethod]
    public void Test_UpdateMyDetails_Success()
    {
        // Arrange
        var courierToUpdate = _bl.Courier.Read(CourierManager.ReadAll().First().Id);
        
        // Act
        _bl.Courier.UpdateMyDetails(courierToUpdate.Id, fullName: "My New Name", phoneNum: "050-987-6543");
        var updatedCourier = _bl.Courier.Read(courierToUpdate.Id);

        // Assert
        Assert.AreEqual("My New Name", updatedCourier.FullName);
        Assert.AreEqual("050-987-6543", updatedCourier.MobilePhone);
    }


    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_UpdateCourier_NotFound_ThrowsException()
    {
        // Arrange
        var nonExistentCourier = new Courier { Id = 12345, FullName = "Ghost" };

        // Act
        _bl.Courier.Update(nonExistentCourier);
    }

    #endregion

    #region Delete Tests

    [TestMethod]
    public void Test_DeleteCourier_Success()
    {
        // Arrange
        var courierToDelete = CourierManager.ReadAll().First();
        
        // Act
        _bl.Courier.Delete(courierToDelete.Id);

        // Assert
        Assert.ThrowsException<BlDoesNotExistException>(() => _bl.Courier.Read(courierToDelete.Id));
    }

    [TestMethod]
    [ExpectedException(typeof(BlDoesNotExistException))]
    public void Test_DeleteCourier_NotFound_ThrowsException()
    {
        // Act
        _bl.Courier.Delete(12345); // Non-existent ID
    }

    #endregion

    #region Specific Logic Tests

    [TestMethod]
    public void Test_GetCourierDeliveryHistory_ReturnsHistory()
    {
        // This is a complex test that requires creating orders and deliveries.
        // For a true unit test, this would involve mocking the Dal layer.
        // In this integration-style test, we rely on the DB being initialized.
        
        // Arrange: Find a courier and check they have history (the init DB should provide this)
        var courier = CourierManager.ReadAll().First(c => _bl.Courier.GetCourierDeliveryHistory(c.Id).Any());
        
        // Act
        var history = _bl.Courier.GetCourierDeliveryHistory(courier.Id);

        // Assert
        Assert.IsTrue(history.Any());
        Assert.IsTrue(history.All(d => d.CourierId == courier.Id));
    }

    [TestMethod]
    public void Test_GetCourierStatistics_CalculatesCorrectly()
    {
        // Arrange: Find a courier with history
        var courier = CourierManager.ReadAll().First(c => _bl.Courier.GetCourierDeliveryHistory(c.Id).Any());

        // Act
        var stats = _bl.Courier.GetCourierStatistics(courier.Id);

        // Assert - based on initial data, we expect some stats.
        Assert.IsTrue(stats.TotalDeliveries > 0);
        Assert.IsTrue(stats.TotalDistance > 0);
        Assert.IsTrue(stats.SuccessRate > 0);
    }
    
    [TestMethod]
    public void Test_GetOpenOrders_ReturnsAvailableOrders()
    {
        // Arrange: Find a courier, ensure they have a max distance set.
        var courier = _bl.Courier.Read(CourierManager.ReadAll().First().Id);
        courier.PersonalMaxDeliveryDistance = 1000; // Set a large distance
        _bl.Courier.Update(courier);

        // Act
        var openOrders = _bl.Courier.GetOpenOrders(courier.Id);

        // Assert
        Assert.IsTrue(openOrders.Any(), "With a large max distance, courier should see open orders from init data.");
        Assert.IsTrue(openOrders.All(o => o.OrderStatus == OrderStatus.Open));
    }

    [TestMethod]
    public void Test_GetOpenOrders_NoDistanceSet_ReturnsEmpty()
    {
        // Arrange: Find a courier and ensure they have no max distance.
        var courier = _bl.Courier.Read(CourierManager.ReadAll().First().Id);
        courier.PersonalMaxDeliveryDistance = null;
        _bl.Courier.Update(courier);
        
        // Act
        var openOrders = _bl.Courier.GetOpenOrders(courier.Id);

        // Assert
        Assert.IsFalse(openOrders.Any(), "Courier with no max distance should not see any open orders.");
    }

    #endregion
}
