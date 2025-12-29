using BO;
using System;
using System.Collections.Generic;
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

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierWindow.xaml
/// </summary>
public partial class CourierWindow : Window
{
    public CourierWindow(int ID)
    {
        InitializeComponent();
    }

    public BO.Courier Courier
    {
        get { return (BO.Courier)GetValue(CourierProperty); }
        set { SetValue(CourierProperty, value); }
    }

    public static readonly DependencyProperty CourierProperty =
        DependencyProperty.Register(nameof(Courier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Load courier data when the window is loaded
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        // Handle any cleanup when the window is closed
    }

    private void UpdateCourierDetails()
    {
        // Update the courier details displayed in the window

        if (Courier != null)
        {
            CourierInList = s_bl.Courier.GetCourierInList(Courier.ID);
        }
    }
}
