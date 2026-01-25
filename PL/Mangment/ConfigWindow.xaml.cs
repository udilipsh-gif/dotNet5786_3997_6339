using PL.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PL
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml - System Configuration Interface.
    /// </summary>
    /// <remarks>
    /// This window allows administrators to modify global system settings such as:
    /// <list type="bullet">
    /// <item><description>Manager credentials and store details</description></item>
    /// <item><description>Vehicle speeds for delivery calculations</description></item>
    /// <item><description>Operational logic (max distance, timeout periods)</description></item>
    /// <item><description>External API tokens (Maps, SMS, etc.)</description></item>
    /// </list>
    /// Changes are validated and saved via the Business Logic layer.
    /// </remarks>
    public partial class ConfigWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// Instance of the Business Logic layer.
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Mutex to handle safe thread synchronization for configuration updates.
        /// </summary>
        private readonly ObserverMutex _observerMutex = new();

        /// <summary>
        /// Delegate for the reset event handler, used to unsubscribe correctly.
        /// </summary>
        private readonly Action _resetHandler;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the current system configuration object.
        /// </summary>
        /// <remarks>
        /// Bound Two-Way to the UI controls. Represents the state being edited.
        /// </remarks>
        public BO.Config Configuration
        {
            get => (BO.Config)GetValue(ConfigurationProperty);
            set => SetValue(ConfigurationProperty, value);
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register(nameof(Configuration), typeof(BO.Config), typeof(ConfigWindow));

        #endregion

        #region Constructor & Lifecycle

        /// <summary>
        /// Initializes a new instance of the ConfigWindow class.
        /// </summary>
        public ConfigWindow()
        {
            InitializeComponent();

            // Define the reset handler to close the window if DB is reset externally
            _resetHandler = () => Dispatcher.Invoke(Close);
        }

        /// <summary>
        /// Handles the window loaded event. Registers observers and event listeners.
        /// </summary>
        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to global reset event
            Tools.ResetRequested += _resetHandler;

            // Register configuration observer
            Tools.RunSafe(() => s_bl.Admin.AddConfigObserver(ConfigObserver));

            // Initial fetch
            ConfigObserver();
        }

        /// <summary>
        /// Handles the window closing event. Cleans up observers to prevent memory leaks.
        /// </summary>
        private void ConfigWindow_Close(object? sender, EventArgs e)
        {
            Tools.ResetRequested -= _resetHandler;
            Tools.RunSafe(() => s_bl.Admin.RemoveConfigObserver(ConfigObserver));
        }

        #endregion

        #region Observers

        /// <summary>
        /// Observer callback to fetch and update configuration from the BL.
        /// Uses a background task to prevent UI freezing.
        /// </summary>
        private void ConfigObserver()
        {
            if (_observerMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Task.Run(async () =>
            {
                try
                {
                    var newConfig = s_bl.Admin.GetConfig();

                    Dispatcher.Invoke(() =>
                    {
                        Configuration = newConfig;
                    });

                    if (await _observerMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    {
                        ConfigObserver();
                    }
                }
                finally
                {
                    await _observerMutex.UnsetLoadInProgressAndCheckRestartRequested();
                }
            });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Save button click. Validates and saves changes to the BL.
        /// </summary>
        private void BtuSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate required fields (Basic validation example)
                if (Configuration == null) return;

                 s_bl.Admin.SetConfig(Configuration);

                MessageBox.Show(
                    "הנתונים נשמרו בהצלחה",
                    "שמירה",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"שגיאה בשמירת הנתונים: {ex.Message}",
                    "שגיאה",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Cancel button click. Closes the window without saving.
        /// </summary>
        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}