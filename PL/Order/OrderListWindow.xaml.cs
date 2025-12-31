using PL.Courier;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order
{
    public partial class OrderListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId;

        public ObservableCollection<BO.OrderInList> OrderList
        {
            get { return (ObservableCollection<BO.OrderInList>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }

        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register(nameof(OrderList), typeof(ObservableCollection<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

        // Property לסינון לפי סטטוס
        public BO.ScheduleStatus? SelectedStatusFilter
        {
            get { return (BO.ScheduleStatus?)GetValue(SelectedStatusFilterProperty); }
            set { SetValue(SelectedStatusFilterProperty, value); }
        }

        public static readonly DependencyProperty SelectedStatusFilterProperty =
            DependencyProperty.Register(nameof(SelectedStatusFilter), typeof(BO.ScheduleStatus?), typeof(OrderListWindow), new PropertyMetadata(null));

        public OrderListWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ממלאים את ComboBox בערכים
            StatusComboBox.ItemsSource = new BO.ScheduleStatus?[]
            {
        null,                     
        BO.ScheduleStatus.ONTYME,
        BO.ScheduleStatus.INRISK,
        BO.ScheduleStatus.LATE,
        BO.ScheduleStatus.CANCELLED
            };
            StatusComboBox.SelectedIndex = 0; // ברירת מחדל - הכל

            LoadOrders(); // טוענים את ההזמנות
        }


        // טעינת ההזמנות לפי הסינון שנבחר
        private void LoadOrders()
        {
            try
            {
                BO.OrderInListField? filterField = null;
                object? filterValue = null;

                if (SelectedStatusFilter != null)
                {
                    filterField = BO.OrderInListField.OrderStatus;
                    filterValue = SelectedStatusFilter;
                }

                // קריאה ל-BL עם פילטר וסדר מיון לפי Id
                var orders = s_bl.Order.ReadAll(
                    CURRENT_MANAGER_ID,
                    filterField,
                    filterValue,
                    BO.OrderInListField.OrderId
                );

                OrderList = new ObservableCollection<BO.OrderInList>(orders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // אירוע שינוי ב-ComboBox
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStatusFilter = StatusComboBox.SelectedItem as BO.ScheduleStatus?;
            LoadOrders();
        }
    }
}
