using DO;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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
    /// 
    internal static int NextOrderId {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => s_orderId++; 
    }

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
    internal static int NextDeliveryId {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => delivery_id++; 
    }

    /// <summary>
    /// Gets or sets the system clock time.
    /// </summary>
    internal static DateTime Clock {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = DateTime.Now;

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
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => manager_id;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => manager_id = value;
    }

    /// <summary>
    /// Gets or sets the manager's password.
    /// </summary>
    internal static string PasswordManager {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = "1";

    /// <summary>
    /// Gets or sets the store address.
    /// </summary>
    internal static string? StoreAddress {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = null;

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// </summary>
    internal static double? Latitude {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = null;

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// </summary>
    internal static double? Longitude {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = null;

    /// <summary>
    /// Gets or sets the maximum delivery range in distance units.
    /// </summary>
    internal static double? MaxDeliveryRange {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = null;

    /// <summary>
    /// Gets or sets the average speed for car deliveries.
    /// </summary>
    internal static double AvgSpeedCar {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries.
    /// </summary>
    internal static double AvgSpeedMotorcycle {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for bike deliveries.
    /// </summary>
    internal static double AvgSpeedBike {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for foot deliveries.
    /// </summary>
    internal static double AvgSpeedFoot {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = 00.0;

    /// <summary>
    /// Gets or sets the maximum time allowed for a delivery.
    /// </summary>
    internal static TimeSpan MaxDeliveryTime {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// </summary>
    internal static TimeSpan RiskRange {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed.
    /// </summary>
    internal static TimeSpan MaxTimeInactivity {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = TimeSpan.FromDays(0);
    /// <summary>
    ///  gets or sets the Google API key for accessing Google services.
    /// </summary>
    internal static string GoogleApiKey {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = string.Empty;
    /// <summary>
    /// gets or sets the email address used for system notifications.
    /// </summary>
    internal static string EmailAddress {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = string.Empty;
    /// <summary>
    /// gets or sets the URL of the script used for mail notifications.
    /// </summary>
    internal static string ScriptUrl {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = string.Empty;
    /// <summary>
    /// gets or sets the password for accessing the script.
    /// </summary>

    internal static string ScriptPass {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = string.Empty;
    /// <summary>
    /// gets or sets the token used for SMS  & call notifications.
    /// </summary>

    internal static string TokenCallSms {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set;
    } = string.Empty;

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        s_orderId = StartOrderId;
        delivery_id = StartDeliveryId;
        Clock = DateTime.Now;
        manager_id = StartManagerId;
        StoreAddress = "בר כוכבא 21 בני ברק";
        Latitude = 32.0936195;
        Longitude = 34.8229463;
        MaxDeliveryRange = 25;
        PasswordManager = "1";
        AvgSpeedCar = 30;
        AvgSpeedMotorcycle = 50;
        AvgSpeedBike = 15;
        AvgSpeedFoot = 5;
        MaxDeliveryTime = TimeSpan.FromDays(5);
        RiskRange = TimeSpan.FromDays(1);
        MaxTimeInactivity = TimeSpan.FromDays(100);
        GoogleApiKey = "AIzaSyA-LjTOw9o47TICCR-4zQDUQoQYp1tSzGk";
        EmailAddress = "ddavidov@g.jct.ac.il";
        ScriptUrl = "AKfycbyIlSh0oe9uFaJX9XpBx0U_IJ70uYaLd_7jZyLvnYE1D94oA47HelVQdO00PzIOqrvNIg";
        ScriptPass = "sdfjsak8796978akljdf54gdfgr44";
        TokenCallSms = "WU1BUElL.apik_j2szxGbBK7FSC2mHXJ1MBQ.f3eQocFPKykDUClg3fsSXpuvpodW5qgiZKAlpST5a8c";

    }
}