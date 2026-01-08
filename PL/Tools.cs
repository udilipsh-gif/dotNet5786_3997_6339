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

    internal static void OpenOrActivateWindow<T>(params object[] args) where T : Window
    {
        // חיפוש חלון פתוח מהסוג המבוקש באוסף החלונות של האפליקציה
        var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();

        if (existingWindow != null)
        {
            if (existingWindow.WindowState == WindowState.Minimized)
            {
                existingWindow.WindowState = WindowState.Normal;
            }

            // 2. נביא אותו לקדמת המסך (פוקוס)
            existingWindow.Activate();
        }
        else
        {
            // אם החלון לא קיים - ניצור מופע חדש ונציג אותו
            // Activator.CreateInstance מקבל מערך של פרמטרים
            var newWindow = (T)Activator.CreateInstance(typeof(T), args)!;
            newWindow.Show(); // שימוש ב-Show לא חוסם את החלון הראשי
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

}
