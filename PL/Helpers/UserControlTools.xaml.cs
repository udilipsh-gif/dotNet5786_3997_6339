using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// A custom UserControl that allows inputting a TimeSpan broken down into Days and Hours.
/// Implements INotifyPropertyChanged to support UI data binding updates.
/// </summary>
public partial class TimeSpanInput : UserControl, INotifyPropertyChanged
{
    /// <summary>
    /// Initializes a new instance of the TimeSpanInput control.
    /// </summary>
    public TimeSpanInput()
    {
        InitializeComponent();
    }

    #region Dependency Property

    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(TimeSpan), typeof(TimeSpanInput),
            new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    /// <summary>
    /// Gets or sets the total time duration represented by this control.
    /// This is the main property to bind to from external view models.
    /// </summary>
    public TimeSpan Value
    {
        get => (TimeSpan)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Callback method invoked when the <see cref="Value"/> property changes.
    /// Notifies the UI to update the individual <see cref="DaysPart"/> and <see cref="HoursPart"/> fields.
    /// </summary>
    /// <param name="d">The dependency object (TimeSpanInput instance).</param>
    /// <param name="e">Event arguments containing old and new values.</param>
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TimeSpanInput)d;
        // Notify the UI that the split properties have changed based on the new total Value
        control.OnPropertyChanged(nameof(DaysPart));
        control.OnPropertyChanged(nameof(HoursPart));
    }

    #endregion

    #region Internal UI Properties

    /// <summary>
    /// Gets or sets the 'Days' component of the TimeSpan.
    /// Used for binding to the Days TextBox in the UI.
    /// </summary>
    /// <remarks>
    /// Setting this property creates a new <see cref="TimeSpan"/> with the updated days
    /// and assigns it to <see cref="Value"/>.
    /// </remarks>
    public int DaysPart
    {
        get => Value.Days;
        set
        {
            if (Value.Days != value)
            {
                Value = new TimeSpan(value, Value.Hours, Value.Minutes, Value.Seconds);
                OnPropertyChanged(nameof(DaysPart));
            }
        }
    }

    /// <summary>
    /// Gets or sets the 'Hours' component of the TimeSpan.
    /// Used for binding to the Hours TextBox in the UI.
    /// </summary>
    /// <remarks>
    /// Validates that the input is between 0 and 23.
    /// Setting this property creates a new <see cref="TimeSpan"/> with the updated hours
    /// and assigns it to <see cref="Value"/>.
    /// </remarks>
    public int HoursPart
    {
        get => Value.Hours;
        set
        {
            // Ensure hours are within a valid daily range (0-23)
            if (value >= 0 && value < 24 && Value.Hours != value)
            {
                Value = new TimeSpan(Value.Days, value, Value.Minutes, Value.Seconds);
                OnPropertyChanged(nameof(HoursPart));
            }
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="name">The name of the property that changed.</param>
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}