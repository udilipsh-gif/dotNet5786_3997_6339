using BO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PL;

/// <summary>
/// A lightweight popup window that displays route information and a static map for a specific order.
/// This window allows the courier to preview the delivery location and "Collect" the order.
/// </summary>
public partial class MapPopupWindow : Window
{
    /// <summary>
    /// Access point to the Business Logic layer.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    #region Properties

    /// <summary>
    /// The order entity being displayed in this popup.
    /// </summary>
    public OpenOrderInList Order { get; }

    /// <summary>
    /// The ID of the current user (Courier).
    /// </summary>
    public int UserId { get; }

    /// <summary>
    /// Gets or sets the URL source for the static map image.
    /// Bound to the Image control in XAML.
    /// </summary>
    public string? MapImageUrl
    {
        get => (string?)GetValue(MapImageUrlProperty);
        set => SetValue(MapImageUrlProperty, value);
    }

    public static readonly DependencyProperty MapImageUrlProperty =
        DependencyProperty.Register(nameof(MapImageUrl), typeof(string),
        typeof(MapPopupWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the text description of the route (Distance and Duration).
    /// Bound to a TextBlock in XAML.
    /// </summary>
    public string? RouteInfo
    {
        get => (string?)GetValue(RouteInfoProperty);
        set => SetValue(RouteInfoProperty, value);
    }

    public static readonly DependencyProperty RouteInfoProperty =
        DependencyProperty.Register(nameof(RouteInfo), typeof(string),
        typeof(MapPopupWindow), new PropertyMetadata(null));

    #endregion

    /// <summary>
    /// Event triggered when the user clicks the "Collect" (Take Order) button.
    /// Pass the order object back to the parent window.
    /// </summary>
    public event Action<OpenOrderInList>? OnCollectClicked;

    /// <summary>
    /// Flag to prevent race conditions during window closing.
    /// </summary>
    private bool _isClosing = false;

    #region Constructor & Initialization

    /// <summary>
    /// Initializes a new instance of the MapPopupWindow.
    /// </summary>
    /// <param name="userId">The ID of the courier.</param>
    /// <param name="order">The order to display.</param>
    /// <param name="shipmentType">The transport method (affects route calculation).</param>
    public MapPopupWindow(int userId, OpenOrderInList order, TheTypeShipment shipmentType = TheTypeShipment.CAR)
    {
        Order = order;
        DataContext = this;
        UserId = userId;

        // Start loading data asynchronously immediately upon creation
        LoadMapData(order, shipmentType);

        InitializeComponent();
    }

    /// <summary>
    /// Asynchronously fetches route details (distance/time) and a static map image URL from the BL.
    /// </summary>
    private async void LoadMapData(OpenOrderInList order, TheTypeShipment shipmentType)
    {
        try
        {
            // 1. Get full order details to extract precise coordinates
            var fullOrder = await s_bl.Order.Read(UserId, order.OrderId);
            if (fullOrder == null) return;

            // 2. Calculate route from store to customer (uses caching in BL)
            var route = await s_bl.Delivery.GetRouteFromStore(
                fullOrder.Latitude,
                fullOrder.Longitude,
                shipmentType);

            if (route != null)
            {
                // Update UI with formatted route info
                RouteInfo = $"מרחק: {route.DistanceText} | זמן משוער: {route.DurationText}";

                // 3. Generate the Static Map URL
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
            // Fail gracefully without crashing the UI
            RouteInfo = "לא ניתן לטעון מידע על המסלול";
        }
    }

    #endregion

    #region Window Lifecycle & Events

    /// <summary>
    /// Overrides the OnClosing method to set the closing flag.
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _isClosing = true;
        base.OnClosing(e);
    }

    private void MapPopupWindow_Loaded(object sender, EventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
    }

    private void MapPopupWindow_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
    }

    /// <summary>
    /// Automatically closes the popup when it loses focus (user clicks outside).
    /// </summary>
    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!_isClosing) Close();
    }

    #endregion

    #region UI Interaction

    /// <summary>
    /// Closes the popup manually via the Close button.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isClosing) Close();
    }

    /// <summary>
    /// Handles the "Collect Order" action.
    /// Triggers the OnCollectClicked event and closes the popup.
    /// </summary>
    private void CollectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isClosing) return;

        OnCollectClicked?.Invoke(Order);
        Close();
    }

    /// <summary>
    /// Enables dragging the window by clicking and holding the header area.
    /// Necessary because the window likely has WindowStyle="None".
    /// </summary>
    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    #endregion
}