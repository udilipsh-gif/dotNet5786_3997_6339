using BO;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PL.Courier;

public partial class CourierWindow : Window
{
    // 1. גישה לשכבת ה-BL
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // נניח שזה ה-ID של המנהל המחובר כרגע (תצטרך להחליף זאת בלוגיקה האמיתית שלך)
    private int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId;

    public bool IsUpdateMode => ButtonText == "Update";

    public BO.Courier? CurrentCourier
    {
        get => (BO.Courier)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }

    // האובייקט אותו אנו עורכים או מוסיפים
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(new BO.Courier()
        {
            Id = 0,
            Name = "",
            Phone = "",
            Email = "",
            Password = "",
            Active = true,
            MaxDistanceDelivery = 0,
            TypeShipment = TheTypeShipment.CAR,
            WorkingSince = s_bl.Admin.GetClock(),
            DeliveryLate = 0,
            DeliveryOnTime = 0

        }));

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }
    //טקסט של הכפתור (הוספה/עדכון)
    public static readonly DependencyProperty ButtonTextProperty =
       DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(CourierWindow), new PropertyMetadata("Add"));

    // רשימה ל-ComboBox של סוגי רכב
    public IEnumerable<BO.TheTypeShipment> VehicleTypesList { get; } =
     Enum.GetValues(typeof(BO.TheTypeShipment)).Cast<BO.TheTypeShipment>();


    // בנאי
    public CourierWindow(int id = 0)
    {
        InitializeComponent();
        DataContext = this;


        if (id != 0) // מצב עדכון
        {
            ButtonText = "Update";
            
            try
            {
                // שליפת השליח מה-BL
                CurrentCourier = s_bl.Courier.Read(CURRENT_MANAGER_ID, id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
                Close();
            }
        }
        else // מצב הוספה
        {
            ButtonText = "Add";
            CurrentCourier = new BO.Courier()
            {
                Id = 0,
                Name = "",
                Phone = "",
                Email = "",
                Password = "",
                Active = true,
                MaxDistanceDelivery = 0,
                TypeShipment = TheTypeShipment.CAR,
                WorkingSince = s_bl.Admin.GetClock(),
                DeliveryLate = 0,
                DeliveryOnTime = 0

            };

        }

     
    }

    
    private void CourierWindow_Closed(object sender, EventArgs e)
    {
        // ניקוי אם צריך
    }

    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        // ולידציה בסיסית לפני שליחה (אופציונלי)
        if (CurrentCourier == null || string.IsNullOrEmpty(CurrentCourier.Name))
        {
            MessageBox.Show("נא להזין שם");
            return;
        }

        try
        {
            if (ButtonText == "Add")
            {
                AddCourier();
                MessageBox.Show("השליח נוסף בהצלחה!");
            }
            else
            {
                UpdateCourier();
                MessageBox.Show("הפרטים עודכנו בהצלחה!");
            }

            // סגירת החלון לאחר הצלחה
            this.Close();
        }
        catch (BO.BlInvalidValueException ex)
        {
            MessageBox.Show($"נתונים לא תקינים: {ex.Message}");
        }
        catch (BO.BlAlreadyExistsException ex)
        {
            MessageBox.Show($"שגיאה: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה כללית: {ex.Message}");
        }
    }

    private void AddCourier()
    {
        // קריאה לפונקציית Create ב-BL
        // ה-CurrentCourier מתעדכן אוטומטית מהמסך בזכות ה-Binding
        //s_bl.Courier.Create(CURRENT_MANAGER_ID, CurrentCourier);
    }

    private void UpdateCourier()
    {
        // קריאה לפונקציית Update ב-BL
       // s_bl.Courier.Update(CURRENT_MANAGER_ID, CurrentCourier);
    }

    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Allow only digits
        e.Handled = !e.Text.All(char.IsDigit);
    }
}



//using BO;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

//namespace PL.Courier;

///// <summary>
///// Interaction logic for CourierWindow.xaml
///// </summary>
//public partial class CourierWindow : Window
//{
//    private static readonly BlApi.IBl bl = BlApi.Factory.Get();
//    // Property תלות עבור הטקסט של הכפתור
//    public static readonly DependencyProperty ButtonTextProperty =
//        DependencyProperty.Register(
//            nameof(ButtonText),
//            typeof(string),
//            typeof(CourierWindow),
//            new PropertyMetadata("Add"));

//    public string ButtonText
//    {
//        get => (string)GetValue(ButtonTextProperty);
//        set => SetValue(ButtonTextProperty, value);
//    }

//    public CourierWindow(int id)
//    {
//        InitializeComponent();

//        if (id != 0) // עדכון
//        {

//            ButtonText = "Update";
//        }
//        else
//        {

//            ButtonText = "Add";
//        }

//        this.DataContext = this; 
//    }

//    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
//    {
//        if (ButtonText == "Add")
//        {
//            AddCourier();
//        }
//        else
//        {
//            UpdateCourier();
//        }
//    }

//    private void AddCourier()
//    {

//    }

//    private void UpdateCourier()
//    {
//        // כאן הקוד לעדכון ישות קיימת
//    }


    //public BO.Courier Courier
    //{
    //    get { return (BO.Courier)GetValue(CourierProperty); }
    //    set { SetValue(CourierProperty, value); }
    //}

    //public static readonly DependencyProperty CourierProperty =
    //    DependencyProperty.Register(nameof(Courier), typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));


//    private void Window_Loaded(object sender, RoutedEventArgs e)
//    {
//        // Load courier data when the window is loaded
//    }

//    private void Window_Closed(object sender, EventArgs e)
//    {
//        // Handle any cleanup when the window is closed
//    }

//    private void UpdateCourierDetails()
//    {
//        // Update the courier details displayed in the window

//        if (Courier != null)
//        {
//            CourierInList = s_bl.Courier.GetCourierInList(Courier.ID);
//        }
//    }
//}
