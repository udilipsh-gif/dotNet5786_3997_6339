using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// Interaction logic for MainCureier.xaml
/// </summary>
public partial class MainCureier : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int USERID;

    public MainCureier(int userId)
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
        typeof(MainCureier), new PropertyMetadata(false));

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
            typeof(MainCureier), new PropertyMetadata(null));

    public bool IsOrderInProgress
    {
        get => (bool)GetValue(IsOrderInProgressProperty);
        set => SetValue(IsOrderInProgressProperty, value);
    }

    public static readonly DependencyProperty IsOrderInProgressProperty =
        DependencyProperty.Register("IsOrderInProgress", typeof(bool),
            typeof(MainCureier), new PropertyMetadata(false));

    private void MainCureier_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.RunSafe(() => s_bl.Courier.AddObserver(USERID, GetCurier));
        IsOrderInProgress = false;
        GetCurier();
    }

    private void MainCureier_Closed(object sender, EventArgs e)
    {
        if (USERID != 0)
            Tools.RunSafe(() => s_bl.Courier.RemoveObserver(GetCurier));
    }

    private void GetCurier()
    {
        try
        {
            CurrentUser = s_bl.Courier.Read(USERID, USERID)
                ?? throw new BO.BlDoesNotExistException();
            if (CurrentUser.OrderInProgress is not null)
            {
                IsOrderInProgress = true;
            }
        }
        catch (BO.BlDoesNotExistException)
        {
            MessageBox.Show("שליח לא קיים", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
        

        // הצגת החלון כ-Modal Dialog
        bool? result = selectionWindow.ShowDialog();

        if (result == true && selectionWindow.IsConfirmed)
        {
            var selectedReason = selectionWindow.SelectedEndDelivery;

            try
            {
                if (selectedReason is BO.EndDelivery status)
                {
                    s_bl.Order.Deliver(USERID, USERID, CurrentUser.OrderInProgress.DeliveryId, status);

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
        if(CurrentUser?.OrderInProgress != null)
        {
            MessageBox.Show("יש משלוח פעיל לסיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Tools.OpenOrActivateWindow<StartDeliveryWindow>(USERID, USERID);
    }

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
