using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

public static class Tools
{
    // Attached Property for numeric-only validation
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(Tools),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    public static bool GetNumericOnly(DependencyObject obj)
    {
        return (bool)obj.GetValue(NumericOnlyProperty);
    }

    public static void SetNumericOnly(DependencyObject obj, bool value)
    {
        obj.SetValue(NumericOnlyProperty, value);
    }

    private static void OnNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
            }
            else
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            }
        }
    }

    private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    internal static string GetDescription(this Enum value)
    {
        if(value.GetType().GetField(value.ToString()) is FieldInfo field)
        {
            if (field == null) return value.ToString();

            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;

        }
        else
        {
            return string.Empty;
        }   
    }

    // גרסה 1: ללא תנאי (כמו שהיה לך עד עכשיו - שומר על תאימות לאחור)
    internal static void OpenOrActivateWindow<T>(params object?[] args) where T : Window
    {
        OpenOrActivateWindow<T>(null, args);
    }

    // גרסה 2: עם תנאי סינון (הפונקציה הראשית)
    internal static void OpenOrActivateWindow<T>(Predicate<T>? matchPredicate, params object?[] args) where T : Window
    {
        // חיפוש חלון: גם מהסוג הנכון וגם (אם נשלח תנאי) עומד בתנאי
        var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault(window =>
            matchPredicate == null || matchPredicate(window));

        if (existingWindow != null)
        {
            if (existingWindow.WindowState == WindowState.Minimized)
                existingWindow.WindowState = WindowState.Normal;

            existingWindow.Activate();

            // עדכון מצב אם החלון תומך בזה (כפי שעשינו קודם)
            if (existingWindow is IWindowUpdater updaterWindow)
            {
                updaterWindow.UpdateState(args);
            }
        }
        else
        {
            // יצירת חלון חדש
            var newWindow = (T)Activator.CreateInstance(typeof(T), args)!;
            newWindow.Show();
        }
    }

    public static IEnumerable<SelectionItem> GetEnumList<T>(string defaultText = "הכל") where T : struct, Enum
    {
        // 1. יצירת רשימה והוספת פריט ברירת המחדל
        var list = new List<SelectionItem>
        {
            new SelectionItem { Id = null, Name = defaultText }
        };

        // 2. שליפת ערכי ה-Enum והמרתם
        var enumValues = Enum.GetValues(typeof(T))
                             .Cast<T>()
                             .Select(e => new SelectionItem
                             {
                                 Id = e, // ה-Id יקבל את ערך ה-Enum
                                 Name = GetDescription(e) // שימוש בפונקציה הקיימת שלך לתיאור
                             });

        // 3. איחוד והחזרה
        list.AddRange(enumValues);
        return list;
    }

    public class SelectionItem
    {
        public object? Id { get; set; }
        public required string Name { get; set; }
    }

    public static T GetSafeFromBl<T>(Func<T> functionToRun, T defaultValue = default!)
    {
        try
        {
            // כאן אנחנו מפעילים את הפונקציה שנשלחה
            return functionToRun();
        }
        catch (Exception ex)
        {
            // הצגת הודעה למשתמש
            MessageBox.Show($"לא הצלחנו לקבל תשובה מ s_bl: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // במקרה של שגיאה, חייבים להחזיר משהו.
            // default(T) יחזיר 0 למספרים, או null לאובייקטים.
            return defaultValue;
        }
    }

    public static void RunSafe(Action actionToRun)
    {
        try
        {
            actionToRun();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

}

public interface IWindowUpdater
{
    void UpdateState(params object?[] args);
}