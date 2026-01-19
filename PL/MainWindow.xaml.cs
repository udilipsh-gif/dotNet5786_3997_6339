
using PL.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PL;

public partial class MainWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public ICommand LoginCommand { get; private set; }

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

    public string UserId
    {
        get { return (string)GetValue(UserIdProperty); }
        set { SetValue(UserIdProperty, value); }
    }

    public static readonly DependencyProperty UserIdProperty =
        DependencyProperty.Register("UserId", typeof(string), typeof(MainWindow));


    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        LoginCommand = new RelayCommand(ExecuteLogin, CanLogin);

        InitializeComponent();
    }

    private void ExecuteLogin(object? parameter)
    {
        try
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
            {
                MessageBox.Show("נא להזין סיסמה", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(UserId))
            {
                MessageBox.Show("נא להזין תעודת זהות", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(UserId, out int userId))
            {
                MessageBox.Show("תעודת זהות לא תקינה", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string? user = s_bl.Courier.Login(userId, passwordBox.Password);

            UserId = string.Empty;
            passwordBox.Clear();

            if (user == "Manager")
            {
                Tools.OpenOrActivateWindow<ManagerWindow>(this, userId);
                return;
            }
            else if (user == "Courier")
            {
                // אפשר לפתוח חלון שליח
                if (userId != 0)
                {
                    Window window = new MainCourier(userId);
                    window.SetSoftOwner(this);
                    window.Show();
                }

                return;
            }

            //throw new BlNoAccessException("לא הצלחנו לחבר אותך");
        }
        catch (BO.BlIncorrectPasswordException)
        {
            MessageBox.Show("הסיסמה לא נכונה");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }


    }

    private bool CanLogin(object? parameter)
    {
        // 1. בדיקה שיש תעודת זהות (מקושרת ב-Binding)
        if (string.IsNullOrEmpty(this.UserId))
            return false;

        // 2. בדיקה שיש סיסמה (התקבלה כפרמטר מה-Binding)
        var passwordBox = parameter as PasswordBox;
        if (passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
            return false;

        return true;
    }
    private readonly ObserverMutex _clockMutex = new(); //stage 7
    private void ClockObserver()
    {
        if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())//הדלקת פלאג בפונקציה שמציינת שהריצה בעיצומה ואם מישהו ביקש ריסטארט בזמן הזה
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            CurrentTime = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock());
            if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())//אם מישהו ביקש ריסטארט בזמן שהריצה הייתה בעיצומה
                ClockObserver();
        });
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ClockObserver();
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));
    }

    private void MainWindow_Close(object sender, System.EventArgs e)
    {
        Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));

    }
}


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

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
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
        if (parameter == null && default(T) != null)
            return false;

        return _canExecute == null || _canExecute((T?)parameter);
    }

    public void Execute(object? parameter)
    {
        _execute((T?)parameter);
    }
}
