
using DalApi;
using System.Text.RegularExpressions;

namespace Helpers;

internal static class Tools
{
    private static readonly IDal s_dal = Factory.Get; //stage 4

    public static string ToStringProperty<T>(this T t)
    {
        return "hi";
    }

    public static double GetDistance(double lat1, double lon1, double lat2, double lon2) //GIMINI
    {
        const double R = 6371;

        double dLat = s_toRadians(lat2 - lat1);
        double dLon = s_toRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(s_toRadians(lat1)) * Math.Cos(s_toRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    public static double GetDistance(DO.Order order) //פונקציית העמסה למרחק מהחנות להזמנה
    {
        double storeLatitude = AdminManager.GetConfig().Latitude ??
            throw new InvalidOperationException("Latitude is not set in configuration.");

        double storeLongitude = AdminManager.GetConfig().Longitude ??
            throw new InvalidOperationException("Longitude is not set in configuration.");
        
        return GetDistance(order.Latitude, order.Longitude, storeLatitude, storeLongitude);
    }

    // פונקציית עזר להמרה לרדיאנים
    private static double s_toRadians(double angleIn10thofaDegree)
    {
        return (angleIn10thofaDegree * Math.PI) / 180;
    }

    public static BO.ScheduleStatus GetScheduleStatus(DO.Order order, DO.Delivery? delivery = null)
    {
        TimeSpan riskRange = AdminManager.GetConfig()?.RiskRange ??
            throw new Exception("Risk range not configured");

        DateTime maxDeliveryTime = order.OrderDate + AdminManager.GetConfig()?.MaxDeliveryTime ??
            throw new Exception("Max Delivery Time");

        if (delivery == null)
        {
            delivery = (from deliver in DeliveryManager.ReadAll()
                        where deliver.OrderId == order.Id
                        select deliver).FirstOrDefault();
        }

        if (order.OrderStatus is DO.OrderStatus.COMPLETED)
        {
            DateTime timeEndDelivery = delivery?.TimeEndDelivery ??
                throw new Exception("order completed but not fonud delivry");

            if (maxDeliveryTime >= timeEndDelivery)
            {
                return BO.ScheduleStatus.ONTYME;
            }
            else
            {
                return BO.ScheduleStatus.LATE;
            }
        }

        if(order.OrderStatus is DO.OrderStatus.DELIVERING)
        {
            if(delivery is null)  throw new Exception("order start but not fonud delivry");

            TimeSpan timeBuffer = maxDeliveryTime - GetEstimatedDeliveryTime(delivery);

            if (timeBuffer > riskRange)
                return BO.ScheduleStatus.ONTYME;

            if (timeBuffer >= TimeSpan.Zero) // כלומר: בין 0 ל-riskRange
                return BO.ScheduleStatus.INRISK;

            return BO.ScheduleStatus.LATE; // הזמן המשוער הוא אחרי זמן המקסימום (שלילי)
        }

        if(order.OrderStatus is DO.OrderStatus.OPEN)
        {
            TimeSpan timeLaft = maxDeliveryTime - AdminManager.Now;
            TimeSpan timeBuffer = timeLaft - TimeSpan.FromHours((GetDistance(order) / 4));

            if (timeBuffer > riskRange)
                return BO.ScheduleStatus.ONTYME;

            if (timeBuffer >= TimeSpan.Zero) // כלומר: בין 0 ל-riskRange
                return BO.ScheduleStatus.INRISK;

            return BO.ScheduleStatus.LATE; // הזמן המשוער הוא אחרי זמן המקסימום (שלילי)
        }

        return BO.ScheduleStatus.CONCEL;

    }

    //בדיקות תקינות של ערכים
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    public static bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^0\d{8,9}$") ||
        Regex.IsMatch(phone, @"^\+?[1-9]\d{1,14}$");
    }
    public static bool IsStrongPassword(string password)
    {
        if (password.Length < 8)
            return false;
        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
    public static bool IsValidId(int id)
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

    public static DateTime GetEstimatedDeliveryTime(DO.Delivery delivery)
    {
        DateTime estimatedDeliveryTime;
        if (delivery.ActualDistance.HasValue)
        {
            // שליפת המהירות הממוצעת לפי סוג הרכב
            DO.Courier courier = s_dal.Courier.Read(delivery.CourierId)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={delivery.CourierId} does Not exist");
            double avgSpeed = courier.TypeShipment switch
            {
                DO.TheTypeShipment.CAR => AdminManager.GetConfig().AvgSpeedCar,
                DO.TheTypeShipment.MOTORCYCLE => AdminManager.GetConfig().AvgSpeedMotorcycle,
                DO.TheTypeShipment.BIKE => AdminManager.GetConfig().AvgSpeedBike,
                DO.TheTypeShipment.FOOT => AdminManager.GetConfig().AvgSpeedFoot,
                _ => 1.0
            };

            // חישוב משך הזמן בשעות והוספה לזמן ההזמנה
            double estimatedHours = delivery.ActualDistance.Value / avgSpeed;
            estimatedDeliveryTime = delivery.OrderDate.AddHours(estimatedHours);
        }
        else
        {
            // אם אין מרחק בפועל, משתמשים בזמן המקסימלי המוגדר
            estimatedDeliveryTime = delivery.OrderDate + AdminManager.GetConfig().MaxDeliveryTime;
        }
        return estimatedDeliveryTime;

    }
}
