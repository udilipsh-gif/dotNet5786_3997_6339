using PL.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

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
        get => (ObservableCollection<BO.ClosedDeliveryInList>)GetValue(DeliveriesHistoryProperty);
        set => SetValue(DeliveriesHistoryProperty, value);
    }
    public static readonly DependencyProperty DeliveriesHistoryProperty =
        DependencyProperty.Register("DeliveriesHistory", typeof(ObservableCollection<BO.ClosedDeliveryInList>),
            typeof(CourierDeliveryHistoryWindow), new PropertyMetadata(null));



    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Tools.ResetRequested += () => this.Close();
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
            Tools.ResetRequested -= () => this.Close();
            s_bl.Order.RemoveObserver(OrderObserver);
        }
        catch (Exception)
        {
            Close();
        }

    }

    private readonly ObserverMutex _Mutex = new(); //stage 7
    private async void OrderObserver()
    {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        try
        {
            var newList = await s_bl.Delivery.GetClosed(MANAGER_ID, UserId, null, null)
                          ?? throw new BO.BlDoesNotExistException($"The list for id: {UserId} does not exist");

            // 2. עדכון ה-UI במקרה של הצלחה
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DeliveriesHistory == null)
                {
                    DeliveriesHistory = new ObservableCollection<BO.ClosedDeliveryInList>(newList);
                }
                else
                {
                    DeliveriesHistory.Clear();
                    foreach (var item in newList)
                    {
                        DeliveriesHistory.Add(item);
                    }
                }
            });
        }
        catch (Exception ex) 
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string msg = (ex is BO.BlDoesNotExistException) ?
                             $"שגיאה בטעינת הנתונים: {ex.Message}" :
                             $"שגיאה כללית: {ex.Message}";

                MessageBox.Show(msg, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())
            {
                OrderObserver();
            }
        }
    }
}
