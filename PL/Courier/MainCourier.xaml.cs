using PL.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PL;

/// <summary>
/// Main window for courier operations in the presentation layer.
/// Provides courier profile management, delivery operations, and real-time updates.
/// </summary>
/// <remarks>
/// This window implements INotifyPropertyChanged for data binding and uses the Observer pattern
/// to receive real-time updates from the business logic layer about courier and delivery status changes.
/// </remarks>
public partial class MainCourier : Window, INotifyPropertyChanged
{
    #region Services & Constants
    
    /// <summary>
    /// Business logic layer interface instance for accessing courier and delivery services.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    
    /// <summary>
    /// Current date and time from the system clock.
    /// </summary>
    private static DateTime CURRENT_DATE = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock());
    
    /// <summary>
    /// The unique identifier of the logged-in courier user.
    /// </summary>
    private readonly int USERID;
    
    /// <summary>
    /// Mutex for managing concurrent observer callbacks and preventing race conditions.
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
    /// Backing field for the IsEditMode property.
    /// </summary>
    private bool _IsEditMode = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether the window is in edit mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if the courier details can be edited; otherwise, <c>false</c>.
    /// </value>
    public bool IsEditMode
    {
        get => _IsEditMode;
        set
        {
            if (_IsEditMode != value)
            {
                _IsEditMode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Backing field for the IsOrderInProgress property.
    /// </summary>
    private bool? _IsOrderInProgress = null;
    
    /// <summary>
    /// Gets or sets a value indicating whether the courier has an active order in progress.
    /// </summary>
    /// <value>
    /// <c>true</c> if an order is currently being delivered; otherwise, <c>false</c>.
    /// </value>
    public bool? IsOrderInProgress
    {
        get => _IsOrderInProgress;
        set
        {
            if (_IsOrderInProgress != value)
            {
                _IsOrderInProgress = value;
                OnPropertyChanged();
            }
        }
    }

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
    /// Initializes a new instance of the <see cref="MainCourier"/> class.
    /// </summary>
    /// <param name="userId">The unique identifier of the courier user.</param>
    public MainCourier(int userId)
    {
        USERID = userId;

        InitializeComponent();

        DataContext = this;
    }

    /// <summary>
    /// Handles the Loaded event of the MainCourier window.
    /// Initializes the window state, loads courier data, and registers observers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void MainCourier_Loaded(object sender, RoutedEventArgs e)
    {
       
        
        GetCurier();
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Courier.AddObserver(USERID, GetCurier));
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));
    }

    /// <summary>
    /// Handles the Closed event of the MainCourier window.
    /// Unregisters observers and cleans up resources.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void MainCourier_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        if (USERID != 0)
        {
            Tools.RunSafe(() => s_bl.Courier.RemoveObserver(USERID, GetCurier));
            Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
        }
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Handles the ReportDelivery button click event.
    /// Opens a dialog to complete the current delivery with a status reason.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Validates that a delivery is in progress before allowing completion.
    /// Opens a modal dialog for the courier to select the delivery completion status.
    /// </remarks>
    private void ReportDelivery_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier?.OrderInProgress == null)
        {
            MessageBox.Show("אין משלוח פעיל לסיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectionWindow = new CloseDeliveryWindow();
        selectionWindow.Owner = Window.GetWindow(this);

        bool? result = selectionWindow.ShowDialog();

        if (result == true && selectionWindow.IsConfirmed)
        {
            var selectedReason = selectionWindow.SelectedEndDelivery;

            try
            {
                if (selectedReason is BO.EndDelivery status)
                {
                    s_bl.Delivery.DeliverEnd(USERID, USERID, CurrentCourier.OrderInProgress.DeliveryId, status);

                    MessageBox.Show("המשלוח הסתיים בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    throw new BO.BlInvalidOperationException("סיבת הסיום שנבחרה אינה תקפה.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בסיום המשלוח: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Handles the StartDelivery button click event.
    /// Opens a window to select and start a new delivery based on the courier's vehicle type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Validates that no delivery is currently in progress.
    /// Determines allowed order types based on the courier's vehicle type:
    /// - BIKE/FOOT: Standard orders only
    /// - CAR: Standard and Fast Delivery orders
    /// - MOTORCYCLE: All order types including immediate delivery
    /// </remarks>
    private void StartDelivery_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier?.OrderInProgress != null)
        {
            MessageBox.Show("יש משלוח פעיל לסיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IEnumerable<BO.TypeOfOrder> allowedTypes = CurrentCourier!.TypeShipment switch
        {
            BO.TheTypeShipment.BIKE or BO.TheTypeShipment.FOOT =>
                new[] { BO.TypeOfOrder.STANDART },

            BO.TheTypeShipment.CAR =>
                new[] { BO.TypeOfOrder.STANDART, BO.TypeOfOrder.FAST_DELIVERY },

            BO.TheTypeShipment.MOTORCYCLE =>
                new[] { BO.TypeOfOrder.STANDART, BO.TypeOfOrder.FAST_DELIVERY, BO.TypeOfOrder.DELIVER_IMMEDIATELY },

            _ => Array.Empty<BO.TypeOfOrder>()
        };

        var enumTypeOfOrder = Tools.GetEnumList(allowedTypes);

        Tools.OpenOrActivateWindow<StartDeliveryWindow>(window => window.UserId == USERID, this, USERID, USERID, CurrentCourier!.TypeShipment, enumTypeOfOrder);
    }

    /// <summary>
    /// Handles the CourierDeliveryHistory button click event.
    /// Opens a window displaying the courier's complete delivery history.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void CureierDeliveryHistory_Click(object sender, RoutedEventArgs e)
         => Tools.OpenOrActivateWindow<CourierDeliveryHistoryWindow>(window => window.UserId == USERID, this, USERID);

    /// <summary>
    /// Handles the EditCourier button click event.
    /// Enables edit mode for modifying courier profile details.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void EditCureier_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = true;
    }

    /// <summary>
    /// Handles the CloseCourierEdit button click event.
    /// Exits edit mode and reloads the original courier data, discarding any unsaved changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void CloseCureierEdit_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = false;
        GetCurier();
    }

    /// <summary>
    /// Handles the SaveUpdate button click event.
    /// Validates and saves the modified courier data to the business logic layer.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    /// <remarks>
    /// Performs validation to ensure required fields are filled.
    /// Updates the courier information through the business logic layer.
    /// Exits edit mode upon successful save.
    /// </remarks>
    /// <exception cref="BO.BlInvalidValueException">Thrown when courier data is invalid.</exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown when a conflict occurs with existing data.</exception>
    private void btnSaveUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier == null || string.IsNullOrEmpty(CurrentCourier.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }
        try
        {
            s_bl.Courier.Update(USERID, CurrentCourier);
            MessageBox.Show("הפרטים נשמרו בהצלחה!");
            IsEditMode = false;
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

    #endregion

    #region Logic & Helpers
    
    /// <summary>
    /// Asynchronously retrieves and updates the courier data from the business logic layer.
    /// Implements the Observer pattern to receive real-time updates.
    /// </summary>
    /// <remarks>
    /// This method runs on a background thread to avoid blocking the UI.
    /// Uses the ObserverMutex to prevent concurrent updates and race conditions.
    /// Automatically updates the UI through the Dispatcher when new data arrives.
    /// Handles cases where the courier no longer exists by closing the window.
    /// </remarks>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    private void GetCurier()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        Task.Run(async () =>
        {
            try
            {
                BO.Courier? freshCourier = null;

                await foreach (var courier in s_bl.Courier.Read(USERID, USERID))
                {
                    freshCourier = null;

                    freshCourier = courier
                      ?? throw new BO.BlDoesNotExistException($"The Courier with id: {USERID} does not exist");
                   
                   
                    if (CurrentCourier is null)//עדכון ראשון ויחיד כאן אחרי שחזרנו מהיילד, אין לנו ערך במשלוח כלל
                        await Dispatcher.BeginInvoke(() =>
                        {
                            CurrentCourier = courier;
                           
                        });
                }

                if (freshCourier == null)
                    throw new BO.BlDoesNotExistException($"The Courier with id: {USERID} does not exist");

                await Dispatcher.BeginInvoke(() =>//עדכון שני וסופי כולל השדה משלוח פעיל
                {
                    //########################################################
                    IsOrderInProgress = freshCourier.OrderInProgress != null;//עכשיו אנחנו מאפשרים את הכפתור יציאה למשלוח אם אין משלוח פעיל
                    //########################################################

                    if (CurrentCourier != freshCourier)
                    {
                        CurrentCourier = freshCourier;//עבור מקרה בו יש קיראה חזורת ולכן הפרש קוריור כולו שונה
                    }
                    else
                    {
                        OnPropertyChanged(nameof(CurrentCourier));//מאלץ עדכון, שהרי הרפרנס לא שונה רק השדה משלוח פעיל
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
                    GetCurier();
            }
        });
    }

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
