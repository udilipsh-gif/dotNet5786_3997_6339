using System.Runtime.CompilerServices;

namespace Dal;

/// <summary>
/// Internal static configuration class that manages system-wide settings and auto-incrementing IDs.
/// Provides centralized access to configuration values stored in XML files.
/// </summary>
internal static class Config
{
    /// <summary>
    /// The name of the XML file containing system configuration data.
    /// </summary>
    internal const string s_data_config_xml = "data-config.xml";

    /// <summary>
    /// The name of the XML file containing courier data.
    /// </summary>
    internal const string s_couriers_xml = "couriers.xml";

    /// <summary>
    /// The name of the XML file containing order data.
    /// </summary>
    internal const string s_orders_xml = "orders.xml";

    /// <summary>
    /// The name of the XML file containing delivery data.
    /// </summary>
    internal const string s_deliverys_xml = "deliverys.xml";

    /// <summary>
    /// Gets the next available order ID and automatically increments the counter.
    /// This property is used to generate unique IDs for new orders.
    /// </summary>
    internal static int NextOrderId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    /// <summary>
    /// Gets the next available delivery ID and automatically increments the counter.
    /// This property is used to generate unique IDs for new deliveries.
    /// </summary>
    internal static int NextDeliveryId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    /// <summary>
    /// Gets or sets the current system clock time.
    /// Used for time-based operations and simulations.
    /// </summary>
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the manager's unique identifier.
    /// Used for authentication and authorization purposes.
    /// </summary>
    internal static int ManagerId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<int>(s_data_config_xml, "ManagerId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "ManagerId", value);
    }

    /// <summary>
    /// Gets or sets the manager's password.
    /// Used for authentication purposes.
    /// </summary>
    internal static string PasswordManager
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "PasswordManager");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "PasswordManager", value);
    }

    /// <summary>
    /// Gets or sets the physical address of the store.
    /// </summary>
    internal static string StoreAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "StoreAddress");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "StoreAddress", value);
    }

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// Used for distance calculations and mapping.
    /// </summary>
    internal static double Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Latitude");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Latitude", value);
    }

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// Used for distance calculations and mapping.
    /// </summary>
    internal static double Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Longitude");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Longitude", value);
    }

    /// <summary>
    /// Gets or sets the maximum delivery range in kilometers.
    /// Deliveries beyond this range are not accepted.
    /// </summary>
    internal static double MaxDeliveryRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "MaxDeliveryRange");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryRange", value);
    }

    /// <summary>
    /// Gets or sets the average speed for car deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedCar
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedCar");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedCar", value);
    }

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedMotorcycle
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedMotorcycle");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedMotorcycle", value);
    }

    /// <summary>
    /// Gets or sets the average speed for bike deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedBike
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedBike");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedBike", value);
    }

    /// <summary>
    /// Gets or sets the average speed for foot deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedFoot
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedFoot");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedFoot", value);
    }

    /// <summary>
    /// Gets or sets the maximum time allowed for completing a delivery.
    /// </summary>
    internal static TimeSpan MaxDeliveryTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxDeliveryTime");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryTime", value);
    }

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// Used to flag deliveries that might not meet the deadline.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "RiskRange");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "RiskRange", value);
    }

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed before a courier is flagged.
    /// Used to monitor courier availability and responsiveness.
    /// </summary>
    internal static TimeSpan MaxTimeInactivity
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxTimeInactivity");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxTimeInactivity", value);
    }

    internal static string GoogleApiKey
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "GoogleApiKey");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "GoogleApiKey", value);
    }

    internal static string EmailAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "EmailAddress");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "EmailAddress", value);
    }
    internal static string ScriptUrl
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "ScriptUrl");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "ScriptUrl", value);
    }
    internal static string ScriptPass
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "ScriptPass");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "ScriptPass", value);
    }
    internal static string TokenCallSms
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "TokenCallSms");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "TokenCallSms", value);
    }

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    /// <remarks>
    /// Default values:
    /// <list type="bullet">
    /// <item>NextOrderId: 100001</item>
    /// <item>NextDeliveryId: 200001</item>
    /// <item>PasswordManager: "Admin1234$"</item>
    /// <item>Clock: Current DateTime</item>
    /// <item>ManagerId: 344045810</item>
    /// <item>StoreAddress: "בר כוכבא 21 בני ברק"</item>
    /// <item>Latitude: 32.0936195</item>
    /// <item>Longitude: 34.8229463</item>
    /// <item>MaxDeliveryRange: 20 km</item>
    /// <item>AvgSpeedCar: 30 km/h</item>
    /// <item>AvgSpeedMotorcycle: 50 km/h</item>
    /// <item>AvgSpeedBike: 20 km/h</item>
    /// <item>AvgSpeedFoot: 5 km/h</item>
    /// <item>MaxDeliveryTime: 2 hours</item>
    /// <item>RiskRange: 30 minutes</item>
    /// <item>MaxTimeInactivity: 15 minutes</item>
    /// </list>
    /// </remarks>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        NextOrderId = 100001;
        NextDeliveryId = 200001;
        PasswordManager = "1";
        Clock = DateTime.Now;
        ManagerId = 1;
        StoreAddress = "בר כוכבא 21 בני ברק";
        Latitude = 32.0936195;
        Longitude = 34.8229463;
        MaxDeliveryRange = 25;
        AvgSpeedCar = 30;
        AvgSpeedMotorcycle = 50;
        AvgSpeedBike = 15;
        AvgSpeedFoot = 5;
        MaxDeliveryTime = TimeSpan.FromDays(5);
        RiskRange = TimeSpan.FromDays(1);
        MaxTimeInactivity = TimeSpan.FromDays(100);
        GoogleApiKey = "AIzaSyA-LjTOw9o47TICCR-4zQDUQoQYp1tSzGk";
        EmailAddress = "david48483@gmail.com";
        ScriptUrl = "AKfycbwzaZwF0Lv9iFC2_SWqSAgaIBUQlBDLLqJ9GlAxUS6dJCf4Sm7BqjcQy0nID1Zbip-0jQ";
        ScriptPass = "sdfjsak8796978akljdf54gdfgr44";
        TokenCallSms = "########";

    }
}
