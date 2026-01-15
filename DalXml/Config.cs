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
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    /// <summary>
    /// Gets the next available delivery ID and automatically increments the counter.
    /// This property is used to generate unique IDs for new deliveries.
    /// </summary>
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    /// <summary>
    /// Gets or sets the current system clock time.
    /// Used for time-based operations and simulations.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the manager's unique identifier.
    /// Used for authentication and authorization purposes.
    /// </summary>
    internal static int ManagerId
    {
        get => XMLTools.GetConfigGenericVal<int>(s_data_config_xml, "ManagerId");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "ManagerId", value);
    }

    /// <summary>
    /// Gets or sets the manager's password.
    /// Used for authentication purposes.
    /// </summary>
    internal static string PasswordManager
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "PasswordManager");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "PasswordManager", value);
    }

    /// <summary>
    /// Gets or sets the physical address of the store.
    /// </summary>
    internal static string StoreAddress
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "StoreAddress");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "StoreAddress", value);
    }

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// Used for distance calculations and mapping.
    /// </summary>
    internal static double Latitude
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Latitude", value);
    }

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// Used for distance calculations and mapping.
    /// </summary>
    internal static double Longitude
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Longitude", value);
    }

    /// <summary>
    /// Gets or sets the maximum delivery range in kilometers.
    /// Deliveries beyond this range are not accepted.
    /// </summary>
    internal static double MaxDeliveryRange
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "MaxDeliveryRange");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryRange", value);
    }

    /// <summary>
    /// Gets or sets the average speed for car deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedCar
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedCar");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedCar", value);
    }

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedMotorcycle
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedMotorcycle");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedMotorcycle", value);
    }

    /// <summary>
    /// Gets or sets the average speed for bike deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedBike
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedBike");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedBike", value);
    }

    /// <summary>
    /// Gets or sets the average speed for foot deliveries in km/h.
    /// </summary>
    internal static double AvgSpeedFoot
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedFoot");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedFoot", value);
    }

    /// <summary>
    /// Gets or sets the maximum time allowed for completing a delivery.
    /// </summary>
    internal static TimeSpan MaxDeliveryTime
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxDeliveryTime");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryTime", value);
    }

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// Used to flag deliveries that might not meet the deadline.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "RiskRange", value);
    }

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed before a courier is flagged.
    /// Used to monitor courier availability and responsiveness.
    /// </summary>
    internal static TimeSpan MaxTimeInactivity
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxTimeInactivity");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxTimeInactivity", value);
    }

    internal static string GoogleApiKey
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "GoogleApiKey");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "GoogleApiKey", value);
    }

    internal static string EmailAddress
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "EmailAddress");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "EmailAddress", value);
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
    }
}
