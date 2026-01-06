using PL.Courier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
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

namespace PL.Order;

/// <summary>
/// Interaction logic for _.xaml
/// </summary>
public partial class OrderWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }




    /// <summary>
    /// Business logic layer instance for accessing order operations.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


    private int CurrentID = 0;

    /// <summary>
    /// The ID of the currently logged-in manager.
    /// </summary>
    private readonly int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId!;

    public bool IsUpdateMode => ButtonText == "Update";

    /// <summary>
    /// Gets or sets the text displayed on the button.
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }
    /// <summary>
    /// Identifies the <see cref="ButtonText"/> dependency property.
    /// </summary>
    /// <remarks>This property represents the text displayed on the button within the <see
    /// cref="OrderWindow"/>. The default value is "Add".</remarks>
    public static readonly DependencyProperty ButtonTextProperty =
      DependencyProperty.Register(nameof(ButtonText), typeof(string),
          typeof(OrderWindow), new PropertyMetadata("Add"));



    //private Visibility _cancelButtonVisibility = Visibility.Collapsed;


    //public Visibility CancelButtonVisibility
    //{
    //    get => _cancelButtonVisibility;
    //    set
    //    {
    //        _cancelButtonVisibility = value;
    //        OnPropertyChanged(nameof(CancelButtonVisibility));
    //    }
    //}





    //שיניתי את הערכים לצפייה בלבד אז אין צורך למשוך אינום
    //public IEnumerable<BO.ScheduleStatus> ScheduleStatusValues
    //{
    //    get
    //    {
    //        return Enum.GetValues(typeof(BO.ScheduleStatus)).Cast<BO.ScheduleStatus>();
    //    }
    //}
    //public IEnumerable<BO.OrderStatus> OrderStatusValues
    //{
    //    get
    //    {
    //        return Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>();
    //    }
    //}
    public OrderWindow(int id = 0)
    {
        InitializeComponent();
        CurrentID = id;
    }

    public BO.Order CurrentOrder
    {
        get => (BO.Order)GetValue(CurrentOrderProperty);
        set => SetValue(CurrentOrderProperty, value);
    }
    public static readonly DependencyProperty CurrentOrderProperty =
         DependencyProperty.Register("CurrentOrder", typeof(BO.Order),
             typeof(OrderWindow), new PropertyMetadata(null));

    //public BO.DeliveryPerOrderInList deliveryPerOrderInList
    //{
    //    get => (BO.DeliveryPerOrderInList)GetValue(deliveryPerOrderInListProperty);
    //    set => SetValue(deliveryPerOrderInListProperty, value);
    //}
    //public static readonly DependencyProperty deliveryPerOrderInListProperty =
    //     DependencyProperty.Register("deliveryPerOrderInList", typeof(BO.DeliveryPerOrderInList),
    //         typeof(OrderWindow), new PropertyMetadata(new BO.DeliveryPerOrderInList()
    //         {

    //              DeliveryId = 0,
    //                CourierId = 0,
    //               CourierName = "",
    //               TypeShipment = BO.TheTypeShipment.CAR,
    //               OrderDate = DateTime.Now,
    //         }));


    private void OrderWindow_Loaded(object sender, EventArgs e)
    {
        if (CurrentID != 0)
        {
            ButtonText = "Update";
            //CancelButtonVisibility = Visibility.Visible;

            try
            {
                OrderObserver();
                s_bl.Order.AddObserver(CurrentID, OrderObserver);
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
            //CancelButtonVisibility = Visibility.Collapsed;

            CurrentOrder = new BO.Order()
            {
                Id = 0,
                Name = "",
                Phone = "",
                Addres = "",
                // Details = "",
                OrderStatus = BO.OrderStatus.OPEN,
                OrderDate = s_bl.Admin.GetClock(),
                TimeLeftForDelivery = s_bl.Admin.GetConfig().MaxDeliveryTime,
                Weight = 0,
                // Distance = 0,
                TypeOfOrder = BO.TypeOfOrder.STANDART,
                ScheduleStatus = BO.ScheduleStatus.ONTYME,
                // EstimatedDeliveryTime = null,
                MaxDeliveryTime = s_bl.Admin.GetClock() + s_bl.Admin.GetConfig().MaxDeliveryTime

            };

        }


    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה לבטל את ההזמנה הזו?",
            "הזמנה בוטלה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            s_bl.Order.Cancel(CURRENT_MANAGER_ID, CurrentID);
           
            MessageBox.Show("הזמנה בוטלה בהצלחה");

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
        catch (SmtpException ex)
        {
            MessageBox.Show($" הזמנה בוטלה בהצלחה ({ex.Message})");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בביטול: {ex.Message}");
        }
       
    }
    private void OrderObserver()
    {
        try
        {
            CurrentOrder = s_bl.Order.Read(CURRENT_MANAGER_ID, CurrentID)
                        ?? throw new BO.BlDoesNotExistException($"The Order with id: {CurrentID} does not exist");
        }
        catch (BO.BlDoesNotExistException)
        {
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    private void OrderWindow_Closed(object sender, EventArgs e)
    {
        if (CurrentID != 0)
            s_bl.Order.RemoveObserver(CurrentID, OrderObserver);
    }

    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentOrder == null || string.IsNullOrEmpty(CurrentOrder.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }

        try
        {
            if (sender is Button button && button.Content is "Add")
            {
                s_bl.Order.Create(CURRENT_MANAGER_ID, CurrentOrder);
                MessageBox.Show("Order added successfully!");
            }
            else
            {
                s_bl.Order.Update(CURRENT_MANAGER_ID, CurrentOrder);
                MessageBox.Show("Details updated successfully!");
            }

            this.Close();
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

