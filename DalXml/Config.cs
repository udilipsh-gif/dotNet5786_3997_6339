namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliverys_xml = "deliverys.xml";

    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int ManagerId
    {
        get => XMLTools.GetConfigGenericVal<int>(s_data_config_xml, "ManagerId");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "ManagerId", value);
    }
    internal static string PasswordManager
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "PasswordManager");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "PasswordManager", value);
    }


    internal static string StoreAddress
    {
        get => XMLTools.GetConfigGenericVal<string>(s_data_config_xml, "StoreAddress");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "StoreAddress", value);
    }

    internal static double Latitude
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Latitude", value);
    }

    internal static double Longitude
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "Longitude", value);
    }

    internal static double MaxDeliveryRange
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "MaxDeliveryRange");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryRange", value);
    }

    internal static double AvgSpeedCar
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedCar");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedCar", value);
    }

    internal static double AvgSpeedMotorcycle
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedMotorcycle");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedMotorcycle", value);
    }

    internal static double AvgSpeedBike
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedBike");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedBike", value);
    }

    internal static double AvgSpeedFoot
    {
        get => XMLTools.GetConfigGenericVal<double>(s_data_config_xml, "AvgSpeedFoot");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "AvgSpeedFoot", value);
    }

    internal static TimeSpan MaxDeliveryTime
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxDeliveryTime");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxDeliveryTime", value);
    }

    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "RiskRange", value);
    }

    internal static TimeSpan MaxTimeInactivity
    {
        get => XMLTools.GetConfigGenericVal<TimeSpan>(s_data_config_xml, "MaxTimeInactivity");
        set => XMLTools.SetConfigGenericVal(s_data_config_xml, "MaxTimeInactivity", value);
    }




    internal static void Reset()
    {
        NextOrderId = 100001;
        NextDeliveryId = 200001;
        PasswordManager = "Admin1234$";
        Clock = DateTime.Now;
        ManagerId = 344045810;
        StoreAddress = "בר כוכבא 21 בני ברק";
        Latitude = 32.0936195;
        Longitude = 34.8229463;
        MaxDeliveryRange = 20;
        AvgSpeedCar = 30;
        AvgSpeedMotorcycle = 50;
        AvgSpeedBike = 20;
        AvgSpeedFoot = 5;
        MaxDeliveryTime = TimeSpan.FromHours(2);
        RiskRange = TimeSpan.FromMinutes(30);
        MaxTimeInactivity = TimeSpan.FromMinutes(15);
    }
}
