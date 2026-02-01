using PL.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PL;

/// <summary>
/// Interaction logic for StartDeliveryWindow.xaml.
/// This window allows a courier to view available orders, filter them, and select one to start delivering.
/// </summary>
public partial class StartDeliveryWindow : Window
{
    /// <summary>
    /// Access point to the Business Logic layer.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Helps synchronize asynchronous updates to the order list to prevent race conditions.
    /// </summary>
    private readonly ObserverMutex _Mutex = new();

    #region Properties

    /// <summary>
    /// The ID of the user currently logged in.
    /// </summary>
    public int UserId { get; private init; }

    /// <summary>
    /// The specific courier ID associated with the user.
    /// </summary>
    private int courierId { get; init; }

    /// <summary>
    /// The type of shipment/vehicle the courier uses (affects which orders are shown).
    /// </summary>
    private BO.TheTypeShipment typeShipment { get; init; }

    /// <summary>
    /// Gets or sets the address of the store to be displayed in the UI.
    /// </summary>
    public string? StoreAddress
    {
        get { return (string)GetValue(StoreAddressProperty); }
        set { SetValue(StoreAddressProperty, value); }
    }

    public static readonly DependencyProperty StoreAddressProperty =
        DependencyProperty.Register(nameof(StoreAddress), typeof(string), typeof(StartDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Indicates whether the list of available orders is empty.
    /// Used to toggle visibility of the "No orders available" message in the UI.
    /// </summary>
    public bool OrderListEmpty
    {
        get { return (bool)GetValue(OrderListEmptyProperty); }
        set { SetValue(OrderListEmptyProperty, value); }
    }

    public static readonly DependencyProperty OrderListEmptyProperty =
        DependencyProperty.Register(nameof(OrderListEmpty), typeof(bool), typeof(StartDeliveryWindow), new PropertyMetadata(false));

    /// <summary>
    /// collection of filtering options (Enum values) for the ComboBox.
    /// </summary>
    public IEnumerable<Tools.SelectionItem> EnumTypeOfOrder
    {
        get => (IEnumerable<Tools.SelectionItem>)GetValue(EnumTypeOfOrderProperty);
        set => SetValue(EnumTypeOfOrderProperty, value);
    }

    public static readonly DependencyProperty EnumTypeOfOrderProperty =
        DependencyProperty.Register(nameof(EnumTypeOfOrder), typeof(IEnumerable<Tools.SelectionItem>),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// The currently selected filter from the ComboBox.
    /// Changes to this property trigger a list refresh.
    /// </summary>
    public BO.TypeOfOrder? SelectedFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelectedFilterProperty);
        set => SetValue(SelectedFilterProperty, value);
    }

    public static readonly DependencyProperty SelectedFilterProperty =
        DependencyProperty.Register(nameof(SelectedFilter), typeof(BO.TypeOfOrder?),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// The collection of orders displayed in the DataGrid.
    /// </summary>
    public ObservableCollection<BO.OpenOrderInList> DeliveryListView
    {
        get { return (ObservableCollection<BO.OpenOrderInList>)GetValue(DeliveryListViewProperty); }
        set { SetValue(DeliveryListViewProperty, value); }
    }

    public static readonly DependencyProperty DeliveryListViewProperty =
    DependencyProperty.Register(nameof(DeliveryListView), typeof(ObservableCollection<BO.OpenOrderInList>),
        typeof(StartDeliveryWindow), new PropertyMetadata(null));

    #endregion

    #region Constructor & Lifecycle

    /// <summary>
    /// Initializes a new instance of the StartDeliveryWindow.
    /// </summary>
    /// <param name="userId">The system user ID.</param>
    /// <param name="courierId">The courier entity ID.</param>
    /// <param name="TypeShipment">The transport method.</param>
    /// <param name="enumTypeOfOrder">List of order types for filtering.</param>
    public StartDeliveryWindow(int userId, int courierId, BO.TheTypeShipment TypeShipment, IEnumerable<Tools.SelectionItem>? enumTypeOfOrder)
    {
        this.UserId = userId;
        this.courierId = courierId;
        this.typeShipment = TypeShipment;
        this.EnumTypeOfOrder = enumTypeOfOrder ?? new List<Tools.SelectionItem>();
        this.StoreAddress = s_bl.Admin.GetConfig().StoreAddress;

        InitializeComponent();
    }

    /// <summary>
    /// Event handler for Window Loaded. Subscribes to observers and fetches initial data.
    /// </summary>
    private void StartDeliveryWindow_Loaded(object sender, EventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Order.AddObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Courier.AddObserver(UserId, orderListObserver));

        UpdateOrdersList();
    }

    /// <summary>
    /// Callback method invoked when the observable data changes.
    /// </summary>
    private void orderListObserver() => UpdateOrdersList();

    /// <summary>
    /// Event handler for Window Closed. Unsubscribes from observers to prevent memory leaks.
    /// </summary>
    private void StartDeliveryWindow_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Courier.RemoveObserver(UserId, orderListObserver));
    }

    #endregion

    #region Data Loading Logic

    /// <summary>
    /// Asynchronously fetches and updates the list of available orders.
    /// Uses <see cref="ObserverMutex"/> to handle rapid updates and ensure thread safety.
    /// </summary>
    private async void UpdateOrdersList()
    {
        BO.TypeOfOrder? selectedFilter = null;

        // Check if an update is already running. If so, mark that a restart is required and exit.
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        try
        {
            // Access UI elements on the main thread to get current filter
            Dispatcher.Invoke(() =>
            {
                selectedFilter = SelectedFilter;
            });

            // Perform heavy calculation on a background thread
            var Result = await Task.Run(async () =>
            {
                var DeliveryList = await s_bl.Delivery.GetOpen(UserId, courierId, selectedFilter, null);
                var storeAddress = s_bl.Admin.GetConfig().StoreAddress;
                return (storeAddress, DeliveryList);
            });

            // Update UI on the main thread
            await Dispatcher.BeginInvoke(() =>
            {
                StoreAddress = Result.storeAddress;
                if (DeliveryListView == null)
                {
                    DeliveryListView = new ObservableCollection<BO.OpenOrderInList>(Result.DeliveryList);
                }
                else
                {
                    DeliveryListView.Clear(); // Remove old items
                    foreach (var item in Result.DeliveryList)
                    {
                        DeliveryListView.Add(item); // Add new items
                    }
                }

                if (!Result.DeliveryList.Any())
                    OrderListEmpty = true;
                else
                    OrderListEmpty = false; // Reset if items exist
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}", "שגיאה",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DeliveryListView = new ObservableCollection<BO.OpenOrderInList>();
            });
        }
        finally
        {
            // Release the lock and check if another update was requested while we were working
            if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
                UpdateOrdersList();
        }
    }

    /// <summary>
    /// Handles the selection change event of the Filter ComboBox.
    /// triggers a reload of the order list.
    /// </summary>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => UpdateOrdersList();

    #endregion

    #region Popup & Interaction Logic

    private MapPopupWindow? _currentPopup;

    /// <summary>
    /// Handles the click event on a DataGrid row to show the Map Popup.
    /// </summary>
    /// <remarks>
    /// Checks if the click originated from a Button inside the row using <see cref="IsClickInsideButton"/>.
    /// If it was a button click, the popup is not shown.
    /// </remarks>
    private void DataGridRow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Prevent popup if the user clicked an action button (like "Collect")
        if (IsClickInsideButton(e.OriginalSource))
        {
            return;
        }

        // Close existing popup
        _currentPopup?.Close();
        _currentPopup = null;

        if (sender is DataGridRow row && row.Item is BO.OpenOrderInList selectedOrder)
        {
            var relativePoint = e.GetPosition(this);
            var screenPoint = PointToScreen(relativePoint);

            // Handle DPI scaling
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                double scaleX = source.CompositionTarget.TransformToDevice.M11;
                double scaleY = source.CompositionTarget.TransformToDevice.M22;

                screenPoint.X /= scaleX;
                screenPoint.Y /= scaleY;
            }

            // Create and position the popup
            _currentPopup = new MapPopupWindow(UserId, selectedOrder, typeShipment)
            {
                Left = screenPoint.X + 15,
                Top = screenPoint.Y + 15
            };

            // Subscribe to the collect action inside the popup
            _currentPopup.OnCollectClicked += (order) =>
            {
                CollectOrderInternal(order);
            };

            _currentPopup.Show();
        }
    }

    /// <summary>
    /// Helper method to determine if a clicked element is part of a Button control.
    /// Traverses the visual tree upwards.
    /// </summary>
    /// <param name="originalSource">The UI element that was clicked.</param>
    /// <returns>True if the click was inside a Button; otherwise, false.</returns>
    private bool IsClickInsideButton(object originalSource)
    {
        if (originalSource is DependencyObject depObj)
        {
            while (depObj != null)
            {
                // If we reached a Button, the click was on it
                if (depObj is Button)
                {
                    return true;
                }

                // If we reached the DataGridRow without finding a button, it's a row click
                if (depObj is DataGridRow)
                {
                    return false;
                }

                // Move up the visual tree
                depObj = VisualTreeHelper.GetParent(depObj);
            }
        }
        return false;
    }

    /// <summary>
    /// Event handler for the "Start Delivery" button directly in the list.
    /// </summary>
    private void CollectOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BO.OpenOrderInList selectedOrder)
        {
            CollectOrderInternal(selectedOrder);
        }
    }

    /// <summary>
    /// Executes the logic to assign the order to the courier and start the delivery.
    /// </summary>
    /// <param name="selectedOrder">The order to collect.</param>
    private async void CollectOrderInternal(BO.OpenOrderInList selectedOrder)
    {
        try
        {
            await s_bl.Delivery.StartDelivery(UserId, courierId, selectedOrder.OrderId);

            MessageBox.Show("המשלוח התחיל בהצלחה!", "הצלחה",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בהתחלת המשלוח: {ex.Message}", "שגיאה",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}