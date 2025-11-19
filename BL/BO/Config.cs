

namespace BO;


public class Config
{
   
    //TO_DO: //stage 4
    //add props from DalApi.IConfig that we want to show/change in PL
    //...
     

    /// <summary>
    /// Gets or sets the system clock time.
    /// </summary>
    internal static DateTime Clock { get; set; } = DateTime.Now;

    /// <summary>
    /// The starting ID value for managers.
    /// </summary>
    internal static int StartManagerId = 100000000;

    /// <summary>
    /// Private field for tracking the current manager ID.
    /// </summary>
    private static int manager_id = StartManagerId;

    /// <summary>
    /// Gets or sets the manager ID. Validates the ID using Israeli ID validation algorithm.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the ID is not valid according to Israeli ID validation rules.</exception>
    internal static int ManagerId
    {
        get => manager_id;
        set
        {
            if (!ValidId(value))
                //throw new DalValueIsNotValid(value.ToString());################################צריך מחלקת חריגות משלי
                manager_id = value;
        }
    }

    /// <summary>
    /// Validates an Israeli ID number using the Luhn-like algorithm.
    /// </summary>
    /// <param name="id">The ID number to validate.</param>
    /// <returns>True if the ID is valid, false otherwise.</returns>

    static bool ValidId(int id)
    {
        int tempId = id;
        int sum = 0;
        tempId = tempId / 10;
        for (int i = 1; i < 9; i++)
        {
            int temp = tempId % 10;
            if (i % 2 == 0)
            {
                sum = sum + temp;
            }
            else
            {
                temp = temp * 2;
                sum = sum + (temp % 10 + temp / 10);
            }
            tempId = tempId / 10;
        }

        return (id % 10 == (10 - (sum % 10)));
    }

    /// <summary>
    /// Gets or sets the manager's password.
    /// </summary>
    internal static string PasswordManager { get; set; } = "Admin1234$";

    /// <summary>
    /// Gets or sets the store address.
    /// </summary>
    internal static string? StoreAddress { get; set; } = null;

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// </summary>
    internal static double? Latitude { get; set; } = null;

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// </summary>
    internal static double? Longitude { get; set; } = null;

    /// <summary>
    /// Gets or sets the maximum delivery range in distance units.
    /// </summary>
    internal static double? MaxDeliveryRange { get; set; } = null;

    /// <summary>
    /// Gets or sets the average speed for car deliveries.
    /// </summary>
    internal static double AvgSpeedCar { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries.
    /// </summary>
    internal static double AvgSpeedMotorcycle { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for bike deliveries.
    /// </summary>
    internal static double AvgSpeedBike { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for foot deliveries.
    /// </summary>
    internal static double AvgSpeedFoot { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the maximum time allowed for a delivery.
    /// </summary>
    internal static TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// </summary>
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed.
    /// </summary>
    internal static TimeSpan MaxTimeInactivity { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    internal static void Reset()
    {
       
        Clock = DateTime.Now;
        manager_id = StartManagerId;
        PasswordManager = "Admin1234$";
        StoreAddress = null;
        Latitude = null;
        Longitude = null;
        MaxDeliveryRange = null;
        AvgSpeedCar = 00.0;
        AvgSpeedMotorcycle = 00.0;
        AvgSpeedBike = 00.0;
        AvgSpeedFoot = 00.0;
        MaxDeliveryTime = TimeSpan.FromDays(0);
        RiskRange = TimeSpan.FromDays(0);
        MaxTimeInactivity = TimeSpan.FromDays(0);
    }


}

