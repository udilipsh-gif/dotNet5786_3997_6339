using BO;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace PL.Courier
{
    /// <summary>
    /// Courier List Window - Displays a list of all couriers in the system with filtering options
    /// </summary>
    public partial class CourierListWindow : Window
    {
        /// <summary>
        /// Business logic layer object
        /// </summary>
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Constructor - Creates a new courier list window
        /// </summary>
        public CourierListWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The list of couriers displayed in the window
        /// </summary>
        public IEnumerable<BO.CourierInList> CourierInList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierInListProperty); }
            set { SetValue(CourierInListProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for the courier list
        /// </summary>
        public static readonly DependencyProperty CourierInListProperty =
            DependencyProperty.Register(nameof(CourierInList), typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

        /// <summary>
        /// The current filter selector (All/Active/Inactive)
        /// </summary>
        public BO.CourierFieldFilter CourierFilter
        {
            get { return (BO.CourierFieldFilter)GetValue(CourierFilterProperty); }
            set { SetValue(CourierFilterProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for the filter selector
        /// </summary>
        public static readonly DependencyProperty CourierFilterProperty =
            DependencyProperty.Register(nameof(CourierFilter), typeof(BO.CourierFieldFilter), typeof(CourierListWindow), new PropertyMetadata(BO.CourierFieldFilter.All));

        /// <summary>
        /// Window loaded event - Updates the courier list and adds an observer for changes
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCourierList();
            s_bl.Courier.AddObserver(courierListObserver);
        }

        /// <summary>
        /// Window close event - Removes the observer for changes
        /// </summary>
        private void Window_Close(object? sender, EventArgs e)
        {
            s_bl.Courier.RemoveObserver(courierListObserver);
        }

        /// <summary>
        /// Observer function - Triggered when there is a change in the courier list
        /// </summary>
        private void courierListObserver()
        {
            UpdateCourierList();
        }

        /// <summary>
        /// Updates the courier list according to the current filter
        /// </summary>
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
            CourierInList = s_bl.Courier.ReadAll(s_bl.Admin.GetConfig().ManagerId, isActive, BO.CourierFieldSort.Id);
        }

        /// <summary>
        /// ComboBox selection changed event - Updates the courier list
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCourierList();
        }

        /// <summary>
        /// Add/Edit courier button click event - Opens courier window for adding or editing
        /// </summary>
        private void Add_Edit_Courier_Click(object sender, RoutedEventArgs e)
        {
            CourierWindow courierWindow;
            if (sender is DataGrid dataGrid
                && dataGrid.SelectedItem is BO.CourierInList courierInList)
            {
                // Edit existing courier - הכפתור בתוך השורה
                //courierWindow = new CourierWindow(courierInList.Id);
                courierWindow = new CourierWindow(courierInList.Id);
            }
            else
            {
                // Add new courier - הכפתור מחוץ ל-DataGrid
                courierWindow = new CourierWindow(0);
            }
            courierWindow.ShowDialog();
        }
    }
}
