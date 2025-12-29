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
    /// Interaction logic for CourierListWindow.xaml
    /// </summary>
    public partial class CourierListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public CourierListWindow()
        {
            InitializeComponent();

        }

        public IEnumerable<BO.CourierInList> CourierInList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierInListProperty); }
            set { SetValue(CourierInListProperty, value); }
        }

        public static readonly DependencyProperty CourierInListProperty =
            DependencyProperty.Register(nameof(CourierInList), typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

        public BO.CourierFieldFilter CourierFilter
        {
            get { return (BO.CourierFieldFilter)GetValue(CourierFilterProperty); }
            set { SetValue(CourierFilterProperty, value); }
        }

        public static readonly DependencyProperty CourierFilterProperty =
            DependencyProperty.Register(nameof(CourierFilter), typeof(BO.CourierFieldFilter), typeof(CourierListWindow), new PropertyMetadata(BO.CourierFieldFilter.All));

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCourierList();

            s_bl.Courier.AddObserver(courierListObserver);
        }

        private void Window_Close(object? sender, EventArgs e)
        {
            s_bl.Courier.RemoveObserver(courierListObserver);
        }

        private void courierListObserver()
        {
            UpdateCourierList();
        }

        private void UpdateCourierList()
        {
            bool? isActive = CourierFilter switch
            {
                BO.CourierFieldFilter.IsActive => true,
                BO.CourierFieldFilter.InActive => false,
                BO.CourierFieldFilter.All => null,
                _ => null
            };

            CourierInList = s_bl.Courier.ReadAll(s_bl.Admin.GetConfig().ManagerId, isActive, BO.CourierFieldSort.Id);
     
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCourierList();
            
        }
    }
}
