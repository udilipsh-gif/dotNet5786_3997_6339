using BO;
using DO;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL
{
    /// <summary>
    /// Main window for system management and administrative operations.
    /// </summary>
    /// <remarks>
    /// This window serves as the central administrative dashboard providing:
    /// <list type="bullet">
    /// <item><description>System Clock Management - View current time, advance time manually, or run automated time simulation</description></item>
    /// <item><description>Real-time Statistics Dashboard - Live updates of order counts by status and schedule</description></item>
    /// <item><description>Navigation to Sub-Modules - Quick access to courier management, order management, and system configuration</description></item>
    /// <item><description>Database Operations - Initialize sample data or reset the entire database</description></item>
    /// </list>
    /// Implements the Observer pattern to receive real-time updates from the business logic layer
    /// about clock changes and order statistics changes.
    /// </remarks>
    public partial class ManagerWindow : Window
    {
        #region Private Fields & Constants

        /// <summary>
        /// The unique identifier of the currently logged-in administrator.
        /// </summary>
        private readonly int _userId;

        /// <summary>
        /// Business logic layer interface instance for accessing all system services.
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Mutex for synchronizing clock observer updates to prevent race conditions.
        /// </summary>
        private readonly ObserverMutex _clockMutex = new();

        /// <summary>
        /// Mutex for synchronizing statistics observer updates to prevent race conditions.
        /// </summary>
        private readonly ObserverMutex _statsMutex = new();

        #endregion

        #region Inner Types

        /// <summary>
        /// Represents a single statistical item displayed in the dashboard.
        /// </summary>
        /// <remarks>
        /// Used for data binding to the ItemsControl that displays statistics.
        /// Implements INotifyPropertyChanged to allow real-time updates of the Value property
        /// without recreating the entire statistics list.
        /// </remarks>
        public class StatisticItem : INotifyPropertyChanged
        {
            /// <summary>
            /// Gets or sets the unique identifier for this statistic.
            /// </summary>
            /// <value>
            /// An object that can be either an <see cref="OrderStatus"/> or <see cref="ScheduleStatus"/> enum value.
            /// </value>
            public object Id { get; set; } = 0;
            
            /// <summary>
            /// Gets or sets the display name of the statistic.
            /// </summary>
            /// <value>
            /// A localized string describing what this statistic represents (e.g., "סטטוס הזמנה: ממתין למשלוח").
            /// </value>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Backing field for the Value property.
            /// </summary>
            private int _value;
            
            /// <summary>
            /// Gets or sets the numerical value of this statistic.
            /// </summary>
            /// <value>
            /// The count of orders matching this statistic's criteria.
            /// </value>
            public int Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;
            
            /// <summary>
            /// Raises the PropertyChanged event for the specified property.
            /// </summary>
            /// <param name="name">The name of the property that changed. If omitted, uses the caller member name.</param>
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Dependency Properties & Commands

        /// <summary>
        /// Command to open the Courier List window.
        /// </summary>
        public ICommand btnCourierList_Click { get; private set; }
        
        /// <summary>
        /// Command to open the Order List window.
        /// </summary>
        public ICommand btnOrderList_Click { get; private set; }
        
        /// <summary>
        /// Command to open the Configuration window.
        /// </summary>
        public ICommand btnConfig_Click { get; private set; }
        
        /// <summary>
        /// Command to reset the database. Only enabled when the simulator is stopped.
        /// </summary>
        public ICommand btnResetDB_Click { get; private set; }
        
        /// <summary>
        /// Command to initialize the database with sample data. Only enabled when the simulator is stopped.
        /// </summary>
        public ICommand btnInitDB_Click { get; private set; }
        
        /// <summary>
        /// Command to advance the system clock manually. Only enabled when the simulator is stopped.
        /// </summary>
        public ICommand btnClockForward_Click { get; private set; }
        
        /// <summary>
        /// Command to start or stop the time simulator.
        /// </summary>
        public ICommand btnSimulator_Click { get; private set; }
        
        /// <summary>
        /// Command to filter orders by clicking a statistic button.
        /// </summary>
        public ICommand btnStatistic_Click { get; private set; }

        /// <summary>
        /// Dependency property for CombinedStatistics.
        /// </summary>
        public static readonly DependencyProperty CombinedStatisticsProperty =
            DependencyProperty.Register(nameof(CombinedStatistics), typeof(IEnumerable<StatisticItem>), typeof(ManagerWindow));

        /// <summary>
        /// Gets or sets the collection of statistics items displayed in the dashboard.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="StatisticItem"/> containing both order status
        /// and schedule status statistics.
        /// </value>
        public IEnumerable<StatisticItem> CombinedStatistics
        {
            get { return (IEnumerable<StatisticItem>)GetValue(CombinedStatisticsProperty); }
            set { SetValue(CombinedStatisticsProperty, value); }
        }

        /// <summary>
        /// Dependency property for CurrentTime.
        /// </summary>
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(ManagerWindow));

        /// <summary>
        /// Gets or sets the current simulated system time.
        /// </summary>
        /// <value>
        /// A DateTime representing the current business clock time, which may differ from real-world time.
        /// </value>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        /// <summary>
        /// Dependency property for IsWindowEnabled.
        /// </summary>
        public static readonly DependencyProperty IsWindowEnabledProperty =
            DependencyProperty.Register(nameof(IsWindowEnabled), typeof(bool), typeof(ManagerWindow), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the window controls are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if controls are enabled; <c>false</c> if disabled (e.g., during database operations).
        /// </value>
        public bool IsWindowEnabled
        {
            get { return (bool)GetValue(IsWindowEnabledProperty); }
            set { SetValue(IsWindowEnabledProperty, value); }
        }

        /// <summary>
        /// Dependency property for ClockSpeed.
        /// </summary>
        public static readonly DependencyProperty ClockSpeedProperty =
            DependencyProperty.Register(nameof(ClockSpeed), typeof(int), typeof(ManagerWindow), new PropertyMetadata(1));

        /// <summary>
        /// Gets or sets the speed factor for the time simulator.
        /// </summary>
        /// <value>
        /// An integer multiplier indicating how many time units advance per real second (e.g., 60 = one minute per second).
        /// Default value is 1.
        /// </value>
        public int ClockSpeed
        {
            get { return (int)GetValue(ClockSpeedProperty); }
            set { SetValue(ClockSpeedProperty, value); }
        }

        /// <summary>
        /// Dependency property for RunStopSimulator.
        /// </summary>
        public static readonly DependencyProperty RunStopSimulatorProperty =
            DependencyProperty.Register(nameof(RunStopSimulator), typeof(bool), typeof(ManagerWindow), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the state of the time simulator.
        /// </summary>
        /// <value>
        /// <c>true</c> if the simulator is stopped (button shows "Start"); 
        /// <c>false</c> if the simulator is running (button shows "Stop").
        /// </value>
        public bool RunStopSimulator
        {
            get { return (bool)GetValue(RunStopSimulatorProperty); }
            set { SetValue(RunStopSimulatorProperty, value); }
        }

        #endregion

        #region Constructor & Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerWindow"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the administrator user.</param>
        /// <remarks>
        /// Initializes all command bindings using RelayCommand with appropriate execute and canExecute delegates.
        /// Commands requiring simulator to be stopped (Reset DB, Init DB, Clock Forward) have canExecute validation.
        /// Sets the DataContext to self for data binding and calls InitializeComponent to load the XAML UI.
        /// </remarks>
        public ManagerWindow(int id)
        {
            btnCourierList_Click = new Tools.RelayCommand(CourierList);
            btnOrderList_Click = new Tools.RelayCommand(OrderList);
            btnConfig_Click = new Tools.RelayCommand(Config);
            btnResetDB_Click = new Tools.RelayCommand(ResetDB, IsSimulatorStop);
            btnInitDB_Click = new Tools.RelayCommand(InitDB, IsSimulatorStop);
            btnClockForward_Click = new Tools.RelayCommand(ClockForward, IsSimulatorStop);
            btnSimulator_Click = new Tools.RelayCommand(Simulator);
            btnStatistic_Click = new Tools.RelayCommand(Statistic);

            _userId = id;

            DataContext = this;

            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the ManagerWindow.
        /// Initializes statistics template, performs initial data fetch, and registers observers.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Initialization sequence:
        /// <list type="number">
        /// <item><description>Creates initial statistics template with zero values if not already initialized</description></item>
        /// <item><description>Fetches current clock time and statistics</description></item>
        /// <item><description>Registers observers for real-time clock and statistics updates</description></item>
        /// </list>
        /// Observers ensure the dashboard reflects live data changes from the business logic layer.
        /// </remarks>
        private void ManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (CombinedStatistics == null)
            {
                CombinedStatistics = InitialStatsTemplate.ToList();
            }

            // Initial fetch
            ClockObserver();
            StatisticObserver();

            // Register observers
            Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));
            Tools.RunSafe(() => s_bl.Order.AddObserver(StatisticObserver));
        }

        /// <summary>
        /// Handles the Closed event of the ManagerWindow.
        /// Unregisters observers, stops the simulator, and cleans up resources.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Cleanup sequence:
        /// <list type="number">
        /// <item><description>Removes clock and statistics observers to prevent memory leaks</description></item>
        /// <item><description>Stops the time simulator asynchronously in the background</description></item>
        /// </list>
        /// </remarks>
        private void ManagerWindow_Close(object? sender, EventArgs e)
        {
            Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
            Tools.RunSafe(() => s_bl.Order.RemoveObserver(StatisticObserver));

            // Stop simulator in background
            Task.Run(() => s_bl.Admin.StopSimulator());
        }

        #endregion

        #region Observers & Logic

        /// <summary>
        /// Determines whether commands requiring the simulator to be stopped can execute.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <returns>
        /// <c>true</c> if the simulator is stopped and the command can execute; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Used as the CanExecute predicate for commands like ResetDB, InitDB, and ClockForward
        /// to prevent database operations and manual time changes while the simulator is running.
        /// </remarks>
        private bool IsSimulatorStop(object? parameter)
            => s_bl.Admin.IsSimulatorStop();

        /// <summary>
        /// Gets the initial template for statistics with all values set to zero.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="StatisticItem"/> containing entries for all
        /// <see cref="OrderStatus"/> and <see cref="ScheduleStatus"/> enum values.
        /// </value>
        /// <remarks>
        /// This template is used when the window first loads to display the statistics structure
        /// before actual data is fetched from the database. Each item is initialized with:
        /// <list type="bullet">
        /// <item><description>Id - The enum value (OrderStatus or ScheduleStatus)</description></item>
        /// <item><description>Name - A localized display name with description</description></item>
        /// <item><description>Value - Initially set to 0</description></item>
        /// </list>
        /// </remarks>
        private IEnumerable<StatisticItem> InitialStatsTemplate
        {
            get
            {
                var combinedList = new List<StatisticItem>();

                var orderValues = Enum.GetValues(typeof(BO.OrderStatus))
                                      .Cast<BO.OrderStatus>()
                                      .Select(e => new StatisticItem
                                      {
                                          Id = e,
                                          Name = "סטטוס הזמנה: " + Tools.GetDescription(e),
                                          Value = 0
                                      });
                combinedList.AddRange(orderValues);

                var scheduleValues = Enum.GetValues(typeof(BO.ScheduleStatus))
                                         .Cast<BO.ScheduleStatus>()
                                         .Select(e => new StatisticItem
                                         {
                                             Id = e,
                                             Name = "סטטוס לו\"ז: " + Tools.GetDescription(e),
                                             Value = 0
                                         });
                combinedList.AddRange(scheduleValues);

                return combinedList;
            }
        }

        /// <summary>
        /// Observer callback for clock updates.
        /// Asynchronously retrieves the current clock time and updates the UI.
        /// </summary>
        /// <remarks>
        /// This method is called automatically when the system clock advances (via ForwardClock or simulator).
        /// Uses ObserverMutex to prevent concurrent updates and race conditions.
        /// Runs on a background thread to avoid blocking the UI, with UI updates marshalled to the Dispatcher.
        /// If a new clock update request arrives while processing, it will be restarted after completion.
        /// </remarks>
        private void ClockObserver()
        {
            if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Task.Run(async () =>
            {
                var currentTime = s_bl.Admin.GetClock();

                _ = Dispatcher.BeginInvoke(() =>
                {
                    CurrentTime = currentTime;
                });

                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    ClockObserver();
            });
        }

        /// <summary>
        /// Observer callback for statistics updates.
        /// Asynchronously retrieves order statistics and updates the dashboard.
        /// </summary>
        /// <remarks>
        /// This method is called automatically when orders change in the system.
        /// Uses ObserverMutex to prevent concurrent updates and race conditions.
        /// Optimizes UI updates by only changing the Value property of existing StatisticItems
        /// when possible, rather than recreating the entire collection.
        /// Handles the BlNoAccessException by closing the window when database access is lost
        /// (e.g., after a database reset).
        /// </remarks>
        /// <exception cref="BlNoAccessException">
        /// Thrown when the database has been reset and the session is no longer valid.
        /// Causes the window to close and prompt the user to log in again.
        /// </exception>
        private void StatisticObserver()
        {
            if (_statsMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Task.Run(async () =>
            {
                try
                {
                    var newStatsValues = await s_bl.Order.GetAllOrderStatistic(_userId);

                    if (newStatsValues != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var currentList = CombinedStatistics as IList<StatisticItem>;

                            if (currentList != null && currentList.Count == newStatsValues.Count())
                            {
                                int i = 0;
                                foreach (var newValue in newStatsValues)
                                {
                                    if (currentList[i].Value != newValue)
                                    {
                                        currentList[i].Value = newValue;
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                var template = InitialStatsTemplate;
                                CombinedStatistics = template.Zip(newStatsValues, (item, count) =>
                                {
                                    item.Value = count;
                                    return item;
                                }).ToList();
                            }
                        });
                    }
                }
                catch (BlNoAccessException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("המערכת אותחלה מחדש נא להתחבר שוב", "התחברות", MessageBoxButton.OK, MessageBoxImage.Stop);
                        Close();
                    });
                }
                finally
                {
                    if (await _statsMutex.UnsetLoadInProgressAndCheckRestartRequested())
                        StatisticObserver();
                }
            });
        }

        #endregion

        #region UI Event Handlers (Navigation & Actions)

        /// <summary>
        /// Opens the Courier List window for managing couriers.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// Uses the Tools.OpenOrActivateWindow helper to ensure only one instance of CourierListWindow
        /// is open at a time, activating the existing window if already open.
        /// </remarks>
        private void CourierList(object? parameter)
            => Tools.OpenOrActivateWindow<CourierListWindow>(this);

        /// <summary>
        /// Opens the Order List window for managing orders.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// Opens the order list with no filters applied (all parameters set to null).
        /// Users can apply filters from within the OrderListWindow.
        /// </remarks>
        private void OrderList(object? parameter)
            => Tools.OpenOrActivateWindow<OrderListWindow>(this, (BO.OrderStatus?)null, (BO.ScheduleStatus?)null, (BO.TypeOfOrder?)null);

        /// <summary>
        /// Opens the Configuration window for managing system settings.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// Allows managers to modify store location, delivery settings, vehicle speeds,
        /// manager credentials, and other system configuration parameters.
        /// </remarks>
        private void Config(object? parameter)
            => Tools.OpenOrActivateWindow<ConfigWindow>(this);

        /// <summary>
        /// Toggles the time simulator on or off.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// When starting the simulator:
        /// <list type="bullet">
        /// <item><description>Uses the current ClockSpeed value as the simulation rate</description></item>
        /// <item><description>Sets RunStopSimulator to false to change button text to "Stop"</description></item>
        /// </list>
        /// When stopping the simulator:
        /// <list type="bullet">
        /// <item><description>Stops the automated time advancement</description></item>
        /// <item><description>Sets RunStopSimulator to true to change button text to "Start"</description></item>
        /// </list>
        /// Runs asynchronously to prevent blocking the UI thread.
        /// </remarks>
        private async void Simulator(object? parameter)
        {
            if (RunStopSimulator)
            {
                int speed = ClockSpeed;
                await Task.Run(() => s_bl.Admin.StartSimulator(speed));
                RunStopSimulator = false;
            }
            else
            {
                await Task.Run(() => s_bl.Admin.StopSimulator());
                RunStopSimulator = true;
            }
        }

        /// <summary>
        /// Advances the system clock by a specific time unit.
        /// </summary>
        /// <param name="parameter">
        /// A string indicating the time unit to advance by: "Minute", "Hour", "Day", "Week", "Month", or "Year".
        /// </param>
        /// <remarks>
        /// Only available when the simulator is stopped (enforced by command CanExecute).
        /// Advances the clock by exactly one unit of the specified type.
        /// All registered clock observers are notified of the change, triggering UI updates
        /// and business logic recalculations (delivery times, schedule status, etc.).
        /// </remarks>
        /// <exception cref="BO.BLTemporaryNotAvailableException">
        /// Thrown when the clock cannot be advanced at this time (e.g., simulator is running).
        /// </exception>
        /// <exception cref="BO.BlInvalidValueException">
        /// Thrown when an invalid time unit parameter is provided.
        /// </exception>
        private void ClockForward(object? parameter)
        {
            if (parameter is string value)
            {
                try
                {
                    var unit = value switch
                    {
                        "Minute" => BO.TimeUnit.MINUTE,
                        "Hour" => BO.TimeUnit.HOUR,
                        "Day" => BO.TimeUnit.DAY,
                        "Week" => BO.TimeUnit.WEEK,
                        "Month" => BO.TimeUnit.MONTH,
                        "Year" => BO.TimeUnit.YEAR,
                        _ => throw new BO.BlInvalidValueException("Invalid time unit")
                    };

                    s_bl.Admin.ForwardClock(unit);
                }
                catch (BO.BLTemporaryNotAvailableException ex)
                {
                    MessageBox.Show(ex.Message, "לא ניתן לעדכן", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "שגיאה חלון ניהול", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens the Order List window filtered by the clicked statistic.
        /// </summary>
        /// <param name="parameter">
        /// The statistic identifier, which can be:
        /// <list type="bullet">
        /// <item><description><see cref="BO.OrderStatus"/> - Filters by order status</description></item>
        /// <item><description><see cref="BO.ScheduleStatus"/> - Filters by schedule status</description></item>
        /// <item><description>Any other value - Opens unfiltered list</description></item>
        /// </list>
        /// </param>
        /// <remarks>
        /// This method is called when a user clicks on any statistic button in the dashboard.
        /// It intelligently determines the filter type based on the parameter and opens
        /// the OrderListWindow with the appropriate filter applied.
        /// </remarks>
        private void Statistic(object? parameter)
        {
            if (parameter is not null)
            {
                switch (parameter)
                {
                    case BO.OrderStatus orderStatus:
                        Tools.OpenOrActivateWindow<OrderListWindow>(this, orderStatus, (BO.ScheduleStatus?)null, (BO.TypeOfOrder?)null);
                        break;

                    case BO.ScheduleStatus scheduleStatus:
                        Tools.OpenOrActivateWindow<OrderListWindow>(this, (BO.OrderStatus?)null, scheduleStatus, (BO.TypeOfOrder?)null);
                        break;

                    default:
                        Tools.OpenOrActivateWindow<OrderListWindow>(this, (BO.OrderStatus?)null, (BO.ScheduleStatus?)null, (BO.TypeOfOrder?)null);
                        break;
                }
            }
        }

        /// <summary>
        /// Resets the database after user confirmation.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// This is a destructive operation that:
        /// <list type="bullet">
        /// <item><description>Displays a confirmation dialog before proceeding</description></item>
        /// <item><description>Deletes all data (orders, couriers, deliveries)</description></item>
        /// <item><description>Resets configuration to defaults</description></item>
        /// <item><description>Resets auto-increment ID counters</description></item>
        /// <item><description>Disables the window during the operation</description></item>
        /// <item><description>Triggers a system reset notification to close all open windows</description></item>
        /// <item><description>Refreshes statistics after completion</description></item>
        /// </list>
        /// </remarks>
        private async void ResetDB(object? parameter)
        {
            if (MessageBox.Show("Delete all data?", "ResetDB",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await PerformDbOperation(() => s_bl.Admin.ResetDB());
            }
        }

        /// <summary>
        /// Initializes the database with sample data after user confirmation.
        /// </summary>
        /// <param name="parameter">Command parameter (not used).</param>
        /// <remarks>
        /// This operation:
        /// <list type="bullet">
        /// <item><description>Displays a confirmation dialog before proceeding</description></item>
        /// <item><description>Calls ResetDB internally to ensure a clean state</description></item>
        /// <item><description>Populates the database with sample couriers, orders, and deliveries</description></item>
        /// <item><description>Disables the window during the operation</description></item>
        /// <item><description>Triggers a system reset notification to close all open windows</description></item>
        /// <item><description>Refreshes statistics after completion</description></item>
        /// </list>
        /// Useful for development, testing, and demonstration purposes.
        /// </remarks>
        private async void InitDB(object? parameter)
        {
            if (MessageBox.Show("Do you want to initialize all data?", "InitDB",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await PerformDbOperation(() => s_bl.Admin.InitializeDB());
            }
        }

        /// <summary>
        /// Helper method to execute database operations with proper UI feedback and error handling.
        /// </summary>
        /// <param name="dbAction">The asynchronous database operation to perform.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method provides a consistent pattern for database operations:
        /// <list type="number">
        /// <item><description>Disables window controls to prevent user interaction during the operation</description></item>
        /// <item><description>Triggers a reset notification to close all dependent windows</description></item>
        /// <item><description>Changes the cursor to a wait cursor for visual feedback</description></item>
        /// <item><description>Executes the database operation asynchronously on a background thread</description></item>
        /// <item><description>Displays error messages if the operation fails</description></item>
        /// <item><description>Re-enables window controls and restores the cursor when complete</description></item>
        /// <item><description>Refreshes statistics to reflect the changes</description></item>
        /// </list>
        /// </remarks>
        private async Task PerformDbOperation(Func<Task> dbAction)
        {
            IsWindowEnabled = false;
            Tools.TriggerReset();
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Task.Run(dbAction);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "שגיאה חלון ניהול");
            }
            finally
            {
                IsWindowEnabled = true;
                Mouse.OverrideCursor = null;
                StatisticObserver();
            }
        }

        #endregion
    }
}