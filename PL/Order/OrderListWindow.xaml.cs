using BO;
using PL.Courier;
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


    public BO.TypeOfOrder TypeOfOrderFilter { get; set; } = BO.TypeOfOrder.ALL;

    //private void LoadOrders()
    //{
    //    var orders =
    //    (ScheduleStatusFilter == BO.ScheduleStatus.ALL)
    //? s_bl?.Order.ReadAll(CURRENT_MANAGER_ID, null, null, null)!
    //: s_bl?.Order.ReadAll(CURRENT_MANAGER_ID, BO.OrderInListField.ScheduleStatus, ScheduleStatusFilter, null)!;

    //    OrderList = new ObservableCollection<BO.OrderInList>(orders);
    //}
    private void LoadOrders()
    {
        // 1. שליפת כל הנתונים מה-BL ללא סינון ראשוני כלל
        var allOrders = s_bl?.Order.ReadAll(CURRENT_MANAGER_ID, null, null, null);

        // 2. ביצוע סינון כפול בעזרת LINQ
        var filteredResults = allOrders.Where(order =>
            (ScheduleStatusFilter == BO.ScheduleStatus.ALL || order.ScheduleStatus == ScheduleStatusFilter) &&
            (TypeOfOrderFilter == BO.TypeOfOrder.ALL || order.TypeOfOrder == TypeOfOrderFilter)
        );

        // 3. עדכון הרשימה המוצגת במסך
        OrderList = new ObservableCollection<BO.OrderInList>(filteredResults);
    }

    private void orderListObserver()
        => LoadOrders();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadOrders();
        s_bl.Order.AddObserver(orderListObserver);
    }

    private void Window_Closed(object sender, EventArgs e)
        => s_bl.Order.RemoveObserver(orderListObserver);

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadOrders();
    }

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

    //// Property לסינון לפי סטטוס
    //public BO.ScheduleStatus? SelectedStatusFilter
    //{
    //    get { return (BO.ScheduleStatus?)GetValue(SelectedStatusFilterProperty); }
    //    set { SetValue(SelectedStatusFilterProperty, value); }
    //}

    //public static readonly DependencyProperty SelectedStatusFilterProperty =
    //    DependencyProperty.Register(nameof(SelectedStatusFilter), typeof(BO.ScheduleStatus?), typeof(OrderListWindow), new PropertyMetadata(null));

    //public BO.TypeOfOrder? SelectedTypeFilter
    //{
    //    get { return (BO.TypeOfOrder?)GetValue(SelectedTypeFilterProperty); }
    //    set { SetValue(SelectedTypeFilterProperty, value); }
    //}

    //public static readonly DependencyProperty SelectedTypeFilterProperty =
    //    DependencyProperty.Register(
    //        nameof(SelectedTypeFilter),
    //        typeof(BO.TypeOfOrder?),
    //        typeof(OrderListWindow),
    //        new PropertyMetadata(null));

    //public OrderListWindow()
    //{
    //    InitializeComponent();
    //    DataContext = this;
    //    Loaded += Window_Loaded;
    //}

    //private void Window_Loaded(object sender, RoutedEventArgs e)
    //{
    //    StatusComboBox.ItemsSource =
    //    new BO.ScheduleStatus?[] { null }  // null = הכל
    //    .Concat(new CourierFieldFilter().Cast<BO.ScheduleStatus?>());
    //    StatusComboBox.SelectedIndex = 0;  // ברירת מחדל = הכל

    //    // סוג הזמנה
    //    TypeComboBox.ItemsSource =
    //        new BO.TypeOfOrder?[] { null }  // null = הכל
    //        .Concat(new CourierFieldSort().Cast<BO.TypeOfOrder?>());
    //    TypeComboBox.SelectedIndex = 0;  // ברירת מחדל = הכל

    //    LoadOrders();
    //}



    //// טעינת ההזמנות לפי הסינון שנבחר
    //private void LoadOrders()
    //{
    //    try
    //    {
    //        BO.OrderInListField? filterField = null;
    //        object? filterValue = null;

    //        // סינון לפי סטטוס
    //        if (SelectedStatusFilter != null&& SelectedStatusFilter !=BO.ScheduleStatus.ALL)  // אם null → הכל, אין סינון
    //        {
    //            filterField = BO.OrderInListField.OrderStatus;
    //            filterValue = SelectedStatusFilter;
    //        }

    //        // טוען את ההזמנות מה-BL
    //        var orders = s_bl.Order.ReadAll(
    //            CURRENT_MANAGER_ID,
    //            filterField,
    //            filterValue,
    //            BO.OrderInListField.OrderId
    //        );

    //        // סינון נוסף לפי סוג הזמנה
    //        if (SelectedTypeFilter != null&& SelectedTypeFilter != BO.TypeOfOrder.ALL)  // אם null → הכל
    //        {
    //            orders = orders.Where(o => o.TypeOfOrder == SelectedTypeFilter);
    //        }

    //        OrderList = new ObservableCollection<BO.OrderInList>(orders);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}




}
