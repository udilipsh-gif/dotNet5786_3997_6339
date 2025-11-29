

using System.Text.RegularExpressions;

namespace Helpers;

internal static class Tools
{

    public static string ToStringProperty<T>(this T t)
    {
        return "hi";
    }

    public static double GetDistance(DO.Order order) // ישר מגמיני
    {
        const double R = 6371; // רדיוס כדור הארץ בקילומטרים

        // המרה ממעלות לרדיאנים
        double lat1 = AdminManager.GetConfig().Latitude ?? throw new InvalidOperationException("Latitude is not set in configuration.");
        double lon1 = AdminManager.GetConfig().Longitude ?? throw new InvalidOperationException("Longitude is not set in configuration.");
        double lat2 = order.Latitude;
        double lon2 = order.Longitude;
        double dLat = s_toRadians(lat2 - lat1);
        double dLon = s_toRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(s_toRadians(lat1)) * Math.Cos(s_toRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Return distance in kilometers
    }

    // פונקציית עזר להמרה לרדיאנים
    private static double s_toRadians(double angleIn10thofaDegree)
    {
        return (angleIn10thofaDegree * Math.PI) / 180;
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
}
