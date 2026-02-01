using System.Windows;

namespace PL;

/// <summary>
/// Interaction logic for CloseDeliveryWindow.xaml.
/// A dialog window that prompts the user to select a final status for a delivery 
/// (e.g., Delivered, Refused) before finalizing the action.
/// </summary>
public partial class CloseDeliveryWindow : Window
{
    #region Properties

    /// <summary>
    /// Gets the list of available end-delivery options to be displayed in the UI.
    /// This list is populated from the BO.EndDelivery enum, excluding invalid options.
    /// </summary>
    public List<BO.EndDelivery> EndDeliveryOptions
    {
        get { return (List<BO.EndDelivery>)GetValue(EndDeliveryOptionsProperty); }
        private set { SetValue(EndDeliveryOptionsProperty, value); }
    }

    public static readonly DependencyProperty EndDeliveryOptionsProperty =
        DependencyProperty.Register(nameof(EndDeliveryOptions), typeof(List<BO.EndDelivery>),
            typeof(CloseDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the specific end-delivery reason selected by the user.
    /// Bound two-way to the ComboBox in the UI.
    /// </summary>
    public BO.EndDelivery? SelectedEndDelivery
    {
        get { return (BO.EndDelivery?)GetValue(SelectedEndDeliveryProperty); }
        set { SetValue(SelectedEndDeliveryProperty, value); }
    }

    public static readonly DependencyProperty SelectedEndDeliveryProperty =
        DependencyProperty.Register(nameof(SelectedEndDelivery), typeof(BO.EndDelivery?),
            typeof(CloseDeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Indicates whether the user successfully confirmed the selection (clicked OK).
    /// </summary>
    public bool IsConfirmed { get; private set; } = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the CloseDeliveryWindow.
    /// Populates the options list while filtering out the 'CANCELLED' status.
    /// </summary>
    public CloseDeliveryWindow()
    {
        this.DataContext = this;

        // Populate the list from the Enum, filtering out 'CANCELLED' 
        // as it is not a valid manual completion reason for the courier.
        EndDeliveryOptions = System.Enum.GetValues(typeof(BO.EndDelivery))
                                        .Cast<BO.EndDelivery>()
                                        .Where(e => e != BO.EndDelivery.CANCELLED)
                                        .ToList();

        SelectedEndDelivery = null;

        InitializeComponent();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the OK button click.
    /// Validates that a selection was made, sets the dialog result to true, and closes the window.
    /// </summary>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEndDelivery == null)
        {
            MessageBox.Show("יש לבחור סיבת סיום", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsConfirmed = true;
        DialogResult = true; // Returns true to the ShowDialog() caller
        this.Close();
    }

    /// <summary>
    /// Handles the Cancel button click.
    /// Closes the window without saving the selection.
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        DialogResult = false;
        this.Close();
    }

    /// <summary>
    /// Subscribes to the global reset event when the window is loaded.
    /// </summary>
    private void CloseDeliveryWindow_Loaded(object sender, EventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
    }

    /// <summary>
    /// Unsubscribes from the global reset event when the window is closed to prevent memory leaks.
    /// </summary>
    private void CloseDeliveryWindow_Closed(object sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
    }

    #endregion
}