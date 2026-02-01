using PL.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml - The entry point and login screen.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Constants & Fields

        // Constants for user roles returned by the BL
        private const string ROLE_MANAGER = "Manager";
        private const string ROLE_COURIER = "Courier";

        /// <summary>
        /// The singleton access point to the Business Logic layer.
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Mutex to handle safe thread synchronization for the clock observer.
        /// </summary>
        private readonly ObserverMutex _clockMutex = new();

        #endregion

        #region Properties & Commands

        /// <summary>
        /// Command executed when the login button is clicked or Enter is pressed.
        /// </summary>
        public ICommand LoginCommand { get; private set; }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the User ID entered by the user.
        /// Bound two-way to the text box in the UI.
        /// </summary>
        public string UserId
        {
            get { return (string)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(UserId), typeof(string), typeof(MainWindow));

        /// <summary>
        /// Gets or sets the current simulated system time.
        /// Updates automatically via the clock observer.
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainWindow));

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // Initialize command before UI to ensure binding works correctly
            LoginCommand = new Tools.RelayCommand(ExecuteLogin, CanLogin);

            InitializeComponent();
        }

        /// <summary>
        /// Event handler for Window Loaded. Initializes the clock observer.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClockObserver(); // Initial fetch
            Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));
        }

        /// <summary>
        /// Event handler for Window Closing. Cleans up the clock observer.
        /// </summary>
        private void MainWindow_Close(object sender, EventArgs e)
        {
            Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
        }

        #endregion

        #region Login Logic

        /// <summary>
        /// Validates whether the login command can be executed.
        /// </summary>
        /// <param name="parameter">The PasswordBox control passed as a parameter.</param>
        /// <returns>True if both User ID and Password are provided; otherwise, false.</returns>
        private bool CanLogin(object? parameter)
        {
            if (string.IsNullOrEmpty(UserId))
                return false;

            if (parameter is not PasswordBox passwordBox || string.IsNullOrEmpty(passwordBox.Password))
                return false;

            return true;
        }

        /// <summary>
        /// Executes the login process using the provided credentials.
        /// </summary>
        /// <param name="parameter">The PasswordBox control containing the password.</param>
        private void ExecuteLogin(object? parameter)
        {
            // 1. Validate Input Controls
            if (parameter is not PasswordBox passwordBox)
                return;

            if (string.IsNullOrEmpty(passwordBox.Password))
            {
                ShowMessage("נא להזין סיסמה", MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(UserId))
            {
                ShowMessage("נא להזין תעודת זהות", MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(UserId, out int userId))
            {
                ShowMessage("תעודת זהות חייבת להכיל ספרות בלבד", MessageBoxImage.Error);
                return;
            }

            // 2. Attempt Login
            try
            {
                string? userRole = s_bl.Courier.Login(userId, passwordBox.Password);

                // Clear credentials from UI upon success
                UserId = string.Empty;
                passwordBox.Clear();

                // 3. Navigate based on role
                NavigateToUserDashboard(userRole, userId);
            }
            catch (BO.BlIncorrectPasswordException)
            {
                ShowMessage("שם משתמש או סיסמה שגויים", MessageBoxImage.Warning);
                passwordBox.Clear();
                passwordBox.Focus();
            }
            catch (Exception ex)
            {
                ShowMessage($"שגיאה במערכת: {ex.Message}", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles navigation to the appropriate window based on the user role.
        /// </summary>
        /// <param name="role">The role of the user (e.g., Manager, Courier).</param>
        /// <param name="userId">The unique ID of the user logging in.</param>
        /// <exception cref="BO.BlNoAccessException">Thrown when the provided role is unrecognized.</exception>
        private void NavigateToUserDashboard(string? role, int userId)
        {
            switch (role)
            {
                case ROLE_MANAGER:
                    Tools.OpenOrActivateWindow<ManagerWindow>(this, userId);
                    break;

                case ROLE_COURIER:
                    if (userId != 0)
                    {
                        var courierWindow = new MainCourier(userId);
                        courierWindow.SetSoftOwner(this);
                        courierWindow.Show();
                    }
                    break;

                default:
                    throw new BO.BlNoAccessException("שגיאת הרשאה: תפקיד לא מזוהה.");
            }
        }

        /// <summary>
        /// Helper method to display message boxes with a consistent title.
        /// </summary>
        /// <param name="message">The text to display to the user.</param>
        /// <param name="icon">The icon indicating the severity (Error, Warning, etc.).</param>
        private void ShowMessage(string message, MessageBoxImage icon)
        {
            MessageBox.Show(message, "כניסה למערכת", MessageBoxButton.OK, icon);
        }

        #endregion

        #region Clock Observer

        /// <summary>
        /// Observer callback method to update the UI with the current simulated time.
        /// Uses a Mutex to prevent race conditions during UI updates.
        /// </summary>
        private void ClockObserver()
        {
            // Check if an update is already in progress
            if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // Fetch time on a background thread to keep UI responsive
                CurrentTime = await Task.Run(() => s_bl.Admin.GetClock());

                // Release lock and check if another update is pending
                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    ClockObserver();
            });
        }

        #endregion
    }
}