using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// Interaction logic for CourierDeliveryHistoryWindow.xaml
/// </summary>
public partial class CourierDeliveryHistoryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


    private readonly int userId;

    private readonly int MANAGER_ID =
        Tools.GetSafeFromBl<int>(() => s_bl.Admin.GetConfig().ManagerId);


    public CourierDeliveryHistoryWindow(int couriorId)
    {
        userId = couriorId;
        InitializeComponent();
      

    }

    public ObservableCollection<BO.ClosedDeliveryInList> DeliveriesHistory
    {
        get=> (ObservableCollection<BO.ClosedDeliveryInList>)GetValue(DeliveriesHistoryProperty);
        set => SetValue(DeliveriesHistoryProperty, value);
    }
    public static readonly DependencyProperty DeliveriesHistoryProperty =
        DependencyProperty.Register("DeliveriesHistory", typeof(ObservableCollection<BO.ClosedDeliveryInList>),
            typeof(CourierDeliveryHistoryWindow), new PropertyMetadata(null));



    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            OrderObserver();
            s_bl.Order.AddObserver(OrderObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
            Close();
        }
    }


    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl.Order.RemoveObserver(OrderObserver);
        }
        catch (Exception)
        {
            Close();
        }
       
    }

   
    private void OrderObserver()
    {
        try
        {
           
            var tempList = s_bl.Order.GetClosed(MANAGER_ID, userId, null, null)
                           ?? throw new BO.BlDoesNotExistException($"The list for id: {userId} does not exist");

           
            Dispatcher.Invoke(() =>
            {
                DeliveriesHistory = new ObservableCollection<BO.ClosedDeliveryInList>(tempList);
            });
        }
        catch (BO.BlDoesNotExistException)
        {
            Dispatcher.Invoke(Close);
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
        }
    }

}
