using BO;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PL;

/// <summary>
/// Interaction logic for ManagerWindow.xaml - the main application window and navigation hub.
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
public partial class ManagerWindow : Window
{

    int UserId = 0;

    public class StatisticItem
    {
        public object Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }


    public IEnumerable<StatisticItem> EnumForStatistic
    {
        get
        {
            var combinedList = new List<StatisticItem>(); // שינינו ל-StatisticItem

            var orderValues = Enum.GetValues(typeof(BO.OrderStatus))
                                  .Cast<BO.OrderStatus>()
                                  .Select(e => new StatisticItem // יצירת המופע האמיתי
                                  {
                                      Id = e,
                                      Name = "סטטוס הזמנה: " + Tools.GetDescription(e),
                                      Value = 0 
                                  });

            combinedList.AddRange(orderValues);

            var scheduleValues = Enum.GetValues(typeof(BO.ScheduleStatus))
                                     .Cast<BO.ScheduleStatus>()
                                     .Select(e => new StatisticItem // יצירת המופע האמיתי
                                     {
                                         Id = e,
                                         Name = "סטטוס לו\"ז: " + Tools.GetDescription(e),
                                         Value = 0
                                     });

            combinedList.AddRange(scheduleValues);

            return combinedList;
        }
    }

    public IEnumerable<StatisticItem> CombinedStatistics
    {
        get { return (IEnumerable<StatisticItem>)GetValue(CombinedStatisticsProperty); }
        set { SetValue(CombinedStatisticsProperty, value); }
    }

    public static readonly DependencyProperty CombinedStatisticsProperty =
        DependencyProperty.Register(nameof(CombinedStatistics),
            typeof(IEnumerable<StatisticItem>),
            typeof(ManagerWindow));


    /// <summary>
    /// Initializes a new instance of the ManagerWindow class.
    /// </summary>
    public ManagerWindow(int id)
    {
        InitializeComponent();

        UserId = id;

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
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(ManagerWindow));


    /// <summary>
    /// Handles the window close event, performs cleanup operations.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item><description>Removes clock observer registration</description></item>
    /// <item><description>Removes configuration observer registration</description></item>
    /// </list>
    /// Ensures proper cleanup to prevent memory leaks.
    /// </remarks>
    private void ManagerWindow_Close(object? sender, EventArgs e)
    {
        Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(StatisticObserver));
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
    private void ManagerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ClockObserver();
        StatisticObserver();
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));
        Tools.RunSafe(() => s_bl.Order.AddObserver(StatisticObserver));
    }

    /// <summary>
    /// Observer callback method for clock changes, updates the displayed time.
    /// </summary>
    private void ClockObserver() => CurrentTime = s_bl.Admin.GetClock();

    private void StatisticObserver()
    {
        int[]? newStats = null;
        try
        {
             newStats = s_bl.Order.GetAllOrderStatistic(UserId);
        }
        catch(BlNoAccessException)
        {
            MessageBox.Show("המערכת אותחלה מחדש נא להתחבר שוב", "התחברות", MessageBoxButton.OK, MessageBoxImage.Stop);
            this.Close();
        }
        var enumList = EnumForStatistic;

        if (enumList != null && newStats != null)
        {
            var resultList = enumList.Zip(newStats, (labelObj, count) => new StatisticItem
            {
                Id = labelObj.Id,     // אין צורך ב-dynamic
                Name = labelObj.Name, // אין צורך ב-dynamic
                Value = count         // העדכון מהסטטיסטיקה
            }).ToList();

            CombinedStatistics = resultList;
        }
    }

    /// <summary>
    /// Handles the courier list button click event, opens the courier management window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void btnCourierList_Click(object sender, RoutedEventArgs e)
        => Tools.OpenOrActivateWindow<CourierListWindow>(this);


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
                StatisticObserver();
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
                StatisticObserver();
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
    /// Handles the order list button click event, opens the order management window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnOrderList_Click(object sender, RoutedEventArgs e)
        => Tools.OpenOrActivateWindow<OrderListWindow>(this);

    private void btnStatistic_Click(object sender, RoutedEventArgs e)
    {
        // בדיקה שהשולח הוא אכן כפתור או אלמנט עם Tag
        if (sender is FrameworkElement element && element.Tag != null)
        {
            // שליפת האובייקט המקורי (ה-Enum) מתוך ה-Tag
            var tagValue = element.Tag;

            // שימוש ב-Pattern Matching כדי לבדוק את הסוג
            switch (tagValue)
            {
                // מקרה 1: ה-Tag הוא מסוג OrderStatus
                case BO.OrderStatus orderStatus:
                    Tools.OpenOrActivateWindow<OrderListWindow>(this, orderStatus, (BO.ScheduleStatus?)null, (BO.TypeOfOrder?)null);
                    break;

                // מקרה 2: ה-Tag הוא מסוג ScheduleStatus
                case BO.ScheduleStatus scheduleStatus:
                    Tools.OpenOrActivateWindow<OrderListWindow>(this, (BO.OrderStatus?)null, scheduleStatus, (BO.TypeOfOrder?)null);
                    break;

                // מקרה ברירת מחדל (למשל אם נלחץ משהו אחר או null)
                default:
                    Tools.OpenOrActivateWindow<OrderListWindow>(this, (BO.OrderStatus?)null, (BO.ScheduleStatus?)null, (BO.TypeOfOrder?)null);
                    break;
            }
        }
    }

    private void btnConfig_Click(object sender, RoutedEventArgs e)
        => Tools.OpenOrActivateWindow<ConfigWindow>(this);
}
