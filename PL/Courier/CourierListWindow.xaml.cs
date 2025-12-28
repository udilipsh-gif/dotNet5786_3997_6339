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
        //private void queryCourseList()
        //    => Courier = (Semester == BO.Courier.) ?
        //        s_bl?.Courier.ReadAll()! : s_bl?.Course.ReadAll(null, BO.CourseFieldFilter.SemesterName, Semester)!;
        private void queryCourierList() => s_bl.Courier.ReadAll(s_bl.Admin.GetConfig().ManagerId, true, BO.CourierFieldSort.Id);

        private void courierListObserver()
             => queryCourierList();


        private void Window_Close(object? sender, EventArgs e)
             => s_bl.Courier.RemoveObserver(courierListObserver);


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CourierInList = s_bl.Courier.ReadAll(s_bl.Admin.GetConfig().ManagerId, true, BO.CourierFieldSort.Id);

            s_bl.Courier.AddObserver(courierListObserver);
        }
             

        public IEnumerable<BO.CourierInList> CourierInList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierInList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));
    }
}
