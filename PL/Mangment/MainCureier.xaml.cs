using PL.Courier;
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
/// Interaction logic for MainCureier.xaml
/// </summary>
public partial class MainCureier : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int USERID;

    public MainCureier(int userId)
    {
        USERID = userId;

        InitializeComponent();
    }


    public BO.Courier? CurrentUser
    {
        get => (BO.Courier?)GetValue(CurrentUserProperty);
        set => SetValue(CurrentUserProperty, value);
    }

    /// <summary>
    /// Dependency property for the CurrentCourier object.
    /// </summary>
    public static readonly DependencyProperty CurrentUserProperty =
        DependencyProperty.Register("CurrentUser", typeof(BO.Courier),
            typeof(MainCureier), new PropertyMetadata(null));

    private void MainCureier_Loaded(object sender, RoutedEventArgs e)
    {
        GetCurier();
        s_bl.Courier.AddObserver(GetCurier);
    }

    private void MainCureier_Closed(object sender, EventArgs e)
    {
        if (USERID != 0)
            s_bl.Courier.RemoveObserver(GetCurier);
    }

    private void GetCurier()
    {
        try
        {
            CurrentUser = s_bl.Courier.Read(USERID, USERID)
                ?? throw new BO.BlDoesNotExistException();
        }
        catch (BO.BlDoesNotExistException)
        {
            MessageBox.Show("שליח לא קיים", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ScrollViewer_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
    {

    }
}
