using BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier;

public partial class CourierWindow : Window, System.ComponentModel.INotifyPropertyChanged
{
    // INotifyPropertyChanged implementation בשביל כפתור המחיקה שיופיע רק במצב עדכון
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // 1. גישה לשכבת ה-BL
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // נניח שזה ה-ID של המנהל המחובר כרגע (תצטרך להחליף זאת בלוגיקה האמיתית שלך)
    private readonly int CURRENT_MANAGER_ID = s_bl.Admin.GetConfig().ManagerId!;

    private int CURRENT_ID = 0;
    public bool IsUpdateMode => ButtonText == "Update";

    private bool _isDeleting = false; // דגל שמורה אם אנו בתוך מחיקה

    public BO.Courier CurrentCourier
    {
        get => (BO.Courier)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }

    // האובייקט אותו אנו עורכים או מוסיפים
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier),
            typeof(CourierWindow), new PropertyMetadata(new BO.Courier()
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
       DependencyProperty.Register(nameof(ButtonText), typeof(string),
           typeof(CourierWindow), new PropertyMetadata("Add"));

    // נראות כפתור המחיקה
    private Visibility _deleteButtonVisibility = Visibility.Collapsed;
    public Visibility DeleteButtonVisibility
    {
        get => _deleteButtonVisibility;
        set
        {
            _deleteButtonVisibility = value;
            OnPropertyChanged(nameof(DeleteButtonVisibility));
        }
    }

    // רשימה ל-ComboBox של סוגי רכב
    public IEnumerable<BO.TheTypeShipment> VehicleTypesList { get; } =
     Enum.GetValues(typeof(BO.TheTypeShipment)).Cast<BO.TheTypeShipment>();


    // בנאי
    public CourierWindow(int id = 0)
    {
        InitializeComponent();
        CURRENT_ID = id;
    }

    private void CourierWindow_Loaded(object sender, EventArgs e)
    {
        if (CURRENT_ID != 0) // מצב עדכון
        {
            ButtonText = "Update";
            DeleteButtonVisibility = Visibility.Visible;

            try
            {
                // שליפת השליח מה-BL
                CourierObserver();
                s_bl.Courier.AddObserver(CourierObserver);
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
            DeleteButtonVisibility = Visibility.Collapsed;

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

    // מחיקת שליח
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "ביקשת למחוק שליח?",
            "אישור מחיקה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            //הסרת המשקיף לפני המחיקה אחרת הוא שוב ניגש אל השליח המחוק וזורק לי חריגה. 
            // s_bl.Courier.RemoveObserver(CourierObserver);
            _isDeleting = true; // דגל שמורה שהמחיקה בעבודה כדי שהמשקיף לא ינסה לגשת לנתונים המחוקים ויזרוק לי חרגיה
            s_bl.Courier.Delete(CURRENT_MANAGER_ID, CURRENT_ID);
            MessageBox.Show("השליח נמחק בהצלחה");
            Close(); // מומלץ לסגור את החלון
        }
        catch (BO.BlDoesNotExistException)
        {//לכאורה בלתי אפשרי כי מחיקה מתרחשת מתוך שליח קיים
            MessageBox.Show("שליח לא נמצא למחיקה");
        }
        catch (BO.BlInvalidOperationException)
        {
            MessageBox.Show("לא ניתן למחוק שליח עם הזמנה בתהליך");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה במחיקה: {ex.Message}");
        }
    }




    private void CourierWindow_Closed(object sender, EventArgs e)
    {
        if (CURRENT_ID != 0)
            s_bl.Courier.RemoveObserver(CourierObserver);
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
            if (sender is Button button && button.Content is "Add")
            {
                s_bl.Courier.Create(CURRENT_MANAGER_ID, CurrentCourier);
                MessageBox.Show("השליח נוסף בהצלחה!");
            }
            else
            {
                s_bl.Courier.Update(CURRENT_MANAGER_ID, CurrentCourier);
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

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            CurrentCourier.Password = passwordBox.Password;
        }
    }

    private void CourierObserver()
    {
        if (_isDeleting)
            return; // אם אנו בתוך מחיקה – לא עושים כלום

        CurrentCourier = s_bl.Courier.Read(CURRENT_MANAGER_ID, CURRENT_ID)
            ?? throw new BO.BlDoesNotExistException($"The Courier with id: {CURRENT_ID} is not exist: ");

       

    }   

    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Allow only digits
        e.Handled = !e.Text.All(char.IsDigit);
    }
}




