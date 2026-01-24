using PL.Helpers;
using System.Threading.Tasks;
using System.Windows;

namespace PL;

/// <summary>
/// Interaction logic for MainCourier.xaml
/// </summary>
public partial class MainCourier : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int USERID;

    public MainCourier(int userId)
    {
        USERID = userId;

        InitializeComponent();
    }

    /// <summary>
    /// List of all vehicle/shipment types for the ComboBox.
    /// </summary>
    public IEnumerable<BO.TheTypeShipment> VehicleTypesList { get; } =
     Enum.GetValues(typeof(BO.TheTypeShipment)).Cast<BO.TheTypeShipment>();


    public bool IsEditMode
    {
        get => (bool)GetValue(IsEditModeProperty);
        set => SetValue(IsEditModeProperty, value);
    }

    public static readonly DependencyProperty IsEditModeProperty =
        DependencyProperty.Register("IsEditMode", typeof(bool),
        typeof(MainCourier), new PropertyMetadata(false));

    public BO.Courier? CurrentUser
    {
        get => (BO.Courier?)GetValue(CurrentUserProperty);
        set => SetValue(CurrentUserProperty, value);
    }

    /// <summary>
    /// Dependency property for the CurrentCourier object.
    /// </summary>
    public static readonly DependencyProperty CurrentUserProperty =
        DependencyProperty.Register("CurrentUser", typeof(BO.Courier),
            typeof(MainCourier), new PropertyMetadata(null));

    public bool IsOrderInProgress
    {
        get => (bool)GetValue(IsOrderInProgressProperty);
        set => SetValue(IsOrderInProgressProperty, value);
    }

    public static readonly DependencyProperty IsOrderInProgressProperty =
        DependencyProperty.Register("IsOrderInProgress", typeof(bool),
            typeof(MainCourier), new PropertyMetadata(false));

    private void MainCourier_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Courier.AddObserver(USERID, GetCurier));
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(GetCurier));


        IsOrderInProgress = false;
        GetCurier();
    }

    private void MainCourier_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        if (USERID != 0)
        {
            Tools.RunSafe(() => s_bl.Courier.RemoveObserver(USERID, GetCurier));
            Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(GetCurier));

        }
    }


    private readonly ObserverMutex _Mutex = new(); //stage 7
    private void GetCurier()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())//הדלקת פלאג בפונקציה שמציינת שהריצה בעיצומה ואם מישהו ביקש ריסטארט בזמן הזה
            return;
        //Task.Run(async () =>
        //{
        //    bool windowIsOpen = true;
        //    try
        //    {
        //        var currentUser = await s_bl.Courier.Read(USERID, USERID)
        //            ?? throw new BO.BlDoesNotExistException();
        //        if (currentUser.OrderInProgress is not null)
        //        {
        //            IsOrderInProgress = true;
        //        }


        //        _ = Dispatcher.BeginInvoke(() =>
        //        {
        //            CurrentUser = currentUser;

        //        });


        //        //else
        //        //{
        //        //    IsOrderInProgress = false;
        //        //}
        //    }
        //    catch (BO.BlDoesNotExistException)
        //    {
        //        windowIsOpen = false;
        //        MessageBox.Show("שליח לא קיים", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        //        Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        if (windowIsOpen is true && await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
        //            GetCurier();
        //    }
        //});


        Dispatcher.BeginInvoke(async () =>
        {
            bool windowIsOpen = true;
            try
            {
                CurrentUser = await s_bl.Courier.Read(USERID, USERID)
                    ?? throw new BO.BlDoesNotExistException();
                if (CurrentUser.OrderInProgress is not null)
                {
                    IsOrderInProgress = true;
                }



                //else
                //{
                //    IsOrderInProgress = false;
                //}
            }
            catch (BO.BlDoesNotExistException)
            {
                windowIsOpen = false;
                MessageBox.Show("שליח לא קיים", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (windowIsOpen is true && await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
                    GetCurier();
            }
        });
    }

    private void ReportDelivery_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentUser?.OrderInProgress == null)
        {
            MessageBox.Show("אין משלוח פעיל לסיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // פתיחת חלון בחירת סיבת סיום
        var selectionWindow = new CloseDeliveryWindow();
        selectionWindow.Owner = Window.GetWindow(this);

        // הצגת החלון כ-Modal Dialog
        bool? result = selectionWindow.ShowDialog();

        if (result == true && selectionWindow.IsConfirmed)
        {
            var selectedReason = selectionWindow.SelectedEndDelivery;

            try
            {
                if (selectedReason is BO.EndDelivery status)
                {
                    s_bl.Delivery.Deliver(USERID, USERID, CurrentUser.OrderInProgress.DeliveryId, status);

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

    private void StartDelivery_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentUser?.OrderInProgress != null)
        {
            MessageBox.Show("יש משלוח פעיל לסיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IEnumerable<BO.TypeOfOrder> allowedTypes = CurrentUser!.TypeShipment switch
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

        Tools.OpenOrActivateWindow<StartDeliveryWindow>(window => window.UserId == USERID, this, USERID, USERID, CurrentUser!.TypeShipment, enumTypeOfOrder);
    }

    private void CureierDeliveryHistory_Click(object sender, RoutedEventArgs e)
         => Tools.OpenOrActivateWindow<CourierDeliveryHistoryWindow>(window => window.UserId == USERID, this, USERID);

    private void EditCureier_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = true;
    }

    private void CloseCureierEdit_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = false;
        GetCurier();
    }

    /// <summary>
    /// Handles the add/update button click event, validates and saves courier data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Performs basic validation before submitting data to the business logic layer.
    /// Creates a new courier if in add mode, updates existing courier if in update mode.
    /// Closes the window upon successful operation.
    /// </remarks>
    private void btnSaveUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }
        try
        {
            s_bl.Courier.Update(USERID, CurrentUser);
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
}
