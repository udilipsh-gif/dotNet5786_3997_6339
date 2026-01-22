using PL.Helpers;
using System.Threading.Tasks;
using System.Windows;


namespace PL;

/// <summary>
/// Interaction logic for ConfigWindow.xaml
/// </summary>
public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
    }

    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Gets or sets the current system configuration.
    /// </summary>
    /// <remarks>
    /// This property is bound to the configuration editing UI controls.
    /// Changes to configuration fields are tracked and users are prompted to save before losing focus.
    /// </remarks>
    public BO.Config Configuration
    {
        get => (BO.Config)GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value); 
    }

    /// <summary>
    /// Dependency property for the Configuration property.
    /// </summary>
    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(ConfigWindow));


  

    private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Tools.ResetRequested += () => this.Close();
        Tools.RunSafe(() => s_bl.Admin.AddConfigObserver(ConfigObserver));
        ConfigObserver();
    }

    private void ConfigWindow_Close(object? sender, EventArgs e)
    {
        Tools.ResetRequested -= () => this.Close();
        Tools.RunSafe(() => s_bl.Admin.RemoveConfigObserver(ConfigObserver));
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        this.Close();
    }

    private async void BtuSave_Click(object? sender, EventArgs e)
    {
        try
        {
            await s_bl.Admin.SetConfig(Configuration);
            MessageBox.Show(
            "הנתונים נשמרו בהצלחה",
            "שמירה",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
            ex.Message,
            "שגיאה",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        }
        
    }

    private readonly ObserverMutex _Mutex = new(); //stage 7

    private void ConfigObserver() => Tools.RunSafe(() => {
        if (_Mutex.CheckAndSetLoadInProgressOrRestartRequired())//הדלקת פלאג בפונקציה שמציינת שהריצה בעיצומה ואם מישהו ביקש ריסטארט בזמן הזה
            return;

        Dispatcher.BeginInvoke(async () =>
        {


            Configuration = s_bl.Admin.GetConfig();
            if (await _Mutex.UnsetLoadInProgressAndCheckRestartRequested())//אם מישהו ביקש ריסטארט בזמן שהריצה הייתה בעיצומה
                ConfigObserver();
        });



    });

}