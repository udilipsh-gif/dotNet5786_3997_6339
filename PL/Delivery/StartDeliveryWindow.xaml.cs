using System;
using System.Collections.Generic;
using System.ComponentModel;
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
/// Interaction logic for StartDeliveryWindow.xaml
/// </summary>
public partial class StartDeliveryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int userId;

    private readonly int courierId;

    public ICommand SelectOrderCommand { get; private set; }

    public IEnumerable<object> EnumForSoring
    {
        get
        {
            var list = new List<object>
            {
                new { Id = (BO.TypeOfOrder?)null, Name = "הצג הכל / ללא סינון" }
            };
            var enumValues = Enum.GetValues(typeof(BO.TypeOfOrder))
                                 .Cast<BO.TypeOfOrder>()
                                 .Select(e => new
                                 {
                                     Id = (BO.TypeOfOrder?)e,
                                     Name = Tools.GetDescription(e),
                                 });
            return list.Concat(enumValues);

        }
    }

    private BO.OpenOrderInListField SelctedSort
    {
        get => (BO.OpenOrderInListField)GetValue(SelctedSortProperty);
        set => SetValue(SelctedSortProperty, value);
    }

    public static readonly DependencyProperty SelctedSortProperty =
        DependencyProperty.Register("SelctedSort", typeof(BO.OpenOrderInListField),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    public BO.TypeOfOrder? SelctedFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelctedFilterProperty);
        set => SetValue(SelctedFilterProperty, value);
    }
    public static readonly DependencyProperty SelctedFilterProperty =
        DependencyProperty.Register("SelctedFilter", typeof(BO.TypeOfOrder?),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    public List<BO.OpenOrderInList> DeliveryListView
    {
        get => (List<BO.OpenOrderInList>)GetValue(DeliveryListViewProperty);
        set => SetValue(DeliveryListViewProperty, value);
    }

    public static readonly DependencyProperty DeliveryListViewProperty =
        DependencyProperty.Register("DeliveryListView", typeof(List<BO.OpenOrderInList>),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));


    public StartDeliveryWindow(int userId, int courierId)
    {
        this.userId = userId;

        this.courierId = courierId;

        // אתחול ה-Command לפני InitializeComponent
        SelectOrderCommand = new RelayCommand<BO.OpenOrderInList>(ExecuteSelectOrder, CanSelectOrder);

        InitializeComponent();
    }

    private void StartDeliveryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Order.AddObserver(orderListObserver);
        UpdateOrdersList();

    }

    private void StartDeliveryWindow_Closed(object sender, RoutedEventArgs e)
    {
        s_bl.Order.RemoveObserver(orderListObserver);
        this.Close();
    }

    private void orderListObserver()
        => UpdateOrdersList();

    private void StartDeliveryWindow_Closed(object sender, EventArgs e)
    {
        s_bl.Order.RemoveObserver(orderListObserver);
    }

    private void UpdateOrdersList()
    {
        DeliveryListView = [.. s_bl.Order.GetOpen(userId, courierId, SelctedFilter, SelctedSort)];
    }

    private void ComboBox_FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelctedFilter = (BO.TypeOfOrder)((ComboBox)sender).SelectedItem;
        UpdateOrdersList();
    }

    private void ComboBox_SortSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelctedSort = (BO.OpenOrderInListField)((ComboBox)sender).SelectedItem;
        UpdateOrdersList();
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

            s_bl.Order.StartDelivery(userId, courierId, selectedOrder.OrderId);

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

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var Selcte = SelctedFilter;
        orderListObserver();
    }

}
