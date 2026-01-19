using BO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PL;

public partial class MapPopupWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public OpenOrderInList Order { get; }
    public int UserId { get; }
    public string? MapImageUrl
    {
        get => (string) GetValue(MapImageUrlProperty);
        set => SetValue(MapImageUrlProperty, value);
    }

    public static readonly DependencyProperty MapImageUrlProperty =
    DependencyProperty.Register("MapImageUrl", typeof(string),
    typeof(MapPopupWindow), new PropertyMetadata(string.Empty));

    public string? RouteInfo
    {
        get => (string)GetValue(RouteInfoProperty);
        set => SetValue(RouteInfoProperty, value);
    }

    public static readonly DependencyProperty RouteInfoProperty =
    DependencyProperty.Register("RouteInfo", typeof(string),
    typeof(MapPopupWindow), new PropertyMetadata(string.Empty));

    public event Action<OpenOrderInList>? OnCollectClicked;

    private bool _isClosing = false;

    public MapPopupWindow(int userId, OpenOrderInList order, TheTypeShipment shipmentType = TheTypeShipment.CAR)
    {
        Order = order;
        DataContext = this;
        UserId = userId;

        LoadMapData(order, shipmentType);  

        InitializeComponent();
    }

    private async void LoadMapData(OpenOrderInList order, TheTypeShipment shipmentType)
    {
        try
        {
            // Get full order details for coordinates

            var fullOrder = await s_bl.Order.Read(UserId, order.OrderId);
            if (fullOrder == null) return;

            // Get route info (cached)
            var route = await s_bl.Delivery.GetRouteFromStore(
                fullOrder.Latitude,
                fullOrder.Longitude,
                shipmentType);

            if (route != null)
            {
                RouteInfo = $"מרחק: {route.DistanceText} | זמן משוער: {route.DurationText}";

                // Build static map URL
                MapImageUrl = await s_bl.Delivery.GetStaticMapUrlFromStore(
                    fullOrder.Latitude,
                    fullOrder.Longitude,
                    shipmentType,
                    width: 380,
                    height: 200);
            }
        }
        catch
        {
            RouteInfo = "לא ניתן לטעון מידע על המסלול";
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _isClosing = true;
        base.OnClosing(e);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isClosing) Close();
    }

    private void CollectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isClosing) return;
        OnCollectClicked?.Invoke(Order);
        Close();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!_isClosing) Close();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }
}