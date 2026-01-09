using System;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace PL;

public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId;

    public OrderListWindow()
    {
        InitializeComponent();
    }

    public ObservableCollection<BO.OrderInList> OrderList
    {
        get { return (ObservableCollection<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    public IEnumerable<Tools.SelectionItem> ScheduleStatusList // שיניתי מ-Filter ל-List
    {
        get => Tools.GetEnumList<BO.ScheduleStatus>("הכל");
    }

    public IEnumerable<Tools.SelectionItem> TypeOfOrderList // שיניתי מ-Filter ל-List
    {
        get => Tools.GetEnumList<BO.TypeOfOrder>("הכל");
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(ObservableCollection<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));


    public BO.ScheduleStatus? SelectedScheduleFilter
    {
        get { return (BO.ScheduleStatus?)GetValue(SelectedScheduleFilterProperty); }
        set
        {
            SetValue(SelectedScheduleFilterProperty, value);
        }
    }

    public static readonly DependencyProperty SelectedScheduleFilterProperty =
        DependencyProperty.Register(nameof(SelectedScheduleFilter), typeof(BO.ScheduleStatus?), typeof(OrderListWindow), new PropertyMetadata(null));



    public BO.TypeOfOrder? SelectedTypeFilter
    {
        get { return (BO.TypeOfOrder?)GetValue(SelectedTypeFilterProperty); }
        set
        {
            SetValue(SelectedTypeFilterProperty, value);
            LoadOrders(); // ריענון אוטומטי
        }
    }

    public static readonly DependencyProperty SelectedTypeFilterProperty =
        DependencyProperty.Register(nameof(SelectedTypeFilter), typeof(BO.TypeOfOrder?), typeof(OrderListWindow), new PropertyMetadata(null));

    private void LoadOrders()
    {
        // 1. שליפת כל הנתונים מה-BL ללא סינון ראשוני כלל
        var allOrders = s_bl.Order.ReadAll(CURRENT_MANAGER_ID, null, null, BO.OrderInListField.OrderId);

        // 2. ביצוע סינון כפול בעזרת LINQ
        var filteredResults = allOrders.Where(order =>
        (SelectedScheduleFilter == null || order.ScheduleStatus == SelectedScheduleFilter) &&
        (SelectedTypeFilter == null || order.TypeOfOrder == SelectedTypeFilter)
    );

        if (OrderList == null)
        {
            OrderList = new ObservableCollection<BO.OrderInList>(filteredResults);
        } 
        else
        {
            OrderList.Clear(); // מחיקת הישנים
            foreach (var item in filteredResults)
            {
                OrderList.Add(item); // הוספת החדשים
            }
        }
           
    }

    private void orderListObserver()
        => LoadOrders();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.AddObserver(orderListObserver);
        orderListObserver();
    }

    private void Window_Closed(object sender, EventArgs e)
        => s_bl.Order.RemoveObserver(orderListObserver);

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => orderListObserver();

    private void Add_Edit_Order_Click(object sender, RoutedEventArgs e)
    {
        OrderWindow orderWindow;
        if (sender is DataGrid dataGrid
            && dataGrid.SelectedItem is BO.OrderInList orderInList)
        {
            orderWindow = new OrderWindow(orderInList.OrderId);
        }
        else
        {
            orderWindow = new OrderWindow(0);
        }
        orderWindow.Show();
    }

    private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
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
            try
            {
                s_bl.Order.Cancel(CURRENT_MANAGER_ID, orderInList.OrderId);
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
            catch (SmtpException ex)
            {
                MessageBox.Show($"הזמנה מס' {orderInList.OrderId} בוטלה בהצלחה ({ex.Message})");
                //Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error canceling the order: {ex.Message}");
            }
        }
    }

}
