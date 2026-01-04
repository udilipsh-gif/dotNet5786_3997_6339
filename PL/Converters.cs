using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PL;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        //המרה של ערך ENUM לבוליאני
        string checkValue = value.ToString()!;//אין אפשרות שיהיה null כי זה ENUM
        string targetValue = parameter.ToString()!;

        return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //המרה של ערך בוליאני חזרה ל ENUM
        bool useValue = (bool)value;
        string targetValue = parameter.ToString()!;//אין אפשרות שיהיה null כי זה פרמטר שהוגדר בקישור

        if (useValue)
        {
            return Enum.Parse(targetType, targetValue);
        }

        return Binding.DoNothing;
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // אם הערך לא null - מציג (Visible), אחרת מסתיר (Collapsed)
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}