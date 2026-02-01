using PL.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// Represents a window for creating new orders or viewing and editing existing orders.
/// </summary>
/// <remarks>
/// This window provides functionality for:
/// <list type="bullet">
/// <item><description>Creating new orders with customer details and delivery information</description></item>
/// <item><description>Viewing and updating existing order information</description></item>
/// <item><description>Canceling orders with optional SMS notification to the assigned courier</description></item>
/// <item><description>Real-time updates through the observer pattern when order data changes</description></item>
/// <item><description>Displaying delivery history for orders that have been assigned to couriers</description></item>
/// </list>
/// The window operates in two modes: Add mode (when id = 0) and Update mode (when id != 0).
/// Thread-safe UI updates are ensured using <see cref="ObserverMutex"/> and WPF Dispatcher.
/// </remarks>
public partial class OrderWindow : Window, INotifyPropertyChanged
{
    #region INotifyPropertyChanged Implementation

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Private Fields and Properties

    /// <summary>
    /// Business logic layer instance for accessing order operations.
    /// </summary>
    /// <remarks>
    /// This is a shared static instance retrieved from the factory.
    /// All BL operations are performed through this interface.
    /// </remarks>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The unique identifier of the order being displayed or edited.
    /// </summary>
    /// <remarks>
    /// A value of 0 indicates that this is a new order (Add mode).
    /// Any non-zero value indicates an existing order (Update mode).
    /// </remarks>
    private int CurrentID = 0;

    /// <summary>
    /// The ID of the currently logged-in manager.
    /// </summary>
    /// <remarks>
    /// This value is retrieved from the system configuration and is used
    /// for authorization checks when performing order operations.
    /// </remarks>
    private readonly int CURRENT_MANAGER_ID =
            Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().ManagerId);

    /// <summary>
    /// Mutex for thread-safe observer updates.
    /// </summary>
    /// <remarks>
    /// Prevents race conditions when multiple observer notifications occur simultaneously.
    /// Ensures that only one UI update runs at a time and tracks if another update is needed.
    /// </remarks>
    private readonly ObserverMutex _Mutex = new();

    /// <summary>
    /// Gets a value indicating whether the window is in update mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if the window is displaying an existing order for editing;
    /// <c>false</c> if the window is in add mode for creating a new order.
    /// </value>
    /// <remarks>
    /// This property is determined by the <see cref="ButtonText"/> value.
    /// It's used for conditional logic and UI binding.
    /// </remarks>
    public bool IsUpdateMode => ButtonText == "Update";

    #endregion

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the text displayed on the primary action button.
    /// </summary>
    /// <value>
    /// "Add" when creating a new order, or "Update" when editing an existing order.
    /// </value>
    /// <remarks>
    /// This property is data-bound to the button in the XAML and determines
    /// the button's text and behavior. It also affects the <see cref="IsUpdateMode"/> property.
    /// </remarks>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ButtonText"/> dependency property.
    /// </summary>
    /// <remarks>
    /// This property represents the text displayed on the primary action button within the
    /// <see cref="OrderWindow"/>. The default value is "Add".
    /// </remarks>
    public static readonly DependencyProperty ButtonTextProperty =
      DependencyProperty.Register(nameof(ButtonText), typeof(string),
          typeof(OrderWindow), new PropertyMetadata("Add"));

    /// <summary>
    /// Gets or sets the order currently being displayed or edited.
    /// </summary>
    /// <value>
    /// A <see cref="BO.Order"/> object containing all order details,
    /// or <c>null</c> if no order is loaded.
    /// </value>
    /// <remarks>
    /// This property is the primary data context for the window.
    /// All form controls are data-bound to properties of this object.
    /// Changes to this property automatically update the UI through data binding.
    /// </remarks>
    public BO.Order CurrentOrder
    {
        get => (BO.Order)GetValue(CurrentOrderProperty);
        set => SetValue(CurrentOrderProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CurrentOrder"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentOrderProperty =
         DependencyProperty.Register("CurrentOrder", typeof(BO.Order),
             typeof(OrderWindow), new PropertyMetadata(null));

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderWindow"/> class.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the order to display.
    /// Pass 0 to create a new order, or a valid order ID to edit an existing order.
    /// </param>
    /// <remarks>
    /// The actual initialization of the window content occurs in the
    /// <see cref="OrderWindow_Loaded"/> event handler after the UI elements are created.
    /// </remarks>
    public OrderWindow(int id = 0)
    {
        InitializeComponent();
        CurrentID = id;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Loaded event of the OrderWindow control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs different initialization based on the mode:
    /// <para><b>Update Mode (CurrentID != 0):</b></para>
    /// <list type="bullet">
    /// <item><description>Sets the button text to "Update"</description></item>
    /// <item><description>Loads the existing order data via <see cref="OrderObserver"/></description></item>
    /// <item><description>Registers observers for order-specific and clock changes</description></item>
    /// </list>
    /// <para><b>Add Mode (CurrentID = 0):</b></para>
    /// <list type="bullet">
    /// <item><description>Sets the button text to "Add"</description></item>
    /// <item><description>Initializes a new empty order with default values</description></item>
    /// <item><description>Sets the order date to the current system clock</description></item>
    /// <item><description>Calculates the maximum delivery time based on configuration</description></item>
    /// </list>
    /// </remarks>
    private void OrderWindow_Loaded(object sender, EventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        if (CurrentID != 0)
        {
            ButtonText = "Update";

            try
            {
                OrderObserver();
                s_bl.Order.AddObserver(CurrentID, OrderObserver);
                s_bl.Admin.AddClockObserver(OrderObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
                Close();
            }
        }
        else
        {
            ButtonText = "Add";

            CurrentOrder = new BO.Order()
            {
                Id = 0,
                Name = "",
                Phone = "",
                Addres = "",
                OrderStatus = BO.OrderStatus.OPEN,
                OrderDate = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock()),
                TimeLeftForDelivery = Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().MaxDeliveryTime),
                Weight = 0,
                TypeOfOrder = BO.TypeOfOrder.STANDART,
                ScheduleStatus = BO.ScheduleStatus.ONTYME,
                MaxDeliveryTime = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock() + s_bl.Admin.GetConfig().MaxDeliveryTime)
            };
        }
    }

    /// <summary>
    /// Handles the click event for the Cancel Order button.
    /// </summary>
    /// <param name="sender">The source of the event (the Cancel button).</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Prompts the user for confirmation before canceling</description></item>
    /// <item><description>Retrieves the SMS notification preference from the button's CommandParameter</description></item>
    /// <item><description>Calls the BL layer to cancel the order</description></item>
    /// <item><description>Displays success or error messages to the user</description></item>
    /// <item><description>Closes the window upon successful cancellation</description></item>
    /// </list>
    /// <para><b>Exception Handling:</b></para>
    /// <list type="bullet">
    /// <item><description><see cref="BO.BlDoesNotExistException"/>: Order not found</description></item>
    /// <item><description><see cref="BO.BlInvalidOperationException"/>: Order cannot be cancelled in current state</description></item>
    /// <item><description><see cref="BO.BLNoSendSmsException"/>: Order cancelled but SMS notification failed</description></item>
    /// </list>
    /// </remarks>
    private async void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה לבטל את ההזמנה הזו?",
            "אישור ביטול הזמנה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            var btn = sender as Button;
            bool StateToken = (btn?.CommandParameter as bool?).GetValueOrDefault();

            await s_bl.Order.Cancel(CURRENT_MANAGER_ID, CurrentID, StateToken);

            MessageBox.Show($"הזמנה מס' {CurrentID} בוטלה בהצלחה");

            Close();
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
            MessageBox.Show($"הזמנה מס' {CurrentID} בוטלה בהצלחה ({ex.Message})");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בביטול: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Closed event of the OrderWindow control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs cleanup operations to prevent memory leaks:
    /// <list type="bullet">
    /// <item><description>Unsubscribes from the global reset event</description></item>
    /// <item><description>Removes the order-specific observer (only in Update mode)</description></item>
    /// <item><description>Removes the clock observer</description></item>
    /// </list>
    /// All cleanup operations are wrapped in <see cref="Tools.RunSafe"/> to ensure
    /// exceptions during cleanup don't crash the application.
    /// </remarks>
    private void OrderWindow_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        if (CurrentID != 0)
            Tools.RunSafe(() => s_bl.Order.RemoveObserver(CurrentID, OrderObserver));
        Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(OrderObserver));
    }

    /// <summary>
    /// Handles the click event for the Add/Update button.
    /// </summary>
    /// <param name="sender">The source of the event (the Add/Update button).</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// This method performs different operations based on the button's Tag property:
    /// <para><b>Add Mode (Tag = "Add"):</b></para>
    /// <list type="bullet">
    /// <item><description>Validates that customer name is not empty</description></item>
    /// <item><description>Creates a new order in the system via the BL layer</description></item>
    /// <item><description>Displays success message</description></item>
    /// </list>
    /// <para><b>Update Mode (Tag != "Add"):</b></para>
    /// <list type="bullet">
    /// <item><description>Validates that customer name is not empty</description></item>
    /// <item><description>Updates the existing order via the BL layer</description></item>
    /// <item><description>Displays success message</description></item>
    /// </list>
    /// <para><b>Exception Handling:</b></para>
    /// <list type="bullet">
    /// <item><description><see cref="BO.BlInvalidValueException"/>: Invalid or missing data</description></item>
    /// <item><description><see cref="BO.BlAlreadyExistsException"/>: Order ID conflict (rare in practice)</description></item>
    /// <item><description>Other exceptions: Displays generic error message</description></item>
    /// </list>
    /// </remarks>
    private async void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentOrder == null || string.IsNullOrEmpty(CurrentOrder.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }

        try
        {
            if (sender is Button button && button.Tag.ToString() is "Add")
            {
                await s_bl.Order.Create(CURRENT_MANAGER_ID, CurrentOrder);
                MessageBox.Show("Order added successfully!");
            }
            else
            {
                await s_bl.Order.Update(CURRENT_MANAGER_ID, CurrentOrder);
                MessageBox.Show("Details updated successfully!");
            }

            this.Close();
        }
        catch (BO.BlInvalidValueException ex)
        {
            MessageBox.Show($"ערך חסר או לא חוקי: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlAlreadyExistsException ex)
        {
            MessageBox.Show($"שגיאה מסוג: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה כללית: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Observer Pattern

    /// <summary>
    /// Observer callback method that updates the UI when the order data changes.
    /// </summary>
    /// <remarks>
    /// This method implements the observer pattern for real-time UI updates.
    /// It is called in the following scenarios:
    /// <list type="bullet">
    /// <item><description>When the order data changes (registered via <see cref="BlApi.IOrder.AddObserver"/>)</description></item>
    /// <item><description>When the system clock advances (registered via <see cref="BlApi.IAdmin.AddClockObserver"/>)</description></item>
    /// </list>
    /// <para><b>Thread Safety:</b></para>
    /// The method uses <see cref="ObserverMutex"/> to ensure only one update runs at a time.
    /// If an update is already in progress, it sets a restart flag and returns immediately.
    /// After the current update completes, if a restart was requested, the observer runs again.
    /// <para><b>UI Thread Marshalling:</b></para>
    /// Uses <see cref="Dispatcher.BeginInvoke"/> to ensure UI updates occur on the UI thread.
    /// The actual data fetching is performed asynchronously to keep the UI responsive.
    /// <para><b>Exception Handling:</b></para>
    /// <list type="bullet">
    /// <item><description><see cref="BO.BlDoesNotExistException"/>: Order was deleted, closes the window</description></item>
    /// <item><description>Other exceptions: Displays error message but keeps window open</description></item>
    /// </list>
    /// </remarks>
    private void OrderObserver()
    {
        // Check if update is already in progress; if so, mark restart required and return
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Dispatcher.BeginInvoke(async () =>
        {
            bool windowIsOpen = true;

            try
            {
                CurrentOrder = await s_bl.Order.Read(CURRENT_MANAGER_ID, CurrentID)
                            ?? throw new BO.BlDoesNotExistException($"The Order with id: {CurrentID} does not exist");
            }
            catch (BO.BlDoesNotExistException)
            {
                windowIsOpen = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Release the mutex and check if another update was requested
                if (windowIsOpen is true && await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
                    OrderObserver();
            }
        });
    }

    #endregion
}

