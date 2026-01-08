using System.Windows;

namespace PL;

/// <summary>
/// Interaction logic for EndDeliverySelectionWindow.xaml
/// </summary>
public partial class CloseDeliveryWindow : Window
{
    /// <summary>
    /// Gets the list of available end delivery options from the enum
    /// </summary>
    public List<BO.EndDelivery> EndDeliveryOptions
    { 
        get { return (List<BO.EndDelivery>)GetValue(EndDeliveryOptionsProperty); }
        private set { SetValue(EndDeliveryOptionsProperty, value); }
    }

    public static readonly DependencyProperty EndDeliveryOptionsProperty =
        DependencyProperty.Register("EndDeliveryOptions", typeof(List<BO.EndDelivery>),
            typeof(CloseDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the selected end delivery reason
    /// </summary>
    public BO.EndDelivery? SelectedEndDelivery 
    {
        get { return (BO.EndDelivery?)GetValue(SelectedEndDeliveryProperty); }
        set { SetValue(SelectedEndDeliveryProperty, value); }
    }

    public static readonly DependencyProperty SelectedEndDeliveryProperty =
        DependencyProperty.Register("SelectedEndDelivery", typeof(BO.EndDelivery?),
            typeof(CloseDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets whether the user confirmed the selection
    /// </summary>
    public bool IsConfirmed { get; private set; } = false;

    public CloseDeliveryWindow()
    {
        this.DataContext = this;

        // מילוי הרשימה מה-Enum (ללא CANCELLED אם רוצים להגביל אפשרויות)
        EndDeliveryOptions = System.Enum.GetValues(typeof(BO.EndDelivery))
                                   .Cast<BO.EndDelivery>()
                                   .Where(e => e != BO.EndDelivery.CANCELLED)
                                   .ToList();

        SelectedEndDelivery = null;

        InitializeComponent();
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