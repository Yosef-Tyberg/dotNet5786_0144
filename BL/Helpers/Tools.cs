using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BO;
using DO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Helpers;

/// <summary>
/// Utility helper methods for the Business Layer.
/// Provides exception wrapping, conversion utilities, and reflection-based string formatting for DAL and BO operations.
/// </summary>
internal static class Tools
{
    private static readonly HttpClient s_httpClient = new();

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
                return (double.Parse(bestResult.Lat), double.Parse(bestResult.Lon));
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
                return result.Routes[0].Distance / 1000; // Convert meters to kilometers
            }
        }
        catch (Exception ex)
        {
            throw new BlInvalidInputException("Error getting distance", ex);
        }
        throw new BlInvalidInputException("could not find a route");
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
