using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order;

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

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register(nameof(OrderList), typeof(ObservableCollection<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));


    public BO.ScheduleStatus ScheduleStatusFilter { get; set; } = BO.ScheduleStatus.ALL;


    public BO.TypeOfOrder? TypeOfOrderFilter { get; set; } = BO.TypeOfOrder.ALL;


    private void LoadOrders()
    {
        // 1. שליפת כל הנתונים מה-BL ללא סינון ראשוני כלל
        var allOrders = s_bl.Order.ReadAll(CURRENT_MANAGER_ID, null, null, BO.OrderInListField.OrderId);

        // 2. ביצוע סינון כפול בעזרת LINQ
        var filteredResults = allOrders.Where(order =>
            (ScheduleStatusFilter == BO.ScheduleStatus.ALL || order.ScheduleStatus == ScheduleStatusFilter) &&
            (TypeOfOrderFilter == BO.TypeOfOrder.ALL || order.TypeOfOrder == TypeOfOrderFilter)
        );

        // 3. עדכון הרשימה המוצגת במסך
        OrderList = new ObservableCollection<BO.OrderInList>(filteredResults);
        //OrderList = new ObservableCollection<BO.OrderInList>(s_bl.Order.ReadAll(CURRENT_MANAGER_ID,null,null,null));
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error canceling the order: {ex.Message}");
            }
        }
    }

}
