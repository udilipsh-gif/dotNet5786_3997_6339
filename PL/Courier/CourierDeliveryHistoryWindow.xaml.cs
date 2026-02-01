using PL.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace PL;

/// <summary>
/// Interaction logic for CourierDeliveryHistoryWindow.xaml.
/// Displays a history of completed/closed deliveries for a specific courier.
/// </summary>
public partial class CourierDeliveryHistoryWindow : Window
{
    /// <summary>
    /// Access point to the Business Logic layer.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The ID of the courier whose history is being displayed.
    /// </summary>
    public int UserId { get; private init; }

    /// <summary>
    /// Retrieves the System Manager ID for permission verification when fetching data.
    /// </summary>
    private readonly int MANAGER_ID =
        Tools.GetSafeFromBl<int>(() => s_bl.Admin.GetConfig().ManagerId);

    /// <summary>
    /// Initializes a new instance of the CourierDeliveryHistoryWindow.
    /// </summary>
    /// <param name="couriorId">The unique ID of the courier.</param>
    public CourierDeliveryHistoryWindow(int couriorId)
    {
        UserId = couriorId;
        InitializeComponent();
    }

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the collection of past deliveries.
    /// This property is bound to the DataGrid in the XAML.
    /// </summary>
    public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesHistory
    {
        get => (ObservableCollection<BO.ClosedDeliveryInList>)GetValue(DeliveriesHistoryProperty);
        set => SetValue(DeliveriesHistoryProperty, value);
    }

    public static readonly DependencyProperty DeliveriesHistoryProperty =
        DependencyProperty.Register(nameof(DeliveriesHistory), typeof(ObservableCollection<BO.ClosedDeliveryInList>),
            typeof(CourierDeliveryHistoryWindow), new PropertyMetadata(null));

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Window Loaded event.
    /// Subscribes to the Order observer to receive real-time updates when deliveries change.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Tools.ResetRequested += () => this.Close();
            OrderObserver(); // Initial fetch
            s_bl.Order.AddObserver(OrderObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
            Close();
        }
    }

    /// <summary>
    /// Handles the Window Closed event.
    /// Unsubscribes from observers to prevent memory leaks.
    /// </summary>
    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            Tools.ResetRequested -= () => this.Close();
            s_bl.Order.RemoveObserver(OrderObserver);
        }
        catch (Exception)
        {
            Close();
        }
    }

    #endregion

    #region Data Loading Logic

    /// <summary>
    /// Mutex helper to prevent race conditions during asynchronous data loading.
    /// </summary>
    private readonly ObserverMutex _Mutex = new();

    /// <summary>
    /// Asynchronously fetches the closed delivery list for the courier and updates the UI.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ObserverMutex"/> to ensure that rapid updates do not conflict.
    /// Updates the <see cref="DeliveriesHistory"/> collection on the UI thread via Dispatcher.
    /// Handles errors gracefully by displaying a message box.
    /// </remarks>
    private async void OrderObserver()
    {
        // Prevent concurrent execution; mark restart if needed
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        try
        {
            // Fetch data from BL on a background thread
            var newList = await s_bl.Delivery.GetClosed(MANAGER_ID, UserId, null, null)
                          ?? throw new BO.BlDoesNotExistException($"The list for id: {UserId} does not exist");

            // Update UI on the main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DeliveriesHistory == null)
                {
                    DeliveriesHistory = new ObservableCollection<BO.ClosedDeliveryInList>(newList);
                }
                else
                {
                    DeliveriesHistory.Clear();
                    foreach (var item in newList)
                    {
                        DeliveriesHistory.Add(item);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string msg = (ex is BO.BlDoesNotExistException) ?
                             $"שגיאה בטעינת הנתונים: {ex.Message}" :
                             $"שגיאה כללית: {ex.Message}";

                MessageBox.Show(msg, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            // Release lock and re-run if an update was requested during execution
            if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
            {
                OrderObserver();
            }
        }
    }

    #endregion
}