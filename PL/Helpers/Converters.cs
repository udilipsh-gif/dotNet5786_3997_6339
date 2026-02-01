using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PL;

/// <summary>
/// Converts an Enum value to a boolean based on a parameter match.
/// Typically used for binding multiple RadioButtons to a single Enum property.
/// </summary>
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        string checkValue = value.ToString()!;
        string targetValue = parameter.ToString()!;

        return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Binding.DoNothing;

        bool useValue = (bool)value;
        string targetValue = parameter.ToString()!;

        if (useValue)
        {
            return Enum.Parse(targetType, targetValue);
        }

        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts a null value or an empty collection to Visibility.Collapsed (hidden).
/// Supports an "Invert" parameter to reverse the logic.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNullOrEmpty = value == null;

        // Check if it's an empty collection
        if (!isNullOrEmpty && value is ICollection collection)
        {
            isNullOrEmpty = collection.Count == 0;
        }

        // Handle Inversion (Show if null)
        if (parameter != null && parameter.ToString() == "Invert")
        {
            return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        // Default: Hide if null
        return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to Visibility.
/// True becomes Visible, False becomes Collapsed.
/// Supports an "Invert" parameter to reverse the logic.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (parameter != null && parameter.ToString() == "Invert")
            {
                // Inverted: True -> Collapsed
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            // Default: True -> Visible
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Returns true if the value is null, otherwise false.
/// </summary>
public class NullToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to a Hebrew string ("פעיל" / "לא פעיל").
/// </summary>
public class BooleanToHebrewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? "פעיל" : "לא פעיל";
        return "לא ידוע";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
            return s == "פעיל";
        return false;
    }
}

/// <summary>
/// Maps technical string values (like "Add", "Update", "true") to user-friendly Hebrew display strings.
/// </summary>
public class ValueToHebrewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string val)
        {
            return val switch
            {
                "Add" => "חדש",
                "Update" => "עדכן",
                "true" => "פעיל",
                "false" => "לא פעיל",
                _ => "לא ידוע"
            };
        }
        return "לא ידוע";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string val)
        {
            return val switch
            {
                "חדש" => "Add",
                "עדכן" => "Update",
                "פעיל" => "true",
                "לא פעיל" => "false",
                _ => "null"
            };
        }
        return "null";
    }
}

/// <summary>
/// Converts an Enum value to its Description attribute text.
/// If no Description attribute exists, returns the Enum name.
/// </summary>
public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var fieldInfo = value.GetType().GetField(value.ToString()!);
        if (fieldInfo == null)
            return value.ToString() ?? string.Empty;

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Extracts the total whole hours from a TimeSpan.
/// </summary>
public class TotalHoursConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            // Returns total hours truncated to int (including days converted to hours)
            return (int)timeSpan.TotalHours;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts between TimeSpan values and their string representation in "days:hours" format for UI binding.
/// </summary>
/// <remarks>
/// This converter enables two-way binding between TimeSpan properties in the business logic
/// and user-friendly string representations in TextBox controls. The format used is "DD:HH"
/// where DD represents days and HH represents hours (both zero-padded to 2 digits).
/// Used for configuration fields like MaxDeliveryTime, RiskRange, and MaxTimeInactivity.
/// </remarks>
public class SpanTimeConverter : IValueConverter
{
    /// <summary>
    /// Converts a TimeSpan value to a string in "DD:HH" format for display.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return $"{ts.Days:00}:{ts.Hours:00}";
        }
        return "00:00";
    }

    /// <summary>
    /// Converts a string in "DD:HH" format back to a TimeSpan value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string input)
        {
            var parts = input.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int days) &&
                int.TryParse(parts[1], out int hours))
            {
                return new TimeSpan(days, hours, 0, 0);
            }
        }
        return TimeSpan.Zero;
    }
}

/// <summary>
/// Formats a TimeSpan into a verbose Hebrew string (e.g., "X days, Y hours...").
/// </summary>
public class TimeSpanToShortStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            // Negative check
            if (timeSpan <= TimeSpan.Zero)
                return "00:00:00";

            // If more than a day, include days in the string
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} ימים, {timeSpan.Hours:D2} שעות ו{timeSpan.Minutes:D2} דקות";

            // Otherwise show hours and minutes
            return $"{(int)timeSpan.TotalHours:D2} שעות ו {timeSpan.Minutes:D2} דקות";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Returns one size (double) if the value is null/empty, and another size if it has value.
/// Used for dynamic layout sizing (e.g., Grid Rows/Columns).
/// </summary>
public class NullToSizeConverter : IValueConverter
{
    public double NullSize { get; set; }
    public double NotNullSize { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check for null
        if (value == null)
        {
            return NullSize;
        }

        // Check for empty collection
        if (value is ICollection collection && collection.Count == 0)
        {
            return NullSize;
        }

        // Check for empty string
        if (value is string str && string.IsNullOrEmpty(str))
        {
            return NullSize;
        }

        return NotNullSize;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Determines if an Order can be cancelled based on its status.
/// Returns true only for OPEN, REFUSED, or DELIVERING.
/// </summary>
public class OrderStatusToCancelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return false;

        string status = value.ToString() ?? string.Empty;

        // Logic matches Trigger conditions:
        return status == "OPEN" || status == "REFUSED" || status == "DELIVERING";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Controls visibility based on Order Status.
/// Visible only when status is "DELIVERING".
/// </summary>
public class OrderStatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Collapsed;

        string status = value.ToString() ?? string.Empty;

        return (status == "DELIVERING") ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts the Simulator state (Running/Stopped) to appropriate button text.
/// </summary>
public class RunStopSimulatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRunning)
        {
            return isRunning ? "הפעל סימולציה" : "עצור סימולציה";
        }
        return "הפעל סימולציה";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a status string or boolean to a specific SolidColorBrush.
/// Used for color-coding rows or elements based on state (Green=Good, Red=Bad, etc.).
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Brushes.Transparent;

        // Handle Boolean values
        if (value is bool bo)
            return bo ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")) // Light Green
                      : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")); // Light Red

        // Handle String/Enum values
        if (value.ToString() is string str && str != string.Empty)
        {
            switch (str)
            {
                // Positive / Final states (Light Green)
                case "STANDART":
                case "רגיל":
                case "COMPLETED":
                case "נמסר":
                case "DELIVERED":
                case "נמסר בהצלחה":
                case "True":
                case "פעיל":
                case "ONTYME":
                case "בזמן":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));

                // Process / Warning states (Light Orange/Yellow)
                case "FAST_DELIVERY":
                case "מהיר":
                case "DELIVERING":
                case "במשלוח":
                case "NOTFOUND":
                case "כתובת/לקוח לא נמצא":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));

                // Initial states (Light Blue)
                case "פתוח":
                case "OPEN":
                case "INRISK":
                case "בסיכון":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));

                // Negative / Error states (Light Red)
                case "DELIVER_IMMEDIATELY":
                case "מיידי":
                case "סורב על ידי הלקוח":
                case "REFUSED":
                case "בוטל":
                case "CANCELLED":
                case "LATE":
                case "באיחור":
                case "FAILED":
                case "נכשל":
                case "False":
                case "לא פעיל":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));

                default:
                    return Brushes.Transparent;
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}