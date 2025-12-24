using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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

    private readonly Action _clockObserver;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        SystemCurrentTime = s_bl.Admin.GetClock();

        _clockObserver = () => Dispatcher.Invoke(() =>
            SystemCurrentTime = s_bl.Admin.GetClock()
        );

        s_bl.Admin.AddClockObserver(_clockObserver);

        Closed += (_, __) => s_bl.Admin.RemoveClockObserver(_clockObserver);
    }

    private void MinutePlus_Click(object sender, RoutedEventArgs e) 
    {
        s_bl.Admin.ForwardClock(BO.TimeUnit.MINUTE);
    }
    private void HourPlus_Click(object sender, RoutedEventArgs e) 
    {
        s_bl.Admin.ForwardClock(xp,BO.TimeUnit.HOUR-);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}