using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BO;
using DO;

namespace Helpers;

/// <summary>
/// Utility helper methods for the Business Layer.
/// Provides exception wrapping, conversion utilities, and reflection-based string formatting for DAL and BO operations.
/// </summary>
internal static class Tools
{
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
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

    #endregion
}
