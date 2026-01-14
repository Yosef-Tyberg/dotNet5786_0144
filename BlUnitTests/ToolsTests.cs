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
        // Driving distance should be at least the aerial distance
        double aerial = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        double driving = Tools.GetDrivingDistance(Lat1, Lon1, Lat2, Lon2);
        
        Assert.IsTrue(driving >= aerial, "Driving distance should be >= aerial distance.");
    }

    [TestMethod]
    public void Test_GetWalkingDistance_ReturnsValidValue()
    {
        // Walking distance should be at least the aerial distance
        double aerial = Tools.GetAerialDistance(Lat1, Lon1, Lat2, Lon2);
        double walking = Tools.GetWalkingDistance(Lat1, Lon1, Lat2, Lon2);
        
        Assert.IsTrue(walking >= aerial, "Walking distance should be >= aerial distance.");
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

    #endregion
}
