using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Data;

namespace PL;

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
//public class EnumToStringConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return value.ToString() ?? string.Empty;
//    }
//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNullOrEmpty = value == null;
        
        // בדיקה אם זו רשימה ריקה
        if (!isNullOrEmpty && value is ICollection collection)
        {
            isNullOrEmpty = collection.Count == 0;
        }

        if (parameter != null && parameter.ToString() == "Invert")
        {
            return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (parameter != null && parameter.ToString() == "Invert")
            {
                // אם אנחנו במצב עריכה -> תסתיר את התצוגה הרגילה
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

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

public class ValueToHebrewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string val)
        {
            if(val == "Update")
                return "עדכן";
            if (val == "Add")
                return "חדש";
        }
        return "חדש";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string val)
        {
            if(val == "חדש")
                return "Add";
            if (val == "עדכן")
                return "Update";
        }
        return "Add";
    }
}


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

public class TotalHoursConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            // המרה ל-(int) חותכת את השארית ומחזירה רק שעות שלמות כולל ימים
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
    /// <param name="value">The TimeSpan value to convert.</param>
    /// <param name="targetType">The type of the binding target property (typically string).</param>
    /// <param name="parameter">Optional parameter (not used in this converter).</param>
    /// <param name="culture">Culture information for formatting.</param>
    /// <returns>
    /// A string in "DD:HH" format representing days and hours, or "00:00" if the value is not a TimeSpan.
    /// </returns>
    /// <remarks>
    /// Example: A TimeSpan of 2 days and 5 hours converts to "02:05".
    /// </remarks>
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
    /// <param name="value">The string value to convert (expected format: "DD:HH").</param>
    /// <param name="targetType">The type of the binding target property (typically TimeSpan).</param>
    /// <param name="parameter">Optional parameter (not used in this converter).</param>
    /// <param name="culture">Culture information for parsing.</param>
    /// <returns>
    /// A TimeSpan value representing the parsed days and hours, or TimeSpan.Zero if parsing fails.
    /// </returns>
    /// <remarks>
    /// Example: The string "02:05" converts to a TimeSpan of 2 days and 5 hours.
    /// If the string format is invalid or cannot be parsed, returns TimeSpan.Zero.
    /// </remarks>
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

public class TimeSpanToShortStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            // אם שלילי - לא להציג כלום
            if (timeSpan == TimeSpan.Zero)
                return "00:00:00";

            // אם יותר מיום - הצג ימים
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} ימים, {timeSpan.Hours:D2} שעות ו{timeSpan.Minutes:D2} דקות";

            // אחרת הצג שעות:דקות
            return $"{(int)timeSpan.TotalHours:D2} שעות ו {timeSpan.Minutes:D2} דקות";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToSizeConverter : IValueConverter
{
    public double NullSize { get; set; }
    public double NotNullSize { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // בדיקה אם הערך הוא Null
        if (value == null)
        {
            return NullSize;
        }

        // בדיקה אם זו רשימה ריקה
        if (value is ICollection collection && collection.Count == 0)
        {
            return NullSize;
        }

        // אופציונלי: אם זה סטרינג ריק
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

public class OrderStatusToCancelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // בדיקה אם הערך הוא null
        if (value == null) return false;

        // המרה ל-String או ל-Enum שלך. נניח שזה עובד מול המחרוזת או ה-Enum
        string status = value.ToString();

        // הלוגיקה שהייתה לך ב-Triggers:
        // פעיל רק אם: OPEN, REFUSED, DELIVERING
        return status == "OPEN" || status == "REFUSED" || status == "DELIVERING";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}