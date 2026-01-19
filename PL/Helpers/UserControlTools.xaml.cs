using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL;

public partial class TimeSpanInput : UserControl, INotifyPropertyChanged
{
    public TimeSpanInput()
    {
        InitializeComponent();
    }

    // זה המאפיין שתקשור אליו מבחוץ (למשל: MaxTimeInactivity)
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeSpanInput),
            new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public TimeSpan Value
    {
        get => (TimeSpan)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TimeSpanInput)d;
        control.OnPropertyChanged(nameof(DaysPart));
        control.OnPropertyChanged(nameof(HoursPart));
    }

    // מאפיינים פנימיים לשימוש ה-TextBox
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

    public int HoursPart
    {
        get => Value.Hours;
        set
        {
            if (value >= 0 && value < 24 && Value.Hours != value)
            {
                Value = new TimeSpan(Value.Days, value, Value.Minutes, Value.Seconds);
                OnPropertyChanged(nameof(HoursPart));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
