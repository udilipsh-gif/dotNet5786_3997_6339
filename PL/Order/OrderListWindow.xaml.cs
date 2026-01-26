using BO;
using PL.Helpers;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PL;

public partial class OrderListWindow : Window, IWindowUpdater
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private int CURRENT_MANAGER_ID = Tools.GetSafeFromBl<int>(() => s_bl.Admin.GetConfig().ManagerId);

    private DateTime CURRENT_DATE;

    public OrderListWindow(BO.OrderStatus? orderStatus, BO.ScheduleStatus? scheduleStatus, BO.TypeOfOrder? typeOfOrder)
    {
        SelectedScheduleFilter = scheduleStatus;
        SelectedTypeFilter = typeOfOrder;
        SelectedOrderStatusFilter = orderStatus;

        CURRENT_DATE = Tools.GetSafeFromBl<DateTime>(() => s_bl.Admin.GetClock());

        InitializeComponent();
    }

    public OrderListWindow() : this(null, null, null) { }

    public ObservableCollection<BO.OrderInList> OrderList
    {
        get { return (ObservableCollection<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    public static readonly DependencyProperty OrderListProperty =
    DependencyProperty.Register(nameof(OrderList), typeof(ObservableCollection<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    public IEnumerable<Tools.SelectionItem> ScheduleStatusList
    {
        get => Tools.GetEnumList<BO.ScheduleStatus>("הכל");
    }

    public IEnumerable<Tools.SelectionItem> TypeOfOrderList
    {
        get => Tools.GetEnumList<BO.TypeOfOrder>("הכל");
    }

    public IEnumerable<Tools.SelectionItem> TypeOfOrderStatusList
    {
        get => Tools.GetEnumList<BO.OrderStatus>("הכל");
    }


    
    public BO.ScheduleStatus? SelectedScheduleFilter
    {
        get => (BO.ScheduleStatus?)GetValue(SelectedScheduleFilterProperty);
        set => SetValue(SelectedScheduleFilterProperty, value);

    }

    public static readonly DependencyProperty SelectedScheduleFilterProperty =
        DependencyProperty.Register(nameof(SelectedScheduleFilter), typeof(BO.ScheduleStatus?),
            typeof(OrderListWindow), new PropertyMetadata(null));

    /// <summary>
    /// סינון לפי סוג הזמנה
    /// </summary>
    public BO.TypeOfOrder? SelectedTypeFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelectedTypeFilterProperty);
        set => SetValue(SelectedTypeFilterProperty, value);
    }

    public static readonly DependencyProperty SelectedTypeFilterProperty =
        DependencyProperty.Register(nameof(SelectedTypeFilter), typeof(BO.TypeOfOrder?), typeof(OrderListWindow), new PropertyMetadata(null));

    /// <summary>
    /// סינון לפי סטטוס הזמנה
    /// </summary>
    public BO.OrderStatus? SelectedOrderStatusFilter
    {
        get => (BO.OrderStatus?)GetValue(SelectedOrderStatusFilterProperty);
        set => SetValue(SelectedOrderStatusFilterProperty, value);
    }

    public static readonly DependencyProperty SelectedOrderStatusFilterProperty =
        DependencyProperty.Register(nameof(SelectedOrderStatusFilter), typeof(BO.OrderStatus?),
            typeof(OrderListWindow), new PropertyMetadata(null));




    private readonly ObserverMutex _ListMutex = new(); //stage 7
    private readonly ObserverMutex _ClockMutex = new(); //stage 7

    private async void LoadOrders()
    {
        if (_ListMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        BO.ScheduleStatus? scheduleFilter = null;
        BO.TypeOfOrder? typeFilter = null;
        BO.OrderStatus? statusFilter = null;

        Dispatcher.Invoke(() =>
        {
            scheduleFilter = SelectedScheduleFilter;
            typeFilter = SelectedTypeFilter;
            statusFilter = SelectedOrderStatusFilter;
        });

        try
            {
                Func<BO.OrderInList, bool> filterPredicate = order =>
                    (scheduleFilter == null || order.ScheduleStatus == scheduleFilter) &&
                    (typeFilter == null || order.TypeOfOrder == typeFilter) &&
                    (statusFilter == null || order.OrderStatus == statusFilter);

                var filteredResults = await s_bl.Order.ReadAll(CURRENT_MANAGER_ID, filterPredicate, BO.OrderInListField.OrderId);

                Dispatcher.Invoke(() =>
                {
                    if (OrderList == null)
                        OrderList = new ObservableCollection<BO.OrderInList>(filteredResults);
                    else
                    {
                        OrderList.Clear();
                        foreach (var item in filteredResults)
                            OrderList.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error loading orders: {ex.Message}", "שגיאה ברשימת ההזמנות"));
            }
            finally
            {
                if (await _ListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    LoadOrders();
            }
       
    }

    private async void ClockObserver()
    {
        if (_ClockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;

        try
        {
            var newDate = s_bl.Admin.GetClock();
            var buffer = newDate - CURRENT_DATE;

            // ✅ גישה ל-OrderList על ה-UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                if (OrderList != null)
                {
                    foreach (var order in OrderList)
                    {
                        order.TimeLeftForDelivery -= buffer;
                    }
                }
            });

            CURRENT_DATE = newDate;
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

    public void UpdateState(params object?[] args)
    {
        if (args != null && args.Length >= 3)
        {
            SelectedOrderStatusFilter = args[0] as BO.OrderStatus?;
            SelectedScheduleFilter = args[1] as BO.ScheduleStatus?;
            SelectedTypeFilter = args[2] as BO.TypeOfOrder?;

            LoadOrders();
        }
    }

    private void orderListObserver()
        => LoadOrders();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Order.AddObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Admin.AddClockObserver(ClockObserver));

        orderListObserver();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(orderListObserver));
        Tools.RunSafe(() => s_bl.Admin.RemoveClockObserver(ClockObserver));
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => orderListObserver();

    private void Add_Edit_Order_Click(object sender, RoutedEventArgs e)
    {
        OrderWindow orderWindow;
        if (sender is DataGridRow row
            && row.Item is BO.OrderInList orderInList)
        {
            orderWindow = new OrderWindow(orderInList.OrderId);
        }
        else
        {
            orderWindow = new OrderWindow(0);
        }
        Tools.SetSoftOwner(orderWindow, this);
        orderWindow.Show();
    }





    private async void btnCancelOrder_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה לבטל את ההזמנה הזו?",
            "הזמנה בוטלה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;
        if (sender is Button button
            && button.DataContext is BO.OrderInList orderInList)
        {
            bool StateToken = (button.CommandParameter as bool?).GetValueOrDefault();
            try
            {
                await s_bl.Order.Cancel(CURRENT_MANAGER_ID, orderInList.OrderId, StateToken);
                MessageBox.Show($"הזמנה מס' {orderInList.OrderId} בוטלה בהצלחה");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (BO.BlInvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }

            catch (BLNoSendSmsException ex)
            {
                MessageBox.Show($"הזמנה מס' {orderInList.OrderId} בוטלה בהצלחה ({ex.Message})");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error canceling the order: {ex.Message}");
            }
        }
    }

}
