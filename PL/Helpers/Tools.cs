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
        if (value.GetType().GetField(value.ToString()) is FieldInfo field)
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
    internal static void OpenOrActivateWindow<T>(Window? Owner, params object?[] args) where T : Window
    {
        OpenOrActivateWindow<T>(matchPredicate: null, owner: Owner, args: args);
    }

    // גרסה 2: עם תנאי סינון (הפונקציה הראשית)
    // הוספנו פרמטר אופציונלי 'owner'
    internal static void OpenOrActivateWindow<T>(
        Predicate<T>? matchPredicate,
        Window? owner = null,
        params object?[] args) where T : Window
    {
        var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault(window =>
            matchPredicate == null || matchPredicate(window));

        if (existingWindow != null)
        {
            if (existingWindow.WindowState == WindowState.Minimized)
                existingWindow.WindowState = WindowState.Normal;

            existingWindow.Activate();


            if (existingWindow is IWindowUpdater updaterWindow)
            {
                updaterWindow.UpdateState(args);
            }
        }
        else
        {
            var newWindow = (T)Activator.CreateInstance(typeof(T), args)!;

            // --- שינוי 2: הגדרת הבעלות ---
            if (owner != null)
            {
                newWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                newWindow.Left = -10000;
                newWindow.Top = -10000;

                newWindow.SetSoftOwner(owner);
            }

            newWindow.Show();
            newWindow.Activate();
        }
    }

    public static IEnumerable<SelectionItem> GetEnumList<T>(IEnumerable<T> values, string defaultText = "הכל") where T : struct, Enum
    {
        // 1. יצירת רשימה והוספת פריט ברירת המחדל
        var list = new List<SelectionItem>
    {
        new SelectionItem { Id = null, Name = defaultText }
    };

        // 2. המרת הרשימה שקיבלנו
        var convertedItems = values.Select(e => new SelectionItem
        {
            Id = e,
            Name = GetDescription(e)
        });

        // 3. איחוד והחזרה
        list.AddRange(convertedItems);
        return list;
    }

    public static IEnumerable<SelectionItem> GetEnumList<T>(string defaultText = "הכל") where T : struct, Enum
    {
        var allValues = Enum.GetValues(typeof(T)).Cast<T>();
        return GetEnumList(allValues, defaultText);
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

    public static void SetSoftOwner(this Window child, Window parent)
    {
        child.Loaded += (s, e) =>
        {
            double left = parent.Left + (parent.ActualWidth - child.ActualWidth) / 2;
            double top = parent.Top + (parent.ActualHeight - child.ActualHeight) / 2;

            if (left < 0) left = 0;
            if (top < 0) top = 0;

            child.Left = left;
            child.Top = top;
        };

        EventHandler parentClosed = null!;
        parentClosed = (s, e) =>
        {
            if (child.IsLoaded) child.Close();
        };

        EventHandler parentStateChanged = null!;
        //parentStateChanged = (s, e) =>
        //{
        //    if (parent.WindowState == WindowState.Minimized)
        //        child.WindowState = WindowState.Minimized;
        //    else if (parent.WindowState != WindowState.Minimized &&
        //             child.WindowState == WindowState.Minimized)
        //        child.WindowState = WindowState.Normal;
        //};

        parent.Closed += parentClosed;
        parent.StateChanged += parentStateChanged;

        child.Closed += (s, e) =>
        {
            parent.Closed -= parentClosed;
            parent.StateChanged -= parentStateChanged;
        };
    }

    public static event Action? ResetRequested;

    public static void TriggerReset()
    {
        ResetRequested?.Invoke();
    }


}
