using PL.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PL;

/// <summary>
/// Represents a window for displaying and managing a filterable and sortable list of orders.
/// </summary>
/// <remarks>
/// This window provides functionality for:
/// <list type="bullet">
/// <item><description>Displaying all orders in a DataGrid with real-time updates</description></item>
/// <item><description>Filtering orders by status (Open, Delivering, Completed, etc.)</description></item>
/// <item><description>Filtering orders by schedule status (On Time, At Risk, Late)</description></item>
/// <item><description>Filtering orders by type (Standard, Fast Delivery, Immediate)</description></item>
/// <item><description>Opening individual orders for detailed view or editing</description></item>
/// <item><description>Canceling orders directly from the list with optional SMS notification</description></item>
/// <item><description>Automatic refresh when order data changes through observer pattern</description></item>
/// <item><description>Real-time time remaining updates via clock observer</description></item>
/// </list>
/// The window implements <see cref="IWindowUpdater"/> to support state updates when reactivated
/// through the <see cref="Tools.OpenOrActivateWindow{T}"/> method.
/// Thread-safe operations are ensured using <see cref="ObserverMutex"/> for both list and clock updates.
/// </remarks>
public partial class OrderListWindow : Window, IWindowUpdater
{
    #region Private Fields

    /// <summary>
    /// Business logic layer instance for accessing order and administrative operations.
    /// </summary>
    /// <remarks>
    /// This is a shared static instance retrieved from the factory.
    /// All BL operations are performed through this interface.
    /// </remarks>
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The ID of the currently logged-in manager.
    /// </summary>
    /// <remarks>
    /// This value is retrieved from the system configuration and is used
    /// for authorization checks when performing order operations.
    /// </remarks>
    private int CURRENT_MANAGER_ID = Tools.GetSafeFromBl<int>(() => s_bl.Admin.GetConfig().ManagerId);

    /// <summary>
    /// The current system clock time, used for calculating time remaining for deliveries.
    /// </summary>
    /// <remarks>
    /// This field is updated whenever the clock observer is triggered, allowing the window
    /// to calculate the buffer time and update the TimeLeftForDelivery for all orders.
    /// </remarks>
    private DateTime CURRENT_DATE;

    /// <summary>
    /// Mutex for thread-safe list loading operations.
    /// </summary>
    /// <remarks>
    /// Prevents race conditions when multiple order list update requests occur simultaneously.
    /// Ensures that only one list load runs at a time and tracks if another load is needed.
    /// </remarks>
    private readonly ObserverMutex _ListMutex = new();

    /// <summary>
    /// Mutex for thread-safe clock update operations.
    /// </summary>
    /// <remarks>
    /// Prevents race conditions when multiple clock update notifications occur simultaneously.
    /// Ensures that only one clock update runs at a time and tracks if another update is needed.
    /// </remarks>
    private readonly ObserverMutex _ClockMutex = new();

    #endregion

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the observable collection of orders displayed in the list.
    /// </summary>
    /// <value>
    /// An <see cref="ObservableCollection{T}"/> of <see cref="BO.OrderInList"/> items
    /// representing all orders that match the current filter criteria.
    /// </value>
    /// <remarks>
    /// This collection is data-bound to the DataGrid in the XAML.
    /// Using ObservableCollection ensures that UI updates automatically when items are added,
    /// removed, or modified. The collection is updated on the UI thread to prevent cross-thread exceptions.
    /// </remarks>
    public ObservableCollection<BO.OrderInList> OrderList
    {
        get { return (ObservableCollection<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="OrderList"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(ObservableCollection<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the currently selected schedule status filter.
    /// </summary>
    /// <value>
    /// A <see cref="BO.ScheduleStatus"/> value to filter by, or <c>null</c> to show all schedule statuses.
    /// </value>
    /// <remarks>
    /// This property is data-bound to a ComboBox in the XAML.
    /// When the value changes, the <see cref="ComboBox_SelectionChanged"/> event handler
    /// triggers a reload of the order list with the new filter applied.
    /// </remarks>
    public BO.ScheduleStatus? SelectedScheduleFilter
    {
        get => (BO.ScheduleStatus?)GetValue(SelectedScheduleFilterProperty);
        set => SetValue(SelectedScheduleFilterProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedScheduleFilter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedScheduleFilterProperty =
        DependencyProperty.Register(nameof(SelectedScheduleFilter), typeof(BO.ScheduleStatus?),
            typeof(OrderListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the currently selected order type filter.
    /// </summary>
    /// <value>
    /// A <see cref="BO.TypeOfOrder"/> value to filter by, or <c>null</c> to show all order types.
    /// </value>
    /// <remarks>
    /// This property is data-bound to a ComboBox in the XAML.
    /// Allows filtering orders by Standard, Fast Delivery, or Immediate delivery types.
    /// </remarks>
    public BO.TypeOfOrder? SelectedTypeFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelectedTypeFilterProperty);
        set => SetValue(SelectedTypeFilterProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedTypeFilter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedTypeFilterProperty =
        DependencyProperty.Register(nameof(SelectedTypeFilter), typeof(BO.TypeOfOrder?), typeof(OrderListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the currently selected order status filter.
    /// </summary>
    /// <value>
    /// An <see cref="BO.OrderStatus"/> value to filter by, or <c>null</c> to show all order statuses.
    /// </value>
    /// <remarks>
    /// This property is data-bound to a ComboBox in the XAML.
    /// Allows filtering orders by Open, Delivering, Completed, Refused, or Cancelled statuses.
    /// </remarks>
    public BO.OrderStatus? SelectedOrderStatusFilter
    {
        get => (BO.OrderStatus?)GetValue(SelectedOrderStatusFilterProperty);
        set => SetValue(SelectedOrderStatusFilterProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedOrderStatusFilter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedOrderStatusFilterProperty =
        DependencyProperty.Register(nameof(SelectedOrderStatusFilter), typeof(BO.OrderStatus?),
            typeof(OrderListWindow), new PropertyMetadata(null));

    #endregion

    #region Filter List Properties

    /// <summary>
    /// Gets the list of schedule status options for the filter ComboBox.
    /// </summary>
    /// <value>
    /// An enumerable of <see cref="Tools.SelectionItem"/> containing all <see cref="BO.ScheduleStatus"/>
    /// values with Hebrew descriptions, plus an "All" option with <c>null</c> ID.
    /// </value>
    /// <remarks>
    /// This property is data-bound to the ItemsSource of the schedule status filter ComboBox.
    /// The items include Hebrew descriptions from the enum's Description attributes.
    /// </remarks>
    public IEnumerable<Tools.SelectionItem> ScheduleStatusList
    {
        get => Tools.GetEnumList<BO.ScheduleStatus>("הכל");
    }

    /// <summary>
    /// Gets the list of order type options for the filter ComboBox.
    /// </summary>
    /// <value>
    /// An enumerable of <see cref="Tools.SelectionItem"/> containing all <see cref="BO.TypeOfOrder"/>
    /// values with Hebrew descriptions, plus an "All" option with <c>null</c> ID.
    /// </value>
    /// <remarks>
    /// This property is data-bound to the ItemsSource of the order type filter ComboBox.
    /// </remarks>
    public IEnumerable<Tools.SelectionItem> TypeOfOrderList
    {
        get => Tools.GetEnumList<BO.TypeOfOrder>("הכל");
    }

    /// <summary>
    /// Gets the list of order status options for the filter ComboBox.
    /// </summary>
    /// <value>
    /// An enumerable of <see cref="Tools.SelectionItem"/> containing all <see cref="BO.OrderStatus"/>
    /// values with Hebrew descriptions, plus an "All" option with <c>null</c> ID.
    /// </value>
    /// <remarks>
    /// This property is data-bound to the ItemsSource of the order status filter ComboBox.
    /// </remarks>
    public IEnumerable<Tools.SelectionItem> TypeOfOrderStatusList
    {
        get => Tools.GetEnumList<BO.OrderStatus>("הכל");
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderListWindow"/> class with specified filters.
    /// </summary>
    /// <param name="orderStatus">
    /// The initial order status filter, or <c>null</c> to show all statuses.
    /// </param>
    /// <param name="scheduleStatus">
    /// The initial schedule status filter, or <c>null</c> to show all schedule statuses.
    /// </param>
    /// <param name="typeOfOrder">
    /// The initial order type filter, or <c>null</c> to show all types.
    /// </param>
    /// <remarks>
    /// This constructor allows the window to be opened with pre-applied filters,
    /// useful when navigating from a status-specific view (e.g., showing only late orders).
    /// The actual order loading occurs in the <see cref="Window_Loaded"/> event handler.
    /// </remarks>
    public OrderListWindow(BO.OrderStatus? orderStatus, BO.ScheduleStatus? scheduleStatus, BO.TypeOfOrder? typeOfOrder)
    {
        SelectedScheduleFilter = scheduleStatus;
        SelectedTypeFilter = typeOfOrder;
        SelectedOrderStatusFilter = orderStatus;

        CURRENT_DATE = Tools.GetSafeFromBl<DateTime>(() => s_bl.Admin.GetClock());

        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderListWindow"/> class with no filters.
    /// </summary>
    /// <remarks>
    /// This parameterless constructor creates a window that initially displays all orders.
    /// It delegates to the main constructor with all filter parameters set to <c>null</c>.
    /// </remarks>
    public OrderListWindow() : this(null, null, null) { }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads orders from the business logic layer with the current filter settings.
    /// </summary>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="number">
    /// <item><description>Checks if a load is already in progress using <see cref="_ListMutex"/></description></item>
    /// <item><description>Captures current filter values from the UI thread</description></item>
    /// <item><description>Builds a filter predicate combining all active filters</description></item>
    /// <item><description>Queries the BL layer asynchronously for filtered results</description></item>
    /// <item><description>Updates the <see cref="OrderList"/> collection on the UI thread</description></item>
    /// <item><description>Handles any errors and displays appropriate messages</description></item>
    /// <item><description>Checks if another load was requested during this operation</description></item>
    /// </list>
    /// <para><b>Thread Safety:</b></para>
    /// Uses <see cref="ObserverMutex"/> to prevent concurrent loads and ensure only one update runs at a time.
    /// All UI operations (reading filter values, updating OrderList) are marshalled to the UI thread via Dispatcher.
    /// <para><b>Exception Handling:</b></para>
    /// Catches all exceptions and displays an error message box without crashing the application.
    /// </remarks>
    private async void LoadOrders()
    {
        if (_ListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        BO.ScheduleStatus? scheduleFilter = null;
        BO.TypeOfOrder? typeFilter = null;
        BO.OrderStatus? statusFilter = null;

        Dispatcher.Invoke(() =>
        {
            scheduleFilter = SelectedScheduleFilter;
            typeFilter = SelectedTypeFilter;
            statusFilter = SelectedOrderStatusFilter;
        });

        try
        {
            Func<BO.OrderInList, bool> filterPredicate = order =>
                (scheduleFilter == null || order.ScheduleStatus == scheduleFilter) &&
                (typeFilter == null || order.TypeOfOrder == typeFilter) &&
                (statusFilter == null || order.OrderStatus == statusFilter);

            var filteredResults = await s_bl.Order.ReadAll(CURRENT_MANAGER_ID, filterPredicate, BO.OrderInListField.OrderId);

            Dispatcher.Invoke(() =>
            {
                if (OrderList == null)
                    OrderList = new ObservableCollection<BO.OrderInList>(filteredResults);
                else
                {
                    OrderList.Clear();
                    foreach (var item in filteredResults)
                        OrderList.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show($"Error loading orders: {ex.Message}", "שגיאה ברשימת ההזמנות"));
        }
        finally
        {
            if (await _ListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                LoadOrders();
        }
    }

    /// <summary>
    /// Observer callback method that updates time remaining for all orders when the system clock advances.
    /// </summary>
    /// <remarks>
    /// This method is called whenever the system clock changes (via <see cref="BlApi.IAdmin.AddClockObserver"/>).
    /// It performs the following operations:
    /// <list type="number">
    /// <item><description>Checks if a clock update is already in progress using <see cref="_ClockMutex"/></description></item>
    /// <item><description>Retrieves the new clock time from the BL layer</description></item>
    /// <item><description>Calculates the time buffer (difference between new and old clock)</description></item>
    /// <item><description>Updates TimeLeftForDelivery for all orders in the list by subtracting the buffer</description></item>
    /// <item><description>Updates <see cref="CURRENT_DATE"/> to the new clock value</description></item>
    /// <item><description>Checks if another update was requested during this operation</description></item>
    /// </list>
    /// <para><b>Thread Safety:</b></para>
    /// Uses <see cref="ObserverMutex"/> to prevent concurrent updates.
    /// All access to <see cref="OrderList"/> is marshalled to the UI thread via Dispatcher.InvokeAsync.
    /// <para><b>Exception Handling:</b></para>
    /// Catches exceptions and writes them to the debug output without displaying to the user,
    /// as clock updates should not interrupt the user experience.
    /// <para><b>UI Updates:</b></para>
    /// The TimeLeftForDelivery changes trigger PropertyChanged events in the <see cref="BO.OrderInList"/> objects,
    /// which automatically update the DataGrid display through data binding.
    /// </remarks>
    private async void ClockObserver()
    {
        if (_ClockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        try
        {
            var newDate = s_bl.Admin.GetClock();
            var buffer = newDate - CURRENT_DATE;

            // Access OrderList on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                if (OrderList != null)
                {
                    foreach (var order in OrderList)
                    {
                        order.TimeLeftForDelivery -= buffer;
                    }
                }
            });

            CURRENT_DATE = newDate;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clock Error: {ex.Message}");
        }
        finally
        {
            if (await _ClockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                ClockObserver();
        }
    }

    /// <summary>
    /// Observer callback method that reloads the order list when order data changes.
    /// </summary>
    /// <remarks>
    /// This method is registered with the BL layer's order observer system and is called whenever:
    /// <list type="bullet">
    /// <item><description>An order is created, updated, or deleted</description></item>
    /// <item><description>An order's status changes</description></item>
    /// <item><description>A delivery is assigned or completed</description></item>
    /// </list>
    /// It simply delegates to <see cref="LoadOrders"/> to refresh the entire list.
    /// This ensures the UI always displays the most current data.
    /// </remarks>
    private void orderListObserver()
        => LoadOrders();

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Loaded event of the OrderListWindow control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs initialization operations:
    /// <list type="bullet">
    /// <item><description>Subscribes to the global reset event to close the window on database reset</description></item>
    /// <item><description>Registers the order list observer for real-time updates</description></item>
    /// <item><description>Registers the clock observer for time-based updates</description></item>
    /// <item><description>Performs initial load of orders with current filter settings</description></item>
    /// </list>
    /// All observer registrations are wrapped in <see cref="Tools.RunSafe"/> to ensure
    /// exceptions during registration don't prevent the window from opening.
    /// </remarks>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Order.AddObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));

        orderListObserver();
    }

    /// <summary>
    /// Handles the Closed event of the OrderListWindow control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs cleanup operations to prevent memory leaks:
    /// <list type="bullet">
    /// <item><description>Unsubscribes from the global reset event</description></item>
    /// <item><description>Removes the order list observer</description></item>
    /// <item><description>Removes the clock observer</description></item>
    /// </list>
    /// All cleanup operations are wrapped in <see cref="Tools.RunSafe"/> to ensure
    /// exceptions during cleanup don't prevent the window from closing properly.
    /// </remarks>
    private void Window_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
    }

    /// <summary>
    /// Handles the SelectionChanged event of filter ComboBoxes.
    /// </summary>
    /// <param name="sender">The source of the event (a filter ComboBox).</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This event handler is triggered when the user changes any of the filter ComboBoxes
    /// (order status, schedule status, or order type). It reloads the order list with the
    /// newly selected filter values.
    /// </remarks>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => orderListObserver();

    /// <summary>
    /// Handles the click event for adding a new order or editing an existing order from the list.
    /// </summary>
    /// <param name="sender">
    /// The source of the event, either a button for adding new order or a DataGridRow for editing.
    /// </param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method supports two scenarios:
    /// <para><b>Edit Existing Order:</b></para>
    /// When triggered from a DataGridRow (double-click or button), opens the <see cref="OrderWindow"/>
    /// in edit mode with the selected order's ID.
    /// <para><b>Add New Order:</b></para>
    /// When triggered from an "Add" button, opens the <see cref="OrderWindow"/> in create mode (ID = 0).
    /// <para><b>Window Management:</b></para>
    /// Uses <see cref="Tools.SetSoftOwner"/> to establish a soft owner relationship,
    /// ensuring the order window is centered on this list window and closes when the list window closes.
    /// </remarks>
    private void Add_Edit_Order_Click(object sender, RoutedEventArgs e)
    {
        OrderWindow orderWindow;
        if (sender is DataGridRow row
            && row.Item is BO.OrderInList orderInList)
        {
            orderWindow = new OrderWindow(orderInList.OrderId);
        }
        else
        {
            orderWindow = new OrderWindow(0);
        }
        Tools.SetSoftOwner(orderWindow, this);
        orderWindow.Show();
    }

    /// <summary>
    /// Handles the click event for the Cancel Order button.
    /// </summary>
    /// <param name="sender">
    /// The source of the event (the Cancel button in the DataGrid row).
    /// </param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="number">
    /// <item><description>Prompts the user for confirmation before canceling</description></item>
    /// <item><description>Retrieves the SMS notification preference from the button's CommandParameter</description></item>
    /// <item><description>Extracts the order ID from the button's DataContext</description></item>
    /// <item><description>Calls the BL layer to cancel the order</description></item>
    /// <item><description>Displays success or error messages to the user</description></item>
    /// </list>
    /// <para><b>Button Context:</b></para>
    /// The button must have its DataContext set to a <see cref="BO.OrderInList"/> object,
    /// which is automatically provided by the DataGrid row binding.
    /// <para><b>CommandParameter:</b></para>
    /// The button's CommandParameter should be bound to a CheckBox's IsChecked property
    /// to indicate whether to send an SMS notification to the assigned courier.
    /// <para><b>Exception Handling:</b></para>
    /// <list type="bullet">
    /// <item><description><see cref="BO.BlDoesNotExistException"/>: Order not found</description></item>
    /// <item><description><see cref="BO.BlInvalidOperationException"/>: Order cannot be cancelled in current state</description></item>
    /// <item><description><see cref="BO.BLNoSendSmsException"/>: Order cancelled but SMS notification failed</description></item>
    /// </list>
    /// The list will automatically refresh after cancellation due to the observer pattern.
    /// </remarks>
    private async void btnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה לבטל את ההזמנה הזו?",
            "הזמנה בוטלה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        if (sender is Button button
            && button.DataContext is BO.OrderInList orderInList)
        {
            bool StateToken = (button.CommandParameter as bool?).GetValueOrDefault();
            try
            {
                await s_bl.Order.Cancel(CURRENT_MANAGER_ID, orderInList.OrderId, StateToken);
                MessageBox.Show($"הזמנה מס' {orderInList.OrderId} בוטלה בהצלחה");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (BO.BlInvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (BO.BLNoSendSmsException ex)
            {
                MessageBox.Show($"הזמנה מס' {orderInList.OrderId} בוטלה בהצלחה ({ex.Message})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error canceling the order: {ex.Message}");
            }
        }
    }

    #endregion

    #region IWindowUpdater Implementation

    /// <summary>
    /// Updates the window's filter state with new values and refreshes the order list.
    /// </summary>
    /// <param name="args">
    /// An array of arguments where:
    /// <list type="bullet">
    /// <item><description>args[0]: <see cref="BO.OrderStatus"/> filter value (or null for all)</description></item>
    /// <item><description>args[1]: <see cref="BO.ScheduleStatus"/> filter value (or null for all)</description></item>
    /// <item><description>args[2]: <see cref="BO.TypeOfOrder"/> filter value (or null for all)</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// This method implements the <see cref="IWindowUpdater"/> interface, allowing the window's
    /// state to be updated when it is reactivated through <see cref="Tools.OpenOrActivateWindow{T}"/>.
    /// Instead of creating a new window instance, the existing window can be updated with new filter values.
    /// <para><b>Example Usage:</b></para>
    /// When a user clicks "Show Late Orders" from another window, the system can reactivate this
    /// window with the appropriate filters rather than opening a duplicate window.
    /// <para><b>Parameter Validation:</b></para>
    /// If the args array is null or has fewer than 3 elements, the method does nothing.
    /// </remarks>
    public void UpdateState(params object?[] args)
    {
        if (args != null && args.Length >= 3)
        {
            SelectedOrderStatusFilter = args[0] as BO.OrderStatus?;
            SelectedScheduleFilter = args[1] as BO.ScheduleStatus?;
            SelectedTypeFilter = args[2] as BO.TypeOfOrder?;

            LoadOrders();
        }
    }

    #endregion
}
