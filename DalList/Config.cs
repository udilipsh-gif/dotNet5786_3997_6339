///צריך להוסיף כאן בדיקות תקינות

namespace Dal;

internal static class Config
{
    internal const int StartOrderId = 100001;
    private static int order_id = StartOrderId;
    internal static int NextOrderId { get => order_id++; }

    internal const int StartDeliveryId = 200001;
    private static int delivery_id = StartDeliveryId;
    internal static int NextDeliveryId { get => delivery_id++; }

    internal static DateTime Clock { get; set; } = DateTime.Now;


    internal static int StartManagerId = 100000000;
    private static int manager_id = StartOrderId;
    internal static int ManagerId
    {
        get => manager_id;
        set
        {
            if (!s_isValidIsraeliId(value))
                throw new ArgumentException("תעודת זהות אינה תקינה");
            manager_id = value;
        }
    }

    private static bool s_isValidIsraeliId(int id)
    {
        string idStr = id.ToString().PadLeft(9, '0');
        if (!System.Text.RegularExpressions.Regex.IsMatch(idStr, @"^\d{9}$"))
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = idStr[i] - '0';
            int incNum = digit * ((i % 2) + 1);
            sum += (incNum > 9) ? incNum - 9 : incNum;
        }

        return sum % 10 == 0;
    }

    internal static string PasswordManager { get; set; } = "Admin1234$";
    internal static string? StoreAddress {  get; set; } = null;

    internal static double? Latitude { get; set; } = null;
    internal static double? Longitude { get; set; } = null;
    internal static double? MaxDeliveryRange { get; set; } = null;
    internal static double AvgSpeedCar { get; set; } =00.0;
    internal static double AvgSpeedMotorcycle { get; set; } = 00.0;
    internal static double AvgSpeedBike { get; set; } = 00.0;
    internal static double AvgSpeedFoot { get; set; } = 00.0;
    internal static TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromDays(0);
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromDays(0);
    internal static TimeSpan MaxTimeInactivity { get; set; } = TimeSpan.FromDays(0);
    internal static void Reset()
    {
        order_id = StartOrderId;
        delivery_id = StartDeliveryId;
        Clock = DateTime.Now;
        manager_id= StartManagerId;
        PasswordManager= "Admin1234$";
        StoreAddress= null;
        Latitude= null;
        Longitude = null;
        MaxDeliveryRange= null;
        AvgSpeedCar= 00.0;
        AvgSpeedMotorcycle= 00.0;
        AvgSpeedBike= 00.0;
        AvgSpeedFoot= 00.0;
        MaxDeliveryTime= TimeSpan.FromDays(0);
        RiskRange= TimeSpan.FromDays(0);
        MaxTimeInactivity= TimeSpan.FromDays(0);    
    }
}
