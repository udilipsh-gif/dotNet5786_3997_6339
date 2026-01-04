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

namespace PL;

/// <summary>
/// Interaction logic for StartDeliveryWindow.xaml
/// </summary>
public partial class StartDeliveryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int userId;

    private readonly int courierId;

    private BO.TypeOfOrder filter;

    private BO.OpenOrderInListField sort;

    public BO.TypeOfOrder SelctedFilter
    {
        get => (BO.TypeOfOrder)GetValue(SelctedFilterProperty);
        set => SetValue(SelctedFilterProperty, value);
    }
    public static readonly DependencyProperty SelctedFilterProperty =
        DependencyProperty.Register("SelctedFilter", typeof(BO.TypeOfOrder),
            typeof(StartDeliveryWindow), new PropertyMetadata(BO.TypeOfOrder.All));

    public StartDeliveryWindow(int userId, int courierId)
    {
        this.userId = userId;

        this.courierId = courierId;

        InitializeComponent();
    }

    private void StartDeliveryWindow_Loaded(object sender, RoutedEventArgs e)
    {
       DeliveryListView = s_bl.Order.GetOpen(
    }

    private void StartDeliveryWindow_Closed(object sender, EventArgs e)
    {
        Application.Current.MainWindow!.Show();
    }

}
