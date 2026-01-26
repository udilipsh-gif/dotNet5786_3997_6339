using PL.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// Interaction logic for CourierWindow.xaml - provides CRUD operations for courier management.
/// </summary>
/// <remarks>
/// This window allows managers to add, view, edit, and delete courier information.
/// Couriers can also edit their own information through this window.
/// Implements INotifyPropertyChanged for dynamic UI updates and uses the Observer pattern
/// for real-time synchronization with the business logic layer.
/// </remarks>
public partial class CourierWindow : Window, INotifyPropertyChanged
{
    #region Services & Constants
    
    /// <summary>
    /// Business logic layer interface instance for accessing courier services.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    
    /// <summary>
    /// The unique identifier of the current manager performing the operation.
    /// </summary>
    private readonly int CURRENT_MANAGER_ID = Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().ManagerId);
    
    /// <summary>
    /// Current date and time from the system clock.
    /// </summary>
    private static DateTime CURRENT_DATE = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock());
    
    /// <summary>
    /// The unique identifier of the courier being managed.
    /// Zero indicates add mode; non-zero indicates update mode.
    /// </summary>
    private int CURRENT_ID = 0;
    
    /// <summary>
    /// Mutex for managing concurrent observer callbacks and preventing race conditions.
    /// </summary>
    private readonly ObserverMutex _Mutex = new();
    
    #endregion

    #region INotifyPropertyChanged Implementation
    
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. If omitted, uses the caller member name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #endregion

    #region Properties

    /// <summary>
    /// Backing field for the CurrentCourier property.
    /// </summary>
    private BO.Courier? _currentCourier;
    
    /// <summary>
    /// Gets or sets the current courier entity being displayed and managed.
    /// </summary>
    /// <value>
    /// The courier object containing all courier information including personal details,
    /// delivery statistics, and current order information.
    /// </value>
    public BO.Courier? CurrentCourier
    {
        get => _currentCourier;
        set
        {
            if (_currentCourier != value)
            {
                _currentCourier = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Backing field for the ButtonText property.
    /// </summary>
    private string _buttonText = "Add";
    
    /// <summary>
    /// Gets or sets the text displayed on the submit button.
    /// </summary>
    /// <value>
    /// "Add" when creating a new courier, "Update" when modifying an existing courier.
    /// </value>
    public string ButtonText
    {
        get => _buttonText;
        set
        {
            if (_buttonText != value)
            {
                _buttonText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUpdateMode));
            }
        }
    }

    /// <summary>
    /// Backing field for the DisplayTimeRemaining property.
    /// </summary>
    private TimeSpan? _displayTimeRemaining;
    
    /// <summary>
    /// Gets or sets the time remaining for the courier's current delivery.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the time left to complete the delivery,
    /// or <c>null</c> if no delivery is in progress.
    /// </value>
    public TimeSpan? DisplayTimeRemaining
    {
        get => _displayTimeRemaining;
        set
        {
            if (_displayTimeRemaining != value)
            {
                _displayTimeRemaining = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the window is in update mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if updating an existing courier; <c>false</c> if adding a new courier.
    /// </value>
    public bool IsUpdateMode => ButtonText == "Update";

    /// <summary>
    /// Gets the list of available vehicle types for courier selection.
    /// </summary>
    /// <value>
    /// An enumerable collection of <see cref="BO.TheTypeShipment"/> values.
    /// </value>
    public IEnumerable<BO.TheTypeShipment> VehicleTypesList { get; } =
        Enum.GetValues(typeof(BO.TheTypeShipment)).Cast<BO.TheTypeShipment>();

    #endregion

    #region Constructor & Loading

    /// <summary>
    /// Initializes a new instance of the <see cref="CourierWindow"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to edit, or 0 to create a new courier.</param>
    public CourierWindow(int id = 0)
    {
        InitializeComponent();
        DataContext = this;
        CURRENT_ID = id;
    }

    /// <summary>
    /// Handles the Loaded event of the CourierWindow.
    /// Initializes the window state, loads courier data if in update mode, and registers observers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// In update mode (CURRENT_ID != 0): Loads existing courier data and subscribes to real-time updates.
    /// In add mode (CURRENT_ID == 0): Initializes a new courier object with default values.
    /// </remarks>
    private void CourierWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.ResetRequested += () => Close();

        if (CURRENT_ID != 0) 
        {
            ButtonText = "Update";
            try
            {
                CourierObserver();
                s_bl.Courier.AddObserver(CURRENT_ID, CourierObserver);
                s_bl.Admin.AddClockObserver(ClockObserver);
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

            CurrentCourier = new BO.Courier
            {
                Id = 0,
                Name = "",
                Phone = "",
                Email = "",
                Password = "",
                Active = true,
                MaxDistanceDelivery = 0,
                TypeShipment = BO.TheTypeShipment.CAR,
                WorkingSince = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock()),
                DeliveryLate = 0,
                DeliveryOnTime = 0
            };
        }
    }

    /// <summary>
    /// Handles the Closed event of the CourierWindow.
    /// Unregisters observers and cleans up resources.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CourierWindow_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => Close();
        if (CURRENT_ID != 0)
        {
            s_bl.Courier.RemoveObserver(CURRENT_ID, CourierObserver);
            s_bl.Admin.RemoveClockObserver(ClockObserver);
        }
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Handles the Add/Update button click event.
    /// Validates and saves the courier data to the business logic layer.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Performs validation to ensure the courier name is not empty.
    /// Creates a new courier if CURRENT_ID is 0, otherwise updates the existing courier.
    /// Closes the window upon successful operation.
    /// </remarks>
    /// <exception cref="BO.BlInvalidValueException">Thrown when courier data is invalid.</exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown when attempting to create a courier with an existing ID.</exception>
    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier == null || string.IsNullOrEmpty(CurrentCourier.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }

        try
        {
            if (CURRENT_ID == 0) 
            {
                s_bl.Courier.Create(CURRENT_MANAGER_ID, CurrentCourier);
                MessageBox.Show("Courier added successfully!");
            }
            else
            {
                s_bl.Courier.Update(CURRENT_MANAGER_ID, CurrentCourier);
                MessageBox.Show("Details updated successfully!");
            }

            Close();
        }
        catch (BO.BlInvalidValueException ex)
        {
            MessageBox.Show($"Invalid data: {ex.Message}");
        }
        catch (BO.BlAlreadyExistsException ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"General error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Delete button click event.
    /// Prompts for confirmation and deletes the current courier.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Displays a confirmation dialog before proceeding with the deletion.
    /// Only available in update mode when a courier already exists.
    /// Closes the window upon successful deletion.
    /// </remarks>
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה למחוק את השליח הזה?",
            "אישור מחיקה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            s_bl.Courier.Delete(CURRENT_MANAGER_ID, CURRENT_ID);
            MessageBox.Show("השליח נמחק בהצלחה");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה במחיקה: {ex.Message}");
        }
    }

    #endregion

    #region Logic & Helpers
    
    /// <summary>
    /// Handles the PasswordChanged event of the PasswordBox control.
    /// Updates the courier's password when the user types in the password field.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a <see cref="PasswordBox"/>.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// The password is not bound directly due to security considerations in WPF.
    /// This handler manually updates the courier's password property.
    /// </remarks>
    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && CurrentCourier != null)
        {
            CurrentCourier.Password = passwordBox.Password;
        }
    }

    /// <summary>
    /// Validates that only numeric input is allowed in a TextBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Used for TextBoxes that should only accept numeric values (e.g., phone number, ID).
    /// Sets e.Handled to true if the input is not a digit, preventing the character from being entered.
    /// </remarks>
    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = !e.Text.All(char.IsDigit);

    /// <summary>
    /// Asynchronously retrieves and updates the courier data from the business logic layer.
    /// Implements the Observer pattern to receive real-time updates.
    /// </summary>
    /// <remarks>
    /// This method runs on a background thread to avoid blocking the UI.
    /// Uses the ObserverMutex to prevent concurrent updates and race conditions.
    /// Automatically updates the UI through the Dispatcher when new data arrives.
    /// If the courier no longer exists in the system, the window is automatically closed.
    /// Subscribes to receive updates whenever the courier data changes in the BL.
    /// </remarks>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    private void CourierObserver()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Task.Run(async () =>
        {
            try
            {
                BO.Courier? freshCourier = null;

                await foreach (var courier in s_bl.Courier.Read(CURRENT_MANAGER_ID, CURRENT_ID))
                {
                    freshCourier = null;

                    freshCourier = courier
                      ?? throw new BO.BlDoesNotExistException($"The Courier with id: {CURRENT_ID} does not exist");

                    if (CurrentCourier is null)
                        await Dispatcher.BeginInvoke(() => 
                        {
                            CurrentCourier = courier;
                            DisplayTimeRemaining = courier.OrderInProgress?.TimeRemaining;
                        });
                }

                if (freshCourier == null)
                    throw new BO.BlDoesNotExistException($"The Courier with id: {CURRENT_ID} does not exist");

                await Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentCourier != freshCourier)
                    {
                        CurrentCourier = freshCourier;
                        DisplayTimeRemaining = freshCourier.OrderInProgress?.TimeRemaining;
                    }
                    else
                    {
                        OnPropertyChanged(nameof(CurrentCourier));
                        DisplayTimeRemaining = freshCourier.OrderInProgress?.TimeRemaining;
                    }
                });
            }
            catch (BO.BlDoesNotExistException)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    Close();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
            }
            finally
            {
                if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
                    CourierObserver();
            }
        });
    }

    /// <summary>
    /// Observes clock changes and updates the remaining delivery time accordingly.
    /// </summary>
    /// <remarks>
    /// This method is called automatically when the system clock advances.
    /// Calculates the time difference and decrements the DisplayTimeRemaining property.
    /// If the time reaches zero or below, triggers a courier data refresh to update the delivery status.
    /// Runs asynchronously to avoid blocking the UI thread.
    /// Uses the ObserverMutex to prevent race conditions when multiple clock updates occur simultaneously.
    /// </remarks>
    private async void ClockObserver()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        try
        {
            var newDate = s_bl.Admin.GetClock();
            var buffer = newDate - CURRENT_DATE;

            await Dispatcher.BeginInvoke(() =>
            {
                if (DisplayTimeRemaining.HasValue)
                {
                    if (DisplayTimeRemaining > TimeSpan.Zero && (DisplayTimeRemaining - buffer) <= TimeSpan.Zero)
                        CourierObserver();
                    
                    DisplayTimeRemaining -= buffer;
                    CURRENT_DATE = newDate;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clock Error: {ex.Message}");
        }
        finally
        {
            if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
                ClockObserver();
        }
    }

    #endregion
}

