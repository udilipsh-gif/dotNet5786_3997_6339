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

            this.Loaded += MainWindow_Loaded;

            this.Closed += MainWindow_Close;


        }

        private void MainWindow_Close(object? sender, EventArgs e)
        {
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
 
        }

        public IEnumerable<BO.CourierInList> CourierList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierInList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));
    }
}
