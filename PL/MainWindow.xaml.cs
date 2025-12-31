using PL.Courier;
using PL.Order;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL;

/// <summary>
/// Interaction logic for MainWindow.xaml - the main application window and navigation hub.
/// </summary>
/// <remarks>
/// This window serves as the primary entry point for the application, providing:
/// <list type="bullet">
/// <item><description>System clock display and manipulation controls</description></item>
/// <item><description>Configuration management interface</description></item>
/// <item><description>Navigation to courier, order, and delivery management windows</description></item>
/// <item><description>Database initialization and reset operations</description></item>
/// <item><description>Real-time updates through observer pattern for clock and configuration changes</description></item>
/// </list>
/// Implements INotifyPropertyChanged for data binding support.
/// </remarks>
public partial class MainWindow : Window, INotifyPropertyChanged//זה היה בגלל הנסיון של משקיף למחוק מחוק אבל נראה לי שטופל כבר אחרת. לבדוק אם נצרך.
{

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

    }

    /// <summary>
    /// Business logic layer instance for accessing system operations.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Gets or sets the current system clock time displayed in the UI.
    /// </summary>
    /// <remarks>
    /// This property is bound to the UI and automatically updates when the system clock advances.
    /// It reflects the business clock time, which may differ from the actual system time.
    /// </remarks>
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    /// <summary>
    /// Dependency property for the CurrentTime property.
    /// </summary>
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

    /// <summary>
    /// Gets or sets the current system configuration.
    /// </summary>
    /// <remarks>
    /// This property is bound to the configuration editing UI controls.
    /// Changes to configuration fields are tracked and users are prompted to save before losing focus.
    /// </remarks>
    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    /// <summary>
    /// Dependency property for the Configuration property.
    /// </summary>
    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

    /// <summary>
    /// Handles the window close event, performs cleanup operations.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Closes all child windows except the main window</description></item>
    /// <item><description>Removes clock observer registration</description></item>
    /// <item><description>Removes configuration observer registration</description></item>
    /// </list>
    /// Ensures proper cleanup to prevent memory leaks.
    /// </remarks>
    private void MainWindow_Close(object? sender, EventArgs e)
    {
        CloseAllWindowsExceptMain();
        s_bl.Admin.RemoveClockObserver(ClockObserver);
        s_bl.Admin.RemoveConfigObserver(ConfigObserver);
    }

    /// <summary>
    /// Handles the window loaded event, initializes the window state and observers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Loads the current system clock time</description></item>
    /// <item><description>Registers a clock observer for real-time updates</description></item>
    /// <item><description>Loads the current system configuration</description></item>
    /// <item><description>Registers a configuration observer for real-time updates</description></item>
    /// </list>
    /// </remarks>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        CurrentTime = s_bl.Admin.GetClock();
        s_bl.Admin.AddClockObserver(ClockObserver);

        Configuration = s_bl.Admin.GetConfig();
        s_bl.Admin.AddConfigObserver(ConfigObserver);
    }

    /// <summary>
    /// Observer callback method for clock changes, updates the displayed time.
    /// </summary>
    private void ClockObserver() => CurrentTime = s_bl.Admin.GetClock();
    
    /// <summary>
    /// Observer callback method for configuration changes, updates the displayed configuration.
    /// </summary>
    private void ConfigObserver() => Configuration = s_bl.Admin.GetConfig();


    /// <summary>
    /// Handles the courier list button click event, opens the courier management window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void btnCourierList_Click(object sender, RoutedEventArgs e)
        => new CourierListWindow().Show();

    /// <summary>
    /// Handles the order list button click event, opens the order management window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnOrderList_Click(object sender, RoutedEventArgs e)
        => new OrderListWindow().Show();
    /// <summary>
    /// Handles the clock forward button clicks, advances the system clock by the specified time unit.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// The time unit is determined by the button's Tag property value:
    /// <list type="bullet">
    /// <item><description>"Minute" - advances clock by 1 minute</description></item>
    /// <item><description>"Hour" - advances clock by 1 hour</description></item>
    /// <item><description>"Day" - advances clock by 1 day</description></item>
    /// <item><description>"Week" - advances clock by 1 week</description></item>
    /// <item><description>"Month" - advances clock by 1 month</description></item>
    /// <item><description>"Year" - advances clock by 1 year</description></item>
    /// </list>
    /// All registered clock observers are notified after the clock is advanced.
    /// </remarks>
    private void ClockForward_Click(object sender, RoutedEventArgs e)
    {
    
        if (sender is FrameworkElement element && element.Tag != null)
        {
      
            string tagValue = element.Tag.ToString() ?? string.Empty;

            try
            {
                var value = tagValue switch
                {
                    "Minute" => BO.TimeUnit.MINUTE,
                    "Hour" => BO.TimeUnit.HOUR,
                    "Day" => BO.TimeUnit.DAY,
                    "Week" => BO.TimeUnit.WEEK,
                    "Month" => BO.TimeUnit.MONTH,
                    "Year" => BO.TimeUnit.YEAR,
                    _ => throw new BO.BlInvalidValueException("Invalid time unit")
                };

                s_bl.Admin.ForwardClock(value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }
    }


    /// <summary>
    /// Handles the reset database button click, prompts for confirmation and resets all data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This is a destructive operation that:
    /// <list type="bullet">
    /// <item><description>Prompts the user for confirmation</description></item>
    /// <item><description>Closes all open child windows</description></item>
    /// <item><description>Clears all couriers, orders, and deliveries from the database</description></item>
    /// <item><description>Resets configuration to defaults</description></item>
    /// <item><description>Resets auto-increment ID counters</description></item>
    /// <item><description>Displays a wait cursor during the operation</description></item>
    /// </list>
    /// </remarks>
    private void ResetDB(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Delete all data?", "ResetDB",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result is MessageBoxResult.Yes)
        {
            CloseAllWindowsExceptMain();
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                s_bl.Admin.ResetDB();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

    }

    /// <summary>
    /// Handles the initialize database button click, prompts for confirmation and initializes sample data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This operation:
    /// <list type="bullet">
    /// <item><description>Prompts the user for confirmation</description></item>
    /// <item><description>Closes all open child windows</description></item>
    /// <item><description>Resets the database (calls ResetDB internally)</description></item>
    /// <item><description>Populates the database with sample couriers, orders, and deliveries</description></item>
    /// <item><description>Displays a wait cursor during the operation</description></item>
    /// </list>
    /// Useful for development, testing, and demonstration purposes.
    /// </remarks>
    private void InitDB(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Do you want to initialize all data?", "InitDB",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result is MessageBoxResult.Yes)
        {
            CloseAllWindowsExceptMain();
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                s_bl.Admin.InitializeDB();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

    }


    /// <summary>
    /// Closes all open windows except the main window (MainWindow).
    /// </summary>
    /// <remarks>
    /// Iterates through all currently open application windows and closes any window
    /// that is not the main window. Used when performing database operations to ensure
    /// no stale data is displayed in other windows.
    /// </remarks>
    private void CloseAllWindowsExceptMain()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window != this && window.GetType() != typeof(MainWindow))
            {
                window.Close();
            }
        }
    }

    /// <summary>
    /// Flag indicating whether the user has made unsaved changes to configuration fields.
    /// </summary>
    private bool _isDirty = false;

    /// <summary>
    /// Handles the text box got focus event, resets the dirty flag.
    /// </summary>
    /// <param name="sender">The TextBox control that received focus.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Resets the dirty tracking flag when a user enters a configuration field.
    /// This allows tracking of actual user changes versus programmatic updates.
    /// </remarks>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _isDirty = false;
    }



    /// <summary>
    /// Handles the text box text changed event, sets the dirty flag when user types.
    /// </summary>
    /// <param name="sender">The TextBox control whose text changed.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Only sets the dirty flag if the TextBox has focus, ensuring that programmatic
    /// updates (such as binding refreshes) don't incorrectly mark the field as modified.
    /// This distinguishes between user input and automatic system updates.
    /// </remarks>
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb && tb.IsFocused)
        {
            _isDirty = true;
        }
    }


    /// <summary>
    /// Handles the text box lost focus event, prompts to save changes if modifications were made.
    /// </summary>
    /// <param name="sender">The TextBox control that lost focus.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// When the user leaves a configuration field that has been modified:
    /// <list type="bullet">
    /// <item><description>Prompts the user to save or discard changes</description></item>
    /// <item><description>If "Yes": Saves the configuration to the business layer and displays success message</description></item>
    /// <item><description>If "No": Reloads the original configuration, discarding user changes</description></item>
    /// <item><description>On error: Displays error message and restores original configuration</description></item>
    /// <item><description>Resets the dirty flag after handling</description></item>
    /// </list>
    /// The binding typically updates the Configuration property before this event fires.
    /// </remarks>
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("You made changes. Save?", "Save",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Admin.SetConfig(Configuration);
                    MessageBox.Show("Data saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    Configuration = s_bl.Admin.GetConfig();
                }
            }
            else
            {
                Configuration = s_bl.Admin.GetConfig();
            }
        }

        _isDirty = false;
    }

    /// <summary>
    /// Event raised when a property value changes, enabling data binding updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Handles text input validation for numeric-only fields.
    /// </summary>
    /// <param name="sender">The TextBox control receiving input.</param>
    /// <param name="e">Event arguments containing the input text.</param>
    /// <remarks>
    /// Only allows digit characters (0-9) to be entered into the field.
    /// Used for configuration fields that require integer values (e.g., Manager ID, speeds).
    /// Non-digit characters are automatically rejected.
    /// </remarks>
    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }
    
    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically populated by the compiler when called from a property setter.
    /// </param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Handles preview text input event for text boxes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Currently not implemented. Reserved for future input validation or preprocessing logic.
    /// </remarks>
    private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {

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