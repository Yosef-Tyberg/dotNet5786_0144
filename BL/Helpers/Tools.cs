using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BO;
using DO;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Helpers;

/// <summary>
/// Utility helper methods for the Business Layer.
/// Provides exception wrapping, conversion utilities, and reflection-based string formatting for DAL and BO operations.
/// </summary>
internal static class Tools
{
    private static readonly HttpClient s_httpClient = new();
    private static readonly ConcurrentDictionary<string, (double, double)> s_coordinateCache = new();
    private static readonly ConcurrentDictionary<string, double> s_routeDistanceCache = new();

    private static DalApi.IDal s_dal = DalApi.Factory.Get;

    #region Validation Methods

    public static void ValidateFullName(string name, string label)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BO.BlInvalidInputException($"{label} cannot be empty.");

        if (name.Length > 60)
            throw new BO.BlInvalidInputException($"{label} is too long.");

        if (!Regex.IsMatch(name, @"^[A-Za-zא-ת ]+$"))
            throw new BO.BlInvalidInputException($"{label} may contain only English or Hebrew letters and spaces.");

        bool hasEnglish = Regex.IsMatch(name, @"[A-Za-z]");
        bool hasHebrew = Regex.IsMatch(name, @"[א-ת]");

        if (hasEnglish && hasHebrew)
            throw new BO.BlInvalidInputException($"{label} cannot contain both English and Hebrew characters.");

        if (name.Contains("    "))
            throw new BO.BlInvalidInputException($"{label} cannot contain more than three consecutive spaces.");

        if (!Regex.IsMatch(name, @"^ {0,3}[A-Za-zא-ת]+( {1,3}[A-Za-zא-ת]+)+ {0,3}$"))
            throw new BO.BlInvalidInputException($"{label} must contain at least two words separated by spaces.");
    }

    public static void ValidatePhoneNumber(string phone, string label)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new BO.BlInvalidInputException($"{label} cannot be empty.");

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
            throw new BO.BlInvalidInputException($"{label} must be exactly 10 digits and start with 0.");
    }

    public static void ValidateEmail(string email, string label)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BO.BlInvalidInputException($"{label} cannot be empty.");

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BO.BlInvalidEmailException($"{label} '{email}' is not a valid format.");
    }

    #endregion

    #region Exception Conversion Methods

    /// <summary>
    /// Converts a DAL exception to an appropriate BO exception, preserving the original exception as inner exception.
    /// </summary>
    /// <param name="dalException">The DAL exception to convert.</param>
    /// <param name="operationName">Description of the operation for error messaging.</param>
    /// <returns>An appropriate BO exception.</returns>
    internal static Exception ConvertDalException(Exception dalException)
    {
        return dalException switch
        {
            DalDoesNotExistException ex => new BlDoesNotExistException($"{ex.Message}", ex),
            DalAlreadyExistsException ex => new BlAlreadyExistsException($"{ex.Message}", ex),
            DalInvalidInputException ex => new BlInvalidInputException($"{ex.Message}", ex),
            _ => new BlInvalidInputException($"{dalException.Message}", dalException)
        };
    }

    #endregion Exception Conversion Methods

    #region String Formatting Methods

    /// <summary>
    /// Generic extension method that generates a string representation of any object using reflection.
    /// Traverses all public properties and includes nested BO entities and collections.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert to string.</typeparam>
    /// <param name="obj">The object to convert to string.</param>
    /// <returns>A formatted string representation of the object and its properties.</returns>
    public static string ToStringProperty<T>(this T obj)
    {
        if (obj == null)
            return "null";

        StringBuilder sb = new StringBuilder();
        Type objType = obj.GetType();

        // Start with class name
        sb.AppendLine($"{objType.Name}:");
        sb.AppendLine("{");

        // Retrieve all public properties and filter out indexers and write-only properties
        PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var validProperties = from prop in properties
                              where prop.GetGetMethod() != null && prop.GetIndexParameters().Length == 0
                              select prop;

        // Use LINQ Aggregate to build property lines instead of foreach
        var propertyLines = from prop in validProperties
                            select FormatPropertyLine(prop, obj);

        string allPropertyLines = string.Join(Environment.NewLine, propertyLines);
        if (!string.IsNullOrEmpty(allPropertyLines))
        {
            sb.Append(allPropertyLines).AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Formats a single property line for display, handling exceptions gracefully.
    /// </summary>
    /// <param name="prop">The property to format.</param>
    /// <param name="obj">The object containing the property.</param>
    /// <returns>A formatted property line.</returns>
    private static string FormatPropertyLine(PropertyInfo prop, object obj)
    {
        try
        {
            object? value = prop.GetValue(obj);
            string propertyValue = FormatPropertyValue(value);
            return $"  {prop.Name}: {propertyValue}";
        }
        catch
        {
            return $"  {prop.Name}: [Error reading property]";
        }
    }

    /// <summary>
    /// Formats a property value for display, handling null, collection, and nested object cases.
    /// </summary>
    /// <param name="value">The property value to format.</param>
    /// <returns>A formatted string representation of the value.</returns>
    private static string FormatPropertyValue(object? value)
    {
        // Handle null values
        if (value == null)
            return "null";

        // Handle string values
        if (value is string str)
            return $"\"{str}\"";

        // Handle collections (List, IEnumerable, etc.)
        if (value is IEnumerable enumerable && !(value is string))
        {
            return FormatCollection(enumerable);
        }

        // Handle BO entities (nested objects)
        if (value.GetType().Namespace == "BO")
        {
            return FormatNestedObject(value);
        }

        // Handle primitive types and other types
        return value.ToString() ?? "null";
    }

    /// <summary>
    /// Formats a collection/list for display using LINQ query syntax.
    /// </summary>
    /// <param name="enumerable">The collection to format.</param>
    /// <returns>A formatted string representation of the collection.</returns>
    private static string FormatCollection(IEnumerable enumerable)
    {
        var formattedItems = from object? item in enumerable
                             select item switch
                             {
                                 null => "null",
                                 string str => $"\"{str}\"",
                                 _ when item.GetType().Namespace == "BO"
                                     => $"{item.GetType().Name}(Id={GetIdProperty(item)})",
                                 _ => item.ToString() ?? "null"
                             };

        string itemsString = string.Join(", ", formattedItems);
        return $"[{itemsString}]";
    }

    /// <summary>
    /// Formats a nested BO object for display, showing only key identifying properties.
    /// </summary>
    /// <param name="obj">The nested BO object to format.</param>
    /// <returns>A formatted string representation of the nested object.</returns>
    private static string FormatNestedObject(object obj)
    {
        if (obj == null)
            return "null";

        string objTypeName = obj.GetType().Name;
        object? idValue = GetIdProperty(obj);

        if (idValue != null)
            return $"{objTypeName}(Id={idValue})";

        return objTypeName;
    }

    /// <summary>
    /// Retrieves the Id property value of an object if it exists.
    /// </summary>
    /// <param name="obj">The object to retrieve the Id from.</param>
    /// <returns>The Id property value, or null if not found.</returns>
    private static object? GetIdProperty(object obj)
    {
        if (obj == null)
            return null;

        PropertyInfo? idProp = obj.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        return idProp?.GetValue(obj);
    }

    #endregion String Formatting Methods

    #region Calculation Methods

    /// <summary>
    /// Calculates the distance between two geographical points using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point.</param>
    /// <param name="lon1">Longitude of the first point.</param>
    /// <param name="lat2">Latitude of the second point.</param>
    /// <param name="lon2">Longitude of the second point.</param>
    /// <returns>The distance in kilometers.</returns>
    public static double GetAerialDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    /// <summary>
    /// gets the coordinate of a given address
    /// </summary>
    /// <param name="address"></param>
    /// <returns>a tuple of two doubles, the first is that latitude and the second is the logitude</returns>
    /// <exception cref="BlInvalidInputException"></exception>
    public static (double, double) GetCoordinates(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new BlInvalidAddressException("address cannot be null or empty");
        }
            
        if (s_coordinateCache.TryGetValue(address, out var coordinates))
        {
            return coordinates;
        }

        try
        {
            // The Nominatim API endpoint for searching a structured query
            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("My-C#-App");
            string json = s_httpClient.GetStringAsync(url).Result;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json, options);

            if (results?.Count > 0)
            {
                var bestResult = results[0];
                var newCoordinates = (double.Parse(bestResult.Lat), double.Parse(bestResult.Lon));
                s_coordinateCache.TryAdd(address, newCoordinates);
                return newCoordinates;
            }
        }
        catch (Exception ex)
        {
            throw new BlInvalidAddressException("Error getting coordinates", ex);
        }
        throw new BlInvalidAddressException("address not found");
    }

    /// <summary>
    /// gets the driving distance between two locations
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns>the driving distance in km</returns>
    /// <exception cref="BlInvalidInputException"></exception>
    public static double GetDrivingDistance(double lat1, double lon1, double lat2, double lon2)
    {
        return GetRouteDistance(lat1, lon1, lat2, lon2, "driving");
    }

    /// <summary>
    /// gets the walking distance between two locations
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns>the walking distance in km</returns>
    /// <exception cref="BlInvalidInputException"></exception>
    public static double GetWalkingDistance(double lat1, double lon1, double lat2, double lon2)
    {
        return GetRouteDistance(lat1, lon1, lat2, lon2, "foot");
    }

    private static double GetRouteDistance(double lat1, double lon1, double lat2, double lon2, string profile)
    {
        string cacheKey = $"{lat1},{lon1};{lat2},{lon2};{profile}";
        if (s_routeDistanceCache.TryGetValue(cacheKey, out double distance))
        {
            return distance;
        }

        try
        {
            string url = $"http://router.project-osrm.org/route/v1/{profile}/{lon1},{lat1};{lon2},{lat2}?overview=false";
            s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("My-C#-App");
            string json = s_httpClient.GetStringAsync(url).Result;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<OsrmResponse>(json, options);

            if (result?.Routes?.Count > 0)
            {
                double newDistance = result.Routes[0].Distance / 1000; // Convert meters to kilometers
                s_routeDistanceCache.TryAdd(cacheKey, newDistance);
                return newDistance;
            }
        }
        catch (Exception ex)
        {
            throw new BlInvalidInputException("Error getting distance", ex);
        }
        throw new BlInvalidInputException("could not find a route");
    }
    #endregion

    #region Schedule Methods

    /// <summary>
    /// Calculates the expected delivery time for a delivery.
    /// </summary>
    /// <param name="deliveryType">The delivery type.</param>
    /// <param name="order">The associated order.</param>
    /// <returns>The calculated expected delivery time.</returns>
    /// <exception cref="BO.BlMissingPropertyException">Thrown when calculation fails.</exception>
    internal static DateTime CalculateExpectedDeliveryTime(BO.DeliveryTypes deliveryType, BO.Order order, BO.Delivery? activeDelivery = null)
    {
        var config = AdminManager.GetConfig();

        // Determine the start time: if an active delivery exists for the order, use its DeliveryStartTime; otherwise, use AdminManager.Now 
        DateTime startTime = activeDelivery?.DeliveryStartTime ?? AdminManager.Now;

        var (distance, speed) = GetDistanceAndSpeed(deliveryType, order, config);

        if (speed >= 1)
        {
            var estimatedHours = distance / speed;
            return startTime.AddHours(estimatedHours);
        }

        throw new BO.BlMissingPropertyException("Average speed must be >= 1 km/h");
    }

    private static (double distance, double speed) GetDistanceAndSpeed(BO.DeliveryTypes deliveryType, BO.Order order, BO.Config config)
    {
        double distance;
        double speed;
        switch (deliveryType)
        {
            case BO.DeliveryTypes.Car:
                distance = Tools.GetDrivingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);
                speed = config.AvgCarSpeedKmh;
                break;
            case BO.DeliveryTypes.Motorcycle:
                distance = Tools.GetDrivingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);
                speed = config.AvgMotorcycleSpeedKmh;
                break;
            case BO.DeliveryTypes.Bicycle:
                distance = Tools.GetWalkingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);
                speed = config.AvgBicycleSpeedKmh;
                break;
            case BO.DeliveryTypes.OnFoot:
                distance = Tools.GetWalkingDistance((double)(config.Latitude ?? 0), (double)(config.Longitude ?? 0), order.Latitude, order.Longitude);
                speed = config.AvgWalkingSpeedKmh;
                break;
            default:
                throw new BO.BlMissingPropertyException($"Could not calculate properties for unrecognized delivery type: {deliveryType}");
        }
        return (distance, speed);
    }

    internal static BO.ScheduleStatus DetermineScheduleStatus(BO.Order order, BO.Delivery? activeDelivery = null)
    {
        var config = AdminManager.GetConfig();
        
        var deliveryType = GetFastestType(config);
        
        if (activeDelivery != null)
        {
            deliveryType = activeDelivery.DeliveryType;
        }
        
        var arrival = CalculateExpectedDeliveryTime(deliveryType, order, activeDelivery);  
        var deadline = order.OrderOpenTime.Add(config.MaxDeliveryTimeSpan);

        if (AdminManager.Now > deadline || arrival > deadline)
            return BO.ScheduleStatus.Late;

        if ((deadline - arrival) < config.RiskRange)
            return BO.ScheduleStatus.AtRisk;

        return BO.ScheduleStatus.OnTime;
    }

    internal static BO.DeliveryTypes GetFastestType(BO.Config config)
    {
        var speeds = new Dictionary<BO.DeliveryTypes, double>
        {
            { BO.DeliveryTypes.Car, config.AvgCarSpeedKmh },
            { BO.DeliveryTypes.Motorcycle, config.AvgMotorcycleSpeedKmh },
            { BO.DeliveryTypes.Bicycle, config.AvgBicycleSpeedKmh },
            { BO.DeliveryTypes.OnFoot, config.AvgWalkingSpeedKmh }
        };

        return speeds.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    }


    #endregion

    #region Helper Classes for JSON Deserialization

    private class NominatimResult
    {
        public string Lat { get; set; } = string.Empty;
        public string Lon { get; set; } = string.Empty;
    }

    private class OsrmResponse
    {
        public List<OsrmRoute> Routes { get; set; } = new();
    }

    private class OsrmRoute
    {
        public double Distance { get; set; }
    }

    #endregion
}
