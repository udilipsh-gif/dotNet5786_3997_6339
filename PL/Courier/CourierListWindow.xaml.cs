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
using System.Collections.ObjectModel; 

namespace PL;

/// <summary>
/// Interaction logic for CourierListWindow.xaml - displays and manages a list of couriers.
/// </summary>
/// <remarks>
/// This window provides a comprehensive view of all couriers in the system with the following features:
/// <list type="bullet">
/// <item><description>Filtering by active status (All/Active/Inactive)</description></item>
/// <item><description>Real-time updates through observer pattern</description></item>
/// <item><description>Navigation to add new couriers or edit existing ones</description></item>
/// <item><description>Display of courier statistics including on-time and late deliveries</description></item>
/// </list>
/// </remarks>
public partial class CourierListWindow : Window
{
    /// <summary>
    /// Business logic layer instance for accessing courier operations.
    /// </summary>
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The ID of the currently logged-in manager performing operations.
    /// </summary>
    private int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId;

    public ICommand SelectCourierCommand { get; private set; }

    public ICommand AddCourierCommand { get; private set; }


    /// <summary>
    /// Initializes a new instance of the CourierListWindow class.
    /// </summary>
    public CourierListWindow()
    {
        SelectCourierCommand = new RelayCommand<BO.CourierInList>(Add_Edit_Courier_Click, CanSelectCourier);
        AddCourierCommand = new RelayCommand(_ => Add_Edit_Courier_Click(null));

        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the observable collection of couriers displayed in the window.
    /// </summary>
    /// <remarks>
    /// This collection is bound to the DataGrid and automatically updates the UI when modified.
    /// Changes to this collection will be reflected in the user interface immediately.
    /// </remarks>
    public ObservableCollection<BO.CourierInList> CourierInList
    {
        get => (ObservableCollection<BO.CourierInList>)GetValue(CourierInListProperty);
        set => SetValue(CourierInListProperty, value);
    }

    /// <summary>
    /// Dependency property for the CourierInList collection.
    /// </summary>
    public static readonly DependencyProperty CourierInListProperty =
        DependencyProperty.Register(nameof(CourierInList), typeof(ObservableCollection<BO.CourierInList>),
            typeof(CourierListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the current filter selection (All/Active/Inactive).
    /// </summary>
    /// <remarks>
    /// This property is bound to the ComboBox control and determines which couriers are displayed.
    /// When changed, it triggers an update of the courier list to match the selected filter.
    /// </remarks>
    public BO.CourierFieldFilter CourierFilter
    {
        get { return (BO.CourierFieldFilter)GetValue(CourierFilterProperty); }
        set { SetValue(CourierFilterProperty, value); }
    }

    /// <summary>
    /// Dependency property for the CourierFilter property.
    /// </summary>
    public static readonly DependencyProperty CourierFilterProperty =
       DependencyProperty.Register(nameof(CourierFilter), typeof(BO.CourierFieldFilter), 
           typeof(CourierListWindow), new PropertyMetadata(BO.CourierFieldFilter.All));

    /// <summary>
    /// Handles the window loaded event, initializes the courier list and registers for updates.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method performs two key actions:
    /// <list type="number">
    /// <item><description>Loads the initial courier list based on the default filter</description></item>
    /// <item><description>Registers an observer to receive notifications when courier data changes</description></item>
    /// </list>
    /// </remarks>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Courier.AddObserver(courierListObserver);
        UpdateCourierList();
    }

    /// <summary>
    /// Handles the window close event, unregisters from update notifications.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This cleanup method removes the observer registration to prevent memory leaks
    /// and ensure the window can be properly garbage collected.
    /// </remarks>
    private void Window_Close(object? sender, EventArgs e)
    {
        s_bl.Courier.RemoveObserver(courierListObserver);
    }

    /// <summary>
    /// Observer callback method triggered when courier data changes in the business layer.
    /// </summary>
    /// <remarks>
    /// This method is called automatically when:
    /// <list type="bullet">
    /// <item><description>A new courier is added</description></item>
    /// <item><description>An existing courier is updated</description></item>
    /// <item><description>A courier is deleted</description></item>
    /// <item><description>A courier's delivery status changes</description></item>
    /// </list>
    /// It refreshes the displayed list to reflect the latest data.
    /// </remarks>
    private void courierListObserver()
    {
        Dispatcher.Invoke(() =>
        {
            UpdateCourierList();
        });
    }

    /// <summary>
    /// Updates the courier list according to the currently selected filter.
    /// </summary>
    /// <remarks>
    /// This method:
    /// <list type="number">
    /// <item><description>Converts the filter enum value to the appropriate boolean or null value</description></item>
    /// <item><description>Calls the business logic layer to retrieve the filtered courier list</description></item>
    /// <item><description>Wraps the result in an ObservableCollection for UI binding</description></item>
    /// </list>
    /// The couriers are sorted by ID by default.
    /// </remarks>
    private void UpdateCourierList()
    {
        // Convert filter value to appropriate boolean value
        bool? isActive = CourierFilter switch
        {
            BO.CourierFieldFilter.IsActive => true,
            BO.CourierFieldFilter.InActive => false,
            BO.CourierFieldFilter.All => null,
            _ => null
        };

        // Call business layer to get the filtered courier list
        CourierInList = new ObservableCollection<BO.CourierInList>(s_bl.Courier.ReadAll(CURRENT_MANAGER_ID, isActive, BO.CourierFieldSort.Id).Where(e => e.Id != 0));
    }

    /// <summary>
    /// Handles the filter ComboBox selection changed event, refreshes the courier list.
    /// </summary>
    /// <param name="sender">The ComboBox control that triggered the event.</param>
    /// <param name="e">Event arguments containing selection details.</param>
    /// <remarks>
    /// When the user changes the filter selection, this method is called to update
    /// the displayed list of couriers to match the new filter criteria.
    /// </remarks>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCourierList();
    }

    private bool CanSelectCourier(BO.CourierInList? selectedCourier)
    {
        return selectedCourier != null;
    }

    /// <summary>
    /// Handles the add/edit courier button and DataGrid double-click events.
    /// </summary>
    /// <param name="sender">The control that triggered the event (Button or DataGrid).</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method supports two scenarios:
    /// <list type="bullet">
    /// <item><description>Double-clicking a row in the DataGrid opens the CourierWindow in edit mode for the selected courier</description></item>
    /// <item><description>Clicking the "Add Courier" button opens the CourierWindow in add mode with a new courier</description></item>
    /// </list>
    /// The CourierWindow is displayed as a non-modal window, allowing multiple courier windows to be open simultaneously.
    /// </remarks>
    private void Add_Edit_Courier_Click(BO.CourierInList? selectedCourier)
    {
        try
        {
            CourierWindow courierWindow;

            if (selectedCourier != null)
            {
                courierWindow = new CourierWindow(selectedCourier.Id);
            }
            else
            {
                courierWindow = new CourierWindow(0);
            }
            courierWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בפתיחת החלון: {ex.Message}", "שגיאה",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

