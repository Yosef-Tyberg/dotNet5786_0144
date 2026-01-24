using System.Globalization;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// A WPF ValidationRule that checks if the input string can be parsed as a Double.
/// This runs on the raw text from the TextBox *before* the Converter is called.
/// </summary>
public class DoubleValidationRule : ValidationRule
{
    /// <summary>
    /// Determines if an empty string is considered valid.
    /// Set to true for nullable fields (e.g., MaxGeneralDeliveryDistanceKm).
    /// </summary>
    public bool IsNullable { get; set; } = false;

    /// <summary>
    /// The name of the field being validated, used in error messages.
    /// </summary>
    public string FieldName { get; set; } = "Field";

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var s = value as string;
        
        // Check for empty input
        if (string.IsNullOrWhiteSpace(s))
        {
            return IsNullable 
                ? ValidationResult.ValidResult 
                : new ValidationResult(false, $"{FieldName} cannot be empty.");
        }

        // Attempt to parse. We use InvariantCulture to match the Converter's logic.
        // If parsing fails, we return a failure result which WPF uses to highlight the control (red border).
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, $"{FieldName}: Input must be a valid number.");
    }
}

/// <summary>
/// A WPF ValidationRule that checks if the input string can be parsed as an Integer.
/// </summary>
public class IntegerValidationRule : ValidationRule
{
    /// <summary>
    /// Determines if an empty string is considered valid.
    /// </summary>
    public bool IsNullable { get; set; } = false;

    /// <summary>
    /// The name of the field being validated, used in error messages.
    /// </summary>
    public string FieldName { get; set; } = "Field";

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var s = value as string;
        
        if (string.IsNullOrWhiteSpace(s))
        {
            return IsNullable 
                ? ValidationResult.ValidResult 
                : new ValidationResult(false, $"{FieldName} cannot be empty.");
        }

        // Check if it's a valid integer
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return ValidationResult.ValidResult;

        return new ValidationResult(false, $"{FieldName}: Input must be a valid integer.");
    }
}