using System;
using System.Globalization;
using System.Windows.Data;

namespace PL;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // בדיקה בטוחה שמונעת קריסה אם משהו הוא NULL
        if (value == null || parameter == null)
            return false;

        string checkValue = value.ToString()!;
        string targetValue = parameter.ToString()!;

        return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // בדיקה בטוחה
        if (value == null || parameter == null)
            return Binding.DoNothing;

        bool useValue = (bool)value;
        string targetValue = parameter.ToString()!;

        if (useValue)
        {
            // המרה בטוחה של ה-Enum
            return Enum.Parse(targetType, targetValue);
        }

        return Binding.DoNothing;
    }
}