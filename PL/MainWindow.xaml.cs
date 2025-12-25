using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private DateTime _systemCurrentTime;
    public DateTime SystemCurrentTime
    {
        get => _systemCurrentTime;
        set
        {
            if (_systemCurrentTime == value) return;
            _systemCurrentTime = value;
            OnPropertyChanged();
        }
    }

    private BO.Config _adminConfig;
    public BO.Config AdminConfig
    {
        get => _adminConfig;
        set
        {
            if (_adminConfig == value) return;
            _adminConfig = value;
            OnPropertyChanged();
        }
    }

    private readonly Action _clockObserver;
    private readonly Action _configObserver;


    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        SystemCurrentTime = s_bl.Admin.GetClock();

        AdminConfig = s_bl.Admin.GetConfig();

        _configObserver = () => Dispatcher.Invoke(() =>
            AdminConfig = s_bl.Admin.GetConfig()
        );

        _clockObserver = () => Dispatcher.Invoke(() =>
            SystemCurrentTime = s_bl.Admin.GetClock()
        );

        _configObserver = () => Dispatcher.Invoke(() =>
            AdminConfig = s_bl.Admin.GetConfig()
        );

        s_bl.Admin.AddClockObserver(_clockObserver);
        s_bl.Admin.AddConfigObserver(_configObserver);

        Closed += (_, __) =>
        {
            s_bl.Admin.RemoveClockObserver(_clockObserver);
            s_bl.Admin.RemoveConfigObserver(_configObserver);
        };
    }

    private void MinutePlus_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.MINUTE);
    }
    private void MinuteMinus_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.MINUTE, -1);
    }
    private void HourPlus_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.HOUR);
    }
    private void HourMinus_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.HOUR, -1);
    }
    private void YearPlus_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.YEAR);
    }


    private bool _isDirty = false;

    // 1. כניסה לשדה: מאפסים את הדגל
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _isDirty = false;
    }



    // 2. שינוי טקסט: אם המשתמש מקליד, מרימים את הדגל
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // אנחנו בודקים שיש פוקוס כדי לוודא שזה המשתמש מקליד 
        // ולא סתם עדכון אוטומטי של המערכת
        if (sender is TextBox tb && tb.IsFocused)
        {
            _isDirty = true;
        }
    }


    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("ביצעת שינוי. לשמור?", "שמירה",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // כאן ה-Binding כבר עדכן את המשתנה AdminConfig ברוב המקרים,
                    // אבל ליתר ביטחון אפשר לכפות עדכון אם צריך, או פשוט לשמור:
                    s_bl.Admin.SetConfig(AdminConfig);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("שגיאה: " + ex.Message);
                    AdminConfig = s_bl.Admin.GetConfig(); // שחזור במקרה שגיאה
                }
            }
            else
            {
                // המשתמש בחר "לא" - מבטלים את השינוי
                // טעינה מחדש דורסת את מה שהמשתמש הקליד
                AdminConfig = s_bl.Admin.GetConfig();
            }
        }

        // איפוס סופי ליציאה
        _isDirty = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Add your logic here, for example, allow only digits:
        e.Handled = !e.Text.All(char.IsDigit);
    }
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {

    }
}