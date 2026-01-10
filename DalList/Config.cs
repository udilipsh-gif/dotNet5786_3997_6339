using DO;

namespace Dal;

/// <summary>
/// Internal configuration class for managing system-wide settings, IDs, and constants.
/// </summary>
internal static class Config
{
    /// <summary>
    /// The starting ID value for orders.
    /// </summary>
    internal const int StartOrderId = 100001;

    /// <summary>
    /// Private field for tracking the current order ID.
    /// </summary>
    private static int s_orderId = StartOrderId;

    /// <summary>
    /// Gets the next available order ID and increments the counter.
    /// </summary>
    internal static int NextOrderId { get => s_orderId++; }

    /// <summary>
    /// The starting ID value for deliveries.
    /// </summary>
    internal const int StartDeliveryId = 200001;

    /// <summary>
    /// Private field for tracking the current delivery ID.
    /// </summary>
    private static int delivery_id = StartDeliveryId;

    /// <summary>
    /// Gets the next available delivery ID and increments the counter.
    /// </summary>
    internal static int NextDeliveryId { get => delivery_id++; }

    /// <summary>
    /// Gets or sets the system clock time.
    /// </summary>
    internal static DateTime Clock { get; set; } = DateTime.Now;

    /// <summary>
    /// The starting ID value for managers.
    /// </summary>
    internal static int StartManagerId = 1;

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
                throw new DalValueIsNotValid(value.ToString());
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
    internal static string PasswordManager { get; set; } = "1";

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

    internal static string GoogleApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    internal static void Reset()
    {
        s_orderId = StartOrderId;
        delivery_id = StartDeliveryId;
        Clock = DateTime.Now;
        manager_id = StartManagerId;
        StoreAddress = "בר כוכבא 21 בני ברק";
        Latitude = 32.0936195;
        Longitude = 34.8229463;
        MaxDeliveryRange = 20;
        PasswordManager = "1";
        AvgSpeedCar = 30;
        AvgSpeedMotorcycle = 50;
        AvgSpeedBike = 15;
        AvgSpeedFoot = 5;
        MaxDeliveryTime = TimeSpan.FromDays(5);
        RiskRange = TimeSpan.FromDays(1);
        MaxTimeInactivity = TimeSpan.FromDays(100);
        GoogleApiKey = "AIzaSyA-LjTOw9o47TICCR-4zQDUQoQYp1tSzGk";
    }
}