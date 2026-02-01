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
    /// Current date and time from the system clock simulation.
    /// </summary>
    private static DateTime CURRENT_DATE = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock());

    /// <summary>
    /// The unique identifier of the courier being managed.
    /// Zero indicates 'Add' mode; non-zero indicates 'Update' mode.
    /// </summary>
    private int CURRENT_ID = 0;

    /// <summary>
    /// Mutex for managing concurrent observer callbacks and preventing race conditions during data loading.
    /// </summary>
    private readonly ObserverMutex _Mutex = new();

    /// <summary>
    /// Mutex for managing concurrent clock updates and preventing race conditions.
    /// </summary>
    private readonly ObserverMutex _ClockMutex = new();

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

    private bool _IsCanDelete = true;

    /// <summary>
    /// Gets or sets a value indicating whether the courier record can be deleted.
    /// </summary>
    /// <remarks>
    /// This property is updated asynchronously. It is set to <c>false</c> if the courier
    /// has an existing delivery history, preventing deletion to maintain data integrity.
    /// </remarks>
    public bool IsCanDelete
    {
        get => _IsCanDelete;
        set
        {
            if (_IsCanDelete != value)
            {
                _IsCanDelete = value;
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
    /// Gets the list of available vehicle types for courier selection (e.g., Car, Motorcycle).
    /// </summary>
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
                CourierObserver(); // Initial fetch
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
    /// Unregisters observers and cleans up resources to prevent memory leaks.
    /// </summary>
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
    /// Validates input and saves the courier data to the business logic layer.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
    /// <remarks>
    /// Requires confirmation via MessageBox.
    /// Only allows deletion if <see cref="IsCanDelete"/> is true.
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
    /// Manually updates the courier's password property since PasswordBox does not support direct binding.
    /// </summary>
    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && CurrentCourier != null)
        {
            CurrentCourier.Password = passwordBox.Password;
        }
    }

    /// <summary>
    /// Validates that only numeric input is allowed in specific TextBoxes (e.g. ID, Phone).
    /// </summary>
    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = !e.Text.All(char.IsDigit);

    /// <summary>
    /// Asynchronously retrieves and updates the courier data from the business logic layer.
    /// Also checks if the courier can be deleted based on delivery history.
    /// </summary>
    /// <remarks>
    /// Runs on a background thread. Uses <see cref="ObserverMutex"/> to prevent race conditions.
    /// Updates the UI via Dispatcher.
    /// </remarks>
    private void CourierObserver()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                BO.Courier? freshCourier = null;

                // Check delete permission (heavy operation)
                if (IsCanDelete)
                {
                    bool canDelete = true;
                    await Task.Run(() => canDelete = !s_bl.Delivery.ReadAll(d => d.CourierId == CURRENT_ID).Any());
                    await Dispatcher.BeginInvoke(() => IsCanDelete = canDelete);
                }

                // Fetch courier data stream
                await foreach (var courier in s_bl.Courier.Read(CURRENT_MANAGER_ID, CURRENT_ID))
                {
                    freshCourier = null;

                    freshCourier = courier
                      ?? throw new BO.BlDoesNotExistException($"The Courier with id: {CURRENT_ID} does not exist");

                    if (CurrentCourier is null)
                        await Dispatcher.BeginInvoke(() =>
                        {
                            CurrentCourier = courier;
                        });
                }

                await Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentCourier != freshCourier)
                    {
                        CurrentCourier = freshCourier;
                    }
                    else
                    {
                        OnPropertyChanged(nameof(CurrentCourier));
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
    private async void ClockObserver()
    {
        if (_ClockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        try
        {
            var newDate = s_bl.Admin.GetClock();
            var buffer = newDate - CURRENT_DATE;

            await Dispatcher.BeginInvoke(() =>
            {
                if (CurrentCourier?.OrderInProgress is not null)
                    CurrentCourier.OrderInProgress.TimeRemaining -= buffer;

                CURRENT_DATE = newDate;
            });
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

    #endregion
}