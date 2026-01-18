using System.Collections.ObjectModel;
using System.Windows;

namespace PL;

/// <summary>
/// Interaction logic for CourierDeliveryHistoryWindow.xaml
/// </summary>
public partial class CourierDeliveryHistoryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


    public int UserId { get; private init; }

    private readonly int MANAGER_ID =
        Tools.GetSafeFromBl<int>(() => s_bl.Admin.GetConfig().ManagerId);


    public CourierDeliveryHistoryWindow(int couriorId)
    {
        UserId = couriorId;
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


    private async void OrderObserver()
    {
        try
        {

            var newList = await s_bl.Order.GetClosed(MANAGER_ID, UserId, null, null)
                           ?? throw new BO.BlDoesNotExistException($"The list for id: {UserId} does not exist");
            if (DeliveriesHistory == null)
            {
                DeliveriesHistory = new ObservableCollection<BO.ClosedDeliveryInList>(newList);
            }
            else
            {
                DeliveriesHistory.Clear(); // מחיקת הישנים
                foreach (var item in newList)
                {
                    DeliveriesHistory.Add(item); // הוספת החדשים
                }
            }
        }
        catch (BO.BlDoesNotExistException ex)
        {
            MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
        }
    }

}
