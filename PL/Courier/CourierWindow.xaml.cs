using BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PL;

/// <summary>
/// Interaction logic for CourierWindow.xaml - provides CRUD operations for courier management.
/// </summary>
/// <remarks>
/// This window allows managers to add, view, edit, and delete courier information.
/// Couriers can also edit their own information through this window.
/// Implements INotifyPropertyChanged for dynamic UI updates.
/// </remarks>
public partial class CourierWindow : Window
{
    /// <summary>
    /// Business logic layer instance for accessing courier operations.
    /// </summary>
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// The ID of the currently logged-in manager.
    /// </summary>
    private readonly int CURRENT_MANAGER_ID = Tools.GetSafeFromBl(() => s_bl.Admin.GetConfig().ManagerId);


    /// <summary>
    /// The ID of the courier being viewed or edited. 0 indicates add mode.
    /// </summary>
    private int CURRENT_ID = 0;

    /// <summary>
    /// Gets whether the window is in update mode (as opposed to add mode).
    /// </summary>
    public bool IsUpdateMode => ButtonText == "Update";

    /// <summary>
    /// Gets or sets the current courier being displayed or edited.
    /// </summary>
    public BO.Courier CurrentCourier
    {
        get => (BO.Courier)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }

    /// <summary>
    /// Dependency property for the CurrentCourier object.
    /// </summary>
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
                WorkingSince = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock()),
                DeliveryLate = 0,
                DeliveryOnTime = 0,


            }));

    /// <summary>
    /// Gets or sets the text displayed on the action button ("Add" or "Update").
    /// </summary>
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    /// <summary>
    /// Dependency property for the ButtonText property.
    /// </summary>
    public static readonly DependencyProperty ButtonTextProperty =
       DependencyProperty.Register(nameof(ButtonText), typeof(string),
           typeof(CourierWindow), new PropertyMetadata("Add"));

    /// <summary>
    /// List of all vehicle/shipment types for the ComboBox.
    /// </summary>
    public IEnumerable<BO.TheTypeShipment> VehicleTypesList { get; } =
     Enum.GetValues(typeof(BO.TheTypeShipment)).Cast<BO.TheTypeShipment>();


    /// <summary>
    /// Initializes a new instance of the CourierWindow class.
    /// </summary>
    /// <param name="id">The ID of the courier to edit, or 0 to add a new courier.</param>
    public CourierWindow(int id = 0)
    {
        InitializeComponent();
        CURRENT_ID = id;
    }

    /// <summary>
    /// Handles the window loaded event, initializes the window mode (add or update).
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void CourierWindow_Loaded(object sender, EventArgs e)
    {
        if (CURRENT_ID != 0)
        {
            ButtonText = "Update";

            try
            {
                CourierObserver();
                s_bl.Courier.AddObserver(CURRENT_ID, CourierObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הנתונים: {ex.Message}");
                Close();
            }
        }
        else
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
                WorkingSince = Tools.GetSafeFromBl(() => s_bl.Admin.GetClock()),
                DeliveryLate = 0,
                DeliveryOnTime = 0

            };

        }


    }

    /// <summary>
    /// Handles the delete button click event, prompts for confirmation and deletes the courier.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Prompts the user for confirmation before deletion.
    /// Cannot delete couriers with active orders in progress.
    /// </remarks>
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "האם אתה רוצה למחוק את השליח הזה?",
            "אישור מחיקה",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            s_bl.Courier.Delete(CURRENT_MANAGER_ID, CURRENT_ID);
            MessageBox.Show("השליח נמחק בהצלחה");

            Close();
        }
        catch (BO.BlDoesNotExistException)
        {
            MessageBox.Show($"שליח לא נמצא למחיקה");
        }
        catch (BO.BlInvalidOperationException ex)
        {
            MessageBox.Show($"לא ניתן למחוק מסיבת: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה במחיקה: {ex.Message}");
        }
    }




    /// <summary>
    /// Handles the window closed event, removes observers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void CourierWindow_Closed(object sender, EventArgs e)
    {
        if (CURRENT_ID != 0)
            s_bl.Courier.RemoveObserver(CURRENT_ID, CourierObserver);
    }

    /// <summary>
    /// Handles the add/update button click event, validates and saves courier data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Performs basic validation before submitting data to the business logic layer.
    /// Creates a new courier if in add mode, updates existing courier if in update mode.
    /// Closes the window upon successful operation.
    /// </remarks>
    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier == null || string.IsNullOrEmpty(CurrentCourier.Name))
        {
            MessageBox.Show("Please enter a name");
            return;
        }

        try
        {
            if (sender is Button button && button.Tag.ToString() is "Add")
            {
                s_bl.Courier.Create(CURRENT_MANAGER_ID, CurrentCourier);
                MessageBox.Show("Courier added successfully!");
            }
            else
            {
                s_bl.Courier.Update(CURRENT_MANAGER_ID, CurrentCourier);
                MessageBox.Show("Details updated successfully!");
            }

            this.Close();
        }
        catch (BO.BlInvalidValueException ex)
        {
            MessageBox.Show($"Invalid data: {ex.Message}");
        }
        catch (BO.BlAlreadyExistsException ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"General error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the password changed event for the PasswordBox control.
    /// </summary>
    /// <param name="sender">The PasswordBox that triggered the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// Updates the CurrentCourier's password as the user types.
    /// This is necessary because PasswordBox.Password is not a dependency property and cannot be bound directly.
    /// </remarks>
    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            CurrentCourier.Password = passwordBox.Password;
        }
    }

    /// <summary>
    /// Observer method that updates the courier data when changes occur in the business layer.
    /// </summary>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier no longer exists.</exception>
    /// <remarks>
    /// This method is called whenever the business layer notifies of changes to the courier.
    /// It is skipped if a deletion operation is in progress to avoid accessing deleted data.
    /// </remarks>
    private  async void CourierObserver()
    {
        try
        {
            CurrentCourier = await s_bl.Courier.Read(CURRENT_MANAGER_ID, CURRENT_ID)
                        ?? throw new BO.BlDoesNotExistException($"The Courier with id: {CURRENT_ID} does not exist");
        }
        catch (BO.BlDoesNotExistException)
        {
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }




    }

    /// <summary>
    /// Handles text input validation for numeric-only fields.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing the text input.</param>
    /// <remarks>
    /// Only allows digit characters to be entered.
    /// Used for ID and other numeric fields to prevent invalid input.
    /// </remarks>
    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        => e.Handled = !e.Text.All(char.IsDigit);

}




