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

namespace PL;

/// <summary>
/// Interaction logic for EndDeliverySelectionWindow.xaml
/// </summary>
public partial class CloseDeliveryWindow : Window
{
    /// <summary>
    /// Gets the list of available end delivery options from the enum
    /// </summary>
    public List<BO.EndDelivery> EndDeliveryOptions { get; }

    /// <summary>
    /// Gets or sets the selected end delivery reason
    /// </summary>
    public BO.EndDelivery? SelectedEndDelivery { get; set; }

    /// <summary>
    /// Gets whether the user confirmed the selection
    /// </summary>
    public bool IsConfirmed { get; private set; } = false;

    public CloseDeliveryWindow()
    {
        InitializeComponent();

        this.DataContext = this;

        // מילוי הרשימה מה-Enum (ללא CANCELLED אם רוצים להגביל אפשרויות)
        EndDeliveryOptions = System.Enum.GetValues(typeof(BO.EndDelivery))
                                   .Cast<BO.EndDelivery>()
                                   .Where(e => e != BO.EndDelivery.CANCELLED)
                                   .ToList();

        SelectedEndDelivery = EndDeliveryOptions.FirstOrDefault();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEndDelivery == null)
        {
            MessageBox.Show("יש לבחור סיבת סיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsConfirmed = true;
        DialogResult = true;
        this.Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        DialogResult = false;
        this.Close();
    }
}