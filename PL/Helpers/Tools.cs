using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

/// <summary>
/// A static utility class containing helper methods, attached properties, and extensions
/// used throughout the PL layer.
/// </summary>
public static class Tools
{
    #region Attached Property: NumericOnly

    /// <summary>
    /// Identifies the NumericOnly attached property.
    /// When set to true, the attached TextBox will only accept numeric input.
    /// </summary>
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(Tools),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    /// <summary>
    /// Gets the value of the NumericOnly property.
    /// </summary>
    public static bool GetNumericOnly(DependencyObject obj)
    {
        return (bool)obj.GetValue(NumericOnlyProperty);
    }

    /// <summary>
    /// Sets the value of the NumericOnly property.
    /// </summary>
    public static void SetNumericOnly(DependencyObject obj, bool value)
    {
        obj.SetValue(NumericOnlyProperty, value);
    }

    /// <summary>
    /// Callback triggered when the NumericOnly property changes.
    /// Subscribes or unsubscribes from the PreviewTextInput event.
    /// </summary>
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

    /// <summary>
    /// Handles the text input event to allow only digits.
    /// </summary>
    private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    #endregion

    #region Attached Property: MoveFocusTo

    /// <summary>
    /// Identifies the MoveFocusTo attached property.
    /// Used to specify which control should receive focus when 'Enter' is pressed on the source control.
    /// </summary>
    public static readonly DependencyProperty MoveFocusToProperty =
            DependencyProperty.RegisterAttached(
                "MoveFocusTo",
                typeof(Control),
                typeof(Tools),
                new PropertyMetadata(null, OnMoveFocusToChanged));

    /// <summary>
    /// Gets the target control to move focus to.
    /// </summary>
    public static Control GetMoveFocusTo(DependencyObject obj)
    {
        return (Control)obj.GetValue(MoveFocusToProperty);
    }

    /// <summary>
    /// Sets the target control to move focus to.
    /// </summary>
    public static void SetMoveFocusTo(DependencyObject obj, Control value)
    {
        obj.SetValue(MoveFocusToProperty, value);
    }

    /// <summary>
    /// Callback triggered when the MoveFocusTo property changes.
    /// </summary>
    private static void OnMoveFocusToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Control control)
        {
            control.KeyDown -= Control_KeyDown;
            control.PreviewKeyDown -= Control_KeyDown;

            if (e.NewValue is Control)
            {
                control.KeyDown += Control_KeyDown;
            }
        }
    }

    /// <summary>
    /// Handles the KeyDown event to move focus when Enter is pressed.
    /// </summary>
    private static void Control_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is Control source && e.Key == Key.Enter)
        {
            var target = GetMoveFocusTo(source);

            if (target != null)
            {
                e.Handled = true;
                target.Focus();
            }
        }
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Retrieves the 'Description' attribute text from an Enum value.
    /// If no attribute exists, returns the Enum's name.
    /// </summary>
    /// <param name="value">The enum value to describe.</param>
    /// <returns>The description string.</returns>
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

    /// <summary>
    /// Positions a child window over a parent window and links their closing events.
    /// </summary>
    /// <param name="child">The child window.</param>
    /// <param name="parent">The parent/owner window.</param>
    public static void SetSoftOwner(this Window child, Window parent)
    {
        child.Loaded += (s, e) =>
        {
            // Center the child over the parent
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

        parent.Closed += parentClosed;

        child.Closed += (s, e) =>
        {
            parent.Closed -= parentClosed;
        };
    }

    #endregion

    #region Window Management

    /// <summary>
    /// Opens a new window of type <typeparamref name="T"/> or activates an existing one.
    /// (Simplified overload without filtering).
    /// </summary>
    internal static void OpenOrActivateWindow<T>(Window? Owner, params object?[] args) where T : Window
    {
        OpenOrActivateWindow<T>(matchPredicate: null, owner: Owner, args: args);
    }

    /// <summary>
    /// Opens a new window of type <typeparamref name="T"/> or activates an existing one based on a predicate.
    /// </summary>
    /// <typeparam name="T">The type of Window to open.</typeparam>
    /// <param name="matchPredicate">A condition to find an existing window instance.</param>
    /// <param name="owner">The owner window (optional).</param>
    /// <param name="args">Arguments to pass to the window's constructor.</param>
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

            // If the window implements IWindowUpdater (custom interface), update its state
            if (existingWindow is IWindowUpdater updaterWindow)
            {
                updaterWindow.UpdateState(args);
            }
        }
        else
        {
            // Create new instance using reflection
            var newWindow = (T)Activator.CreateInstance(typeof(T), args)!;

            if (owner != null)
            {
                // Prepare for manual positioning via SetSoftOwner
                newWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                newWindow.Left = -10000; // Move off-screen initially
                newWindow.Top = -10000;

                newWindow.SetSoftOwner(owner);
            }

            newWindow.Show();
            newWindow.Activate();
        }
    }

    #endregion

    #region Helper Methods (Enum List, Safe Execution)

    /// <summary>
    /// Converts a collection of Enum values into a list of SelectionItems.
    /// Adds a default "All" option at the beginning.
    /// </summary>
    public static IEnumerable<SelectionItem> GetEnumList<T>(IEnumerable<T> values, string defaultText = "הכל") where T : struct, Enum
    {
        var list = new List<SelectionItem>
        {
            new SelectionItem { Id = null, Name = defaultText }
        };

        var convertedItems = values.Select(e => new SelectionItem
        {
            Id = e,
            Name = GetDescription(e)
        });

        list.AddRange(convertedItems);
        return list;
    }

    /// <summary>
    /// Gets all values of an Enum type as a list of SelectionItems.
    /// </summary>
    public static IEnumerable<SelectionItem> GetEnumList<T>(string defaultText = "הכל") where T : struct, Enum
    {
        var allValues = Enum.GetValues(typeof(T)).Cast<T>();
        return GetEnumList(allValues, defaultText);
    }

    /// <summary>
    /// Executes a function safely, catching any exceptions and displaying an error message.
    /// Returns a default value in case of failure.
    /// </summary>
    public static T GetSafeFromBl<T>(Func<T> functionToRun, T defaultValue = default!)
    {
        try
        {
            return functionToRun();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"לא הצלחנו לקבל תשובה מ s_bl: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes an action safely, catching any exceptions and displaying an error message.
    /// </summary>
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

    #endregion

    #region Global Events

    public static event Action? ResetRequested;

    /// <summary>
    /// Triggers the global ResetRequested event to restart the application/simulation.
    /// </summary>
    public static void TriggerReset()
    {
        ResetRequested?.Invoke();
    }

    #endregion

    #region Nested Classes

    /// <summary>
    /// Represents a simple item for selection controls (like ComboBox).
    /// </summary>
    public class SelectionItem
    {
        public object? Id { get; set; }
        public required string Name { get; set; }
    }

    /// <summary>
    /// A basic implementation of ICommand for MVVM architecture.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }

    #endregion
}