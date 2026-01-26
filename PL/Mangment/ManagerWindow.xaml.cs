using BO;
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
    /// Interaction logic for ManagerWindow.xaml - The central administrative dashboard.
    /// </summary>
    /// <remarks>
    /// This window provides the main interface for system administrators.
    /// Key features include:
    /// <list type="bullet">
    /// <item><description>System Clock Management (View, Forward, Simulate)</description></item>
    /// <item><description>Real-time Statistics Dashboard (Orders, Schedule)</description></item>
    /// <item><description>Navigation to Sub-Modules (Couriers, Orders, Config)</description></item>
    /// <item><description>Database Operations (Reset, Initialize)</description></item>
    /// </list>
    /// </remarks>
    public partial class ManagerWindow : Window
    {
        #region Private Fields & Constants

        /// <summary>
        /// The ID of the currently logged-in administrator.
        /// </summary>
        private readonly int _userId;

        /// <summary>
        /// Instance of the Business Logic layer.
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Mutex for synchronizing clock updates to prevent race conditions.
        /// </summary>
        private readonly ObserverMutex _clockMutex = new();

        /// <summary>
        /// Mutex for synchronizing statistics updates.
        /// </summary>
        private readonly ObserverMutex _statsMutex = new();

        #endregion

        #region Inner Types

        /// <summary>
        /// Represents a single item in the statistics dashboard.
        /// Used for binding to the ItemsControl in the UI.
        /// </summary>
        public class StatisticItem : INotifyPropertyChanged
        {
            public object Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;

            private int _value;
            public int Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnPropertyChanged(); // <--- זה הקסם שמעדכן רק את המספר
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Collection of statistics items displayed in the dashboard.
        /// </summary>
        public IEnumerable<StatisticItem> CombinedStatistics
        {
            get { return (IEnumerable<StatisticItem>)GetValue(CombinedStatisticsProperty); }
            set { SetValue(CombinedStatisticsProperty, value); }
        }

        public static readonly DependencyProperty CombinedStatisticsProperty =
            DependencyProperty.Register(nameof(CombinedStatistics), typeof(IEnumerable<StatisticItem>), typeof(ManagerWindow));

        /// <summary>
        /// The current simulated system time.
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(ManagerWindow));

        /// <summary>
        /// Controls whether the window controls are enabled (e.g., disabled during DB reset).
        /// </summary>
        public bool IsWindowEnabled
        {
            get { return (bool)GetValue(IsWindowEnabledProperty); }
            set { SetValue(IsWindowEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsWindowEnabledProperty =
            DependencyProperty.Register(nameof(IsWindowEnabled), typeof(bool), typeof(ManagerWindow), new PropertyMetadata(true));

        /// <summary>
        /// The speed factor for the time simulator.
        /// </summary>
        public int ClockSpeed
        {
            get { return (int)GetValue(ClockSpeedProperty); }
            set { SetValue(ClockSpeedProperty, value); }
        }

        public static readonly DependencyProperty ClockSpeedProperty =
            DependencyProperty.Register(nameof(ClockSpeed), typeof(int), typeof(ManagerWindow), new PropertyMetadata(1));

        /// <summary>
        /// State of the simulator (True = Running, False = Stopped).
        /// </summary>
        public bool RunStopSimulator
        {
            get { return (bool)GetValue(RunStopSimulatorProperty); }
            set { SetValue(RunStopSimulatorProperty, value); }
        }

        public static readonly DependencyProperty RunStopSimulatorProperty =
            DependencyProperty.Register(nameof(RunStopSimulator), typeof(bool), typeof(ManagerWindow), new PropertyMetadata(true));

        #endregion

        #region Constructor & Lifecycle

        /// <summary>
        /// Initializes a new instance of the ManagerWindow.
        /// </summary>
        /// <param name="id">The ID of the user logging in.</param>
        public ManagerWindow(int id)
        {
            InitializeComponent();
            _userId = id;
            DataContext = this;
        }

        /// <summary>
        /// Called when the window is loaded. Initializes observers.
        /// </summary>
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
        /// Called when the window is closed. Cleans up observers and stops simulator.
        /// </summary>
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
        /// Helper property to generate the initial template for statistics (with 0 values).
        /// </summary>
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
        /// Handles UI thread synchronization using Dispatcher and Mutex.
        /// </summary>
        private void ClockObserver()
        {
            if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Task.Run(async() =>
            {
                var currentTime = s_bl.Admin.GetClock();

                _=Dispatcher.BeginInvoke(() =>
                {
                    CurrentTime = currentTime;

                });

                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    ClockObserver();

            });
        }

        /// <summary>
        /// Observer callback for statistic updates.
        /// Handles data fetching, UI binding, and error handling for DB access.
        /// </summary>
        private void StatisticObserver()
        {
            if (_statsMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Task.Run(async () =>
            {
                try
                {
                    // שליפת הנתונים החדשים מה-BL
                    var newStatsValues = await s_bl.Order.GetAllOrderStatistic(_userId);

                    if (newStatsValues != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // המרה לרשימה כדי שנוכל לגשת לפי אינדקס
                            var currentList = CombinedStatistics as IList<StatisticItem>;

                            if (currentList != null && currentList.Count == newStatsValues.Count())
                            {
                                int i = 0;
                                foreach (var newValue in newStatsValues)
                                {
                                    // עדכון הערך בלבד - ה-UI יתעדכן אוטומטית בגלל ה-PropertyChanged
                                    if (currentList[i].Value != newValue)
                                    {
                                        currentList[i].Value = newValue;
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                // מקרה חירום: אם הרשימות לא תואמות באורך, נבנה מחדש (כמו בקוד הישן)
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
        /// Opens the Courier List window.
        /// </summary>
        private void btnCourierList_Click(object sender, RoutedEventArgs e)
            => Tools.OpenOrActivateWindow<CourierListWindow>(this);

        /// <summary>
        /// Opens the Order List window.
        /// </summary>
        private void btnOrderList_Click(object sender, RoutedEventArgs e)
            => Tools.OpenOrActivateWindow<OrderListWindow>(this);

        /// <summary>
        /// Opens the Configuration window.
        /// </summary>
        private void btnConfig_Click(object sender, RoutedEventArgs e)
            => Tools.OpenOrActivateWindow<ConfigWindow>(this);

        /// <summary>
        /// Toggles the Time Simulator On/Off.
        /// </summary>
        private async void btnSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null) return;

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
        /// Advances the system clock by a specific unit based on the button clicked.
        /// </summary>
        private void ClockForward_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
            {
                string tagValue = element.Tag.ToString() ?? string.Empty;

                try
                {
                    var unit = tagValue switch
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
                    MessageBox.Show(ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Filters the order list based on the clicked statistic button.
        /// </summary>
        private void btnStatistic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
            {
                switch (element.Tag)
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
        private async void ResetDB(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete all data?", "ResetDB",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await PerformDbOperation(() => s_bl.Admin.ResetDB());
            }
        }

        /// <summary>
        /// Initializes the database with sample data.
        /// </summary>
        private async void InitDB(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to initialize all data?", "InitDB",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await PerformDbOperation(() => s_bl.Admin.InitializeDB());
            }
        }

        /// <summary>
        /// Helper method to execute DB operations with UI blocking and cursor updates.
        /// </summary>
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
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsWindowEnabled = true;
                Mouse.OverrideCursor = null;
                StatisticObserver(); // Force refresh stats
            }
        }

        #endregion
    }
}