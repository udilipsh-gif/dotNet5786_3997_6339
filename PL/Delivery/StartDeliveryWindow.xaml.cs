using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PL;

/// <summary>
/// Interaction logic for StartDeliveryWindow.xaml
/// </summary>
public partial class StartDeliveryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public int UserId { get; private init; }

    private int courierId { get;  init; }

    private BO.TheTypeShipment typeShipment { get;  init; }

    public IEnumerable<Tools.SelectionItem> EnumTypeOfOrder
    {
        get => (IEnumerable<Tools.SelectionItem>)GetValue(EnumTypeOfOrderProperty);
        set => SetValue(EnumTypeOfOrderProperty, value);
    }

    public static readonly DependencyProperty EnumTypeOfOrderProperty =
        DependencyProperty.Register(nameof(EnumTypeOfOrder), typeof(IEnumerable<Tools.SelectionItem>),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    public BO.TypeOfOrder? SelectedFilter
    {
        get => (BO.TypeOfOrder?)GetValue(SelectedFilterProperty);
        set => SetValue(SelectedFilterProperty, value);
    }
    public static readonly DependencyProperty SelectedFilterProperty =
        DependencyProperty.Register(nameof(SelectedFilter), typeof(BO.TypeOfOrder?),
            typeof(StartDeliveryWindow), new PropertyMetadata(null));

    public ObservableCollection<BO.OpenOrderInList> DeliveryListView
    {
        get { return (ObservableCollection<BO.OpenOrderInList>)GetValue(DeliveryListViewProperty); }
        set { SetValue(DeliveryListViewProperty, value); }
    }

    public static readonly DependencyProperty DeliveryListViewProperty =
    DependencyProperty.Register("DeliveryListView", typeof(ObservableCollection<BO.OpenOrderInList>),
        typeof(StartDeliveryWindow), new PropertyMetadata(null));


    public StartDeliveryWindow(int userId, int courierId, BO.TheTypeShipment TypeShipment,  IEnumerable<Tools.SelectionItem>? enumTypeOfOrder)
    {
        this.UserId = userId;

        this.courierId = courierId;

        this.typeShipment = TypeShipment;

        this.EnumTypeOfOrder = enumTypeOfOrder ?? new List<Tools.SelectionItem>();

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
        Tools.RunSafe(() => s_bl.Order.RemoveObserver(orderListObserver));
    }

    private void UpdateOrdersList()
    {
        var DeliveryList = Tools.GetSafeFromBl(() =>
                s_bl.Order.GetOpen(UserId, courierId, SelectedFilter, null)
               .OrderByDescending(o => o.OrderId).ToList() ,
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

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => UpdateOrdersList();

    // הוסף שדה לשמירת החלון הצף הפתוח
    private MapPopupWindow? _currentPopup;

    // הוסף את האירוע הזה
    private void DataGridRow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (IsClickInsideButton(e.OriginalSource))
        {
            // אם זה כפתור, אל תפתח את הפופ-אפ ותן לאירוע להמשיך לכפתור האיסוף
            return;
        }
        _currentPopup?.Close();
        _currentPopup = null;

        if (sender is DataGridRow row && row.Item is BO.OpenOrderInList selectedOrder)
        {
            // קבל את מיקום העכבר על המסך
            var screenPoint = PointToScreen(e.GetPosition(this));

            // צור את החלון הצף
            _currentPopup = new MapPopupWindow(UserId, selectedOrder, typeShipment)
            {
                Left = screenPoint.X + 20,
                Top = screenPoint.Y - 50
            };

            // האזן לאירוע איסוף
            _currentPopup.OnCollectClicked += (order) =>
            {
                CollectOrderInternal(order);
            };

            _currentPopup.Show();
        }
    }

    private bool IsClickInsideButton(object originalSource)
    {
        if(originalSource is DependencyObject depObj)
        {
            while (depObj != null)
            {
                // אם הגענו לכפתור - זה אומר שהלחיצה הייתה עליו
                if (depObj is Button)
                {
                    return true;
                }

                // אם הגענו לשורה עצמה, סימן שלא מצאנו כפתור בדרך
                if (depObj is DataGridRow)
                {
                    return false;
                }

                // עלייה למעלה בעץ הויזואלי
                depObj = VisualTreeHelper.GetParent(depObj);
            }
        }
        return false;
    }

    private void CollectOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BO.OpenOrderInList selectedOrder)
        {
            CollectOrderInternal(selectedOrder);
        }
    }

    private void CollectOrderInternal(BO.OpenOrderInList selectedOrder)
    {
        try
        {
            s_bl.Order.StartDelivery(UserId, courierId, selectedOrder.OrderId);

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
