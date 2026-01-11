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

namespace PL;

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
    private readonly int CURRENT_MANAGER_ID = 
            Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().ManagerId);

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
                OrderDate = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock()),
                TimeLeftForDelivery = Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().MaxDeliveryTime),
                Weight = 0,
                // Distance = 0,
                TypeOfOrder = BO.TypeOfOrder.STANDART,
                ScheduleStatus = BO.ScheduleStatus.ONTYME,
                // EstimatedDeliveryTime = null,
                MaxDeliveryTime = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock() + s_bl.Admin.GetConfig().MaxDeliveryTime)

            };

        }


    }
    /// <summary>
    /// btnCancel_Click - Handles the click event for the Cancel button to cancel the current order.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnCancel_Click(object sender, RoutedEventArgs e)
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
            s_bl.Order.Cancel(CURRENT_MANAGER_ID, CurrentID);
           
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
        catch (SmtpException ex)
        {
            MessageBox.Show($"הזמנה מס' {CurrentID} בוטלה בהצלחה ({ex.Message})");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בביטול: {ex.Message}");
        }
       
    }
    /// <summary>
    /// orderObserver - Observes changes to the current order and updates the UI accordingly.
    /// </summary>
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
    /// <summary>
    /// cleans up observers when the window is closed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OrderWindow_Closed(object sender, EventArgs e)
    {
        if (CurrentID != 0)
            Tools.RunSafe(() => s_bl.Order.RemoveObserver(CurrentID, OrderObserver));
    }

    /// <summary>
    /// btnAddUpdate_Click - Handles the click event for the Add/Update button to add or update an order.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
            MessageBox.Show($"ערך חסר או לא חוקי: {ex.Message}");
        }
        catch (BO.BlAlreadyExistsException ex)
        {
            MessageBox.Show($"שגיאה מסוג: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה כללית: {ex.Message}");
        }
    }

}

