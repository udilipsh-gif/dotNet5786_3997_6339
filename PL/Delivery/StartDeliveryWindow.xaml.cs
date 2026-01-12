using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

/// <summary>
/// Interaction logic for StartDeliveryWindow.xaml
/// </summary>
public partial class StartDeliveryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public int UserId { get; private init; }

    public int courierId { get; private init; }

    public ICommand SelectOrderCommand { get; private set; }

    public IEnumerable<Tools.SelectionItem> EnumForSoring
    {
        get => Tools.GetEnumList<BO.TypeOfOrder>("הכל");
    }



    public BO.OpenOrderInListField SelctedSort
    {
        get => (BO.OpenOrderInListField)GetValue(SelctedSortProperty);
        set => SetValue(SelctedSortProperty, value);
    }

    public static readonly DependencyProperty SelctedSortProperty =
        DependencyProperty.Register("SelctedSort", typeof(BO.OpenOrderInListField),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

   // public BO.OrderStatus SelectedOrderStatusFilter
   // {
   //     get => (BO.OrderStatus)GetValue(SelectedOrderStatusFilterProperty);
   //     set => SetValue(SelectedOrderStatusFilterProperty, value);
   // }
   // public static readonly DependencyProperty SelectedOrderStatusFilterProperty =
   //     DependencyProperty.Register("SelectedOrderStatusFilter", typeof(BO.OrderStatus?),
   //         typeof(StartDeliveryWindow), new PropertyMetadata(null));

   //public static IEnumerable<BO.OrderStatus> OrderStatusFilterOptions
   // {
   //     get => Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>();
   // }

    public BO.TypeOfOrder? SelctedFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelctedFilterProperty);
        set
        { 
            SetValue(SelctedFilterProperty, value); 
            UpdateOrdersList(); 
        }
    }
    public static readonly DependencyProperty SelctedFilterProperty =
        DependencyProperty.Register("SelctedFilter", typeof(BO.TypeOfOrder?),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    public ObservableCollection<BO.OpenOrderInList> DeliveryListView
    {
        get { return (ObservableCollection<BO.OpenOrderInList>)GetValue(DeliveryListViewProperty); }
        set { SetValue(DeliveryListViewProperty, value); }
    }

    public static readonly DependencyProperty DeliveryListViewProperty =
    DependencyProperty.Register("DeliveryListView", typeof(ObservableCollection<BO.OpenOrderInList>),
        typeof(StartDeliveryWindow), new PropertyMetadata(null));


    public StartDeliveryWindow(int userId, int courierId)
    {
        this.UserId = userId;

        this.courierId = courierId;

        // אתחול ה-Command לפני InitializeComponent
        SelectOrderCommand = new RelayCommand<BO.OpenOrderInList>(ExecuteSelectOrder, CanSelectOrder);

        InitializeComponent();
    }

    private void StartDeliveryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.RunSafe(() => s_bl.Order.AddObserver(orderListObserver));
        UpdateOrdersList();

    }

    private void StartDeliveryWindow_Closed(object sender, RoutedEventArgs e)
    {
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(orderListObserver));
        this.Close();
    }

    private void orderListObserver()
        => UpdateOrdersList();

    private void StartDeliveryWindow_Closed(object sender, EventArgs e)
    {
        Tools.RunSafe(() =>  s_bl.Order.RemoveObserver(orderListObserver));
    }

    private void UpdateOrdersList()
    {
        var DeliveryList = Tools.GetSafeFromBl(() =>  
                s_bl.Order.GetOpen(UserId, courierId, SelctedFilter, SelctedSort),
                new List<BO.OpenOrderInList>());

        
        if (DeliveryListView == null)
        {
            DeliveryListView = new ObservableCollection<BO.OpenOrderInList>(DeliveryList); 
        }
        else
        {
            DeliveryListView.Clear(); // מחיקת הישנים
            foreach (var item in DeliveryList)
            {
                DeliveryListView.Add(item); // הוספת החדשים
            }
        }
    }

    private bool CanSelectOrder(BO.OpenOrderInList? selectedOrder)
    {
        // תמיד מאפשר ביצוע אם יש הזמנה נבחרת
        return selectedOrder != null;
    }

    private void ExecuteSelectOrder(BO.OpenOrderInList? selectedOrder)
    {
        try
        {
            if (selectedOrder == null)
                throw new BO.BlInvalidOperationException("סיבת סיום המשלוח לא תקפה");

            Tools.RunSafe(() =>  s_bl.Order.StartDelivery(UserId, courierId, selectedOrder.OrderId));

            MessageBox.Show("המשלוח התחיל בהצלחה!", "הצלחה",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בהתחלת המשלוח: {ex.Message}", "שגיאה",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
