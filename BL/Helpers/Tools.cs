using DalApi;
using BO;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;


/// <summary>
/// Provides utility methods for distance calculations, validation, status determination,
/// and external API integration for the delivery system.
/// </summary>
/// <remarks>
/// This static class contains helper methods used throughout the business logic layer,
/// including geographic calculations, data validation, order status tracking,
/// and communication services.
/// </remarks>
internal static class Tools
{
    /// <summary>
    /// Data access layer instance for database operations.
    /// </summary>
    private static readonly IDal s_dal = Factory.Get;

    /// <summary>
    /// Earth's radius in kilometers for distance calculations.
    /// </summary>
    private const double EarthRadiusKm = 6371;

    /// <summary>
    /// Converts an object to its string representation with property details.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert.</typeparam>
    /// <param name="t">The object to convert.</param>
    /// <returns>
    /// A formatted string containing all properties and their values.
    /// Returns "null" if the object is null.
    /// </returns>
    /// <remarks>
    /// <para>This method uses reflection to iterate through all public properties of the object.</para>
    /// <para>For collection properties (except strings), displays items in a comma-separated list format.</para>
    /// <para>Password fields are masked with asterisks for security.</para>
    /// </remarks>
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null) return "null";

        StringBuilder sb = new StringBuilder();
        Type type = t.GetType();
        PropertyInfo[] properties = type.GetProperties();

        foreach (PropertyInfo prop in properties)
        {
            var value = prop.GetValue(t);
            string strValue = s_formatPropertyValue(prop.Name, value);
            sb.AppendLine($"        {prop.Name}: {strValue}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a property value for display, handling collections and sensitive data.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to format.</param>
    /// <returns>A formatted string representation of the value.</returns>
    private static string s_formatPropertyValue(string propertyName, object? value)
    {
        if (value == null)
            return "null";

        // Mask password fields
        if (propertyName.Equals("password", StringComparison.OrdinalIgnoreCase))
            return "******";

        // Format collections
        if (value is IEnumerable collection && value is not string)
        {
            var items = collection.Cast<object>()
                                  .Select(item => item?.ToString() ?? "null");
            return $"[{string.Join(", ", items)}]";
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Calculates the distance between two geographic coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">The latitude of the first point in decimal degrees.</param>
    /// <param name="lon1">The longitude of the first point in decimal degrees.</param>
    /// <param name="lat2">The latitude of the second point in decimal degrees.</param>
    /// <param name="lon2">The longitude of the second point in decimal degrees.</param>
    /// <returns>The distance between the two points in kilometers.</returns>
    /// <remarks>
    /// This method uses the Haversine formula to calculate the great-circle distance
    /// between two points on Earth's surface. The Earth's radius is assumed to be 6371 km.
    /// </remarks>
    public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = s_degreesToRadians(lat2 - lat1);
        double dLon = s_degreesToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(s_degreesToRadians(lat1)) * Math.Cos(s_degreesToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates the distance from the store to the order's delivery location.
    /// </summary>
    /// <param name="order">The order containing the delivery location coordinates.</param>
    /// <returns>The distance from the store to the order location in kilometers.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the store's latitude or longitude is not configured in the system.
    /// </exception>
    /// <remarks>
    /// This overload retrieves the store's coordinates from the system configuration
    /// and calculates the distance to the order's delivery location.
    /// </remarks>
    public static double GetDistance(DO.Order order)
    {
        var config = AdminManager.GetConfig();

        double storeLatitude = config.Latitude ??
            throw new InvalidOperationException("Latitude is not set in configuration.");

        double storeLongitude = config.Longitude ??
            throw new InvalidOperationException("Longitude is not set in configuration.");

        return GetDistance(order.Latitude, order.Longitude, storeLatitude, storeLongitude);
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees to convert.</param>
    /// <returns>The angle converted to radians.</returns>
    private static double s_degreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    /// <summary>
    /// Determines the business logic order status based on the order and its associated delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <param name="delivery">The delivery record associated with the order, or null if no delivery exists.</param>
    /// <returns>
    /// The business logic order status:
    /// <list type="bullet">
    ///   <item><description>OPEN - No delivery exists or delivery ended with NOTFOUND/FAILED status</description></item>
    ///   <item><description>COMPLETED - Delivery ended with DELIVERED status</description></item>
    ///   <item><description>REFUSED - Delivery ended with REFUSED status</description></item>
    ///   <item><description>CANCELLED - Delivery ended with CANCELLED status</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="Exception">Thrown when the delivery has an unknown status.</exception>
    public static BO.OrderStatus GetOrderStatus(DO.Order order, DO.Delivery? delivery)
    {
        if (delivery is null)
            return BO.OrderStatus.OPEN;

        return delivery.EndDelivery switch
        {
            DO.EndDelivery.DELIVERED => BO.OrderStatus.COMPLETED,
            DO.EndDelivery.REFUSED => BO.OrderStatus.REFUSED,
            DO.EndDelivery.CONCELLED => BO.OrderStatus.CANCELLED,
            DO.EndDelivery.FAILED => BO.OrderStatus.OPEN,
            DO.EndDelivery.NOTFOUND => BO.OrderStatus.OPEN,
            null => BO.OrderStatus.OPEN,
            _ => throw new Exception("Unknown delivery status")
        };
    }

    /// <summary>
    /// Determines the business logic order status for an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <returns>The business logic order status based on the most recent delivery attempt.</returns>
    public static BO.OrderStatus GetOrderStatus(DO.Order order)
    {
        var delivery = s_getLatestDelivery(order.Id);
        return GetOrderStatus(order, delivery);
    }

    /// <summary>
    /// Determines the schedule status of an order based on its timing constraints and current progress.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <param name="delivery">The delivery record associated with the order, or null to retrieve automatically.</param>
    /// <returns>
    /// The schedule status:
    /// <list type="bullet">
    ///   <item><description>ONTYME - Delivery is on track to meet the deadline</description></item>
    ///   <item><description>INRISK - Delivery is at risk of being late (within the risk range buffer)</description></item>
    ///   <item><description>LATE - Delivery has missed or will miss the deadline</description></item>
    ///   <item><description>CANCELLED - Order has been cancelled or refused</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when risk range or max delivery time is not configured,
    /// or when a completed order has no associated delivery record.
    /// </exception>
    public static async Task<BO.ScheduleStatus> GetScheduleStatus(DO.Order order, DO.Delivery? delivery = null)
    {
        BO.Config config;
        lock (AdminManager.BlMutex)
            config = AdminManager.GetConfig();
        TimeSpan riskRange = config?.RiskRange ??
            throw new Exception("Risk range not configured");

        DateTime maxDeliveryTime = order.OrderDate + (config?.MaxDeliveryTime ??
            throw new Exception("Max Delivery Time not configured"));

        return order.OrderStatus switch
        {
            DO.OrderStatus.COMPLETED => GetCompletedOrderScheduleStatus(order, delivery, maxDeliveryTime),
            DO.OrderStatus.DELIVERING => await GetDeliveringOrderScheduleStatus(order, maxDeliveryTime, riskRange),
            DO.OrderStatus.OPEN => GetOpenOrderScheduleStatus(order, maxDeliveryTime, riskRange),
            _ => BO.ScheduleStatus.CANCELLED
        };
    }

    /// <summary>
    /// Determines the schedule status of an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <returns>The schedule status based on timing constraints and current progress.</returns>
    public static async Task<BO.ScheduleStatus> GetScheduleStatus(DO.Order order)
    {
        var delivery = s_getLatestDelivery(order.Id);
        return await GetScheduleStatus(order, delivery);
    }

    /// <summary>
    /// Gets the schedule status for a completed order.
    /// </summary>
    private static BO.ScheduleStatus GetCompletedOrderScheduleStatus(
        DO.Order order,
        DO.Delivery? delivery,
        DateTime maxDeliveryTime)
    {
        try
        {
            lock (AdminManager.BlMutex)
                if (delivery is null)
                    s_getLatestDelivery(order.Id);

            DateTime timeEndDelivery = delivery?.TimeEndDelivery ??
                throw new Exception("Order completed but delivery not found");

            return maxDeliveryTime >= timeEndDelivery
           ? BO.ScheduleStatus.ONTYME
           : BO.ScheduleStatus.LATE;
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"orderId = {order.Id}. DeliveryId = {delivery?.Id}");
            Debug.WriteLine(ex);

            throw new Exception(ex.Message);
        }
        
    }

    /// <summary>
    /// Gets the schedule status for an order currently being delivered.
    /// </summary>
    private static async Task<BO.ScheduleStatus> GetDeliveringOrderScheduleStatus(
        DO.Order order,
        DateTime maxDeliveryTime,
        TimeSpan riskRange)
    {
        TimeSpan estimatedTime = await GetEstimatedDeliveryTime(order) ?? TimeSpan.Zero;
        TimeSpan timeBuffer = (maxDeliveryTime - AdminManager.Now) - estimatedTime;

        return EvaluateTimeBuffer(timeBuffer, riskRange);
    }

    /// <summary>
    /// Gets the schedule status for an open order.
    /// </summary>
    private static BO.ScheduleStatus GetOpenOrderScheduleStatus(
        DO.Order order,
        DateTime maxDeliveryTime,
        TimeSpan riskRange)
    {
        TimeSpan timeLeft = maxDeliveryTime - AdminManager.Now;
        // Estimate time based on average walking speed of 4 km/h
        TimeSpan timeBuffer = timeLeft - TimeSpan.FromHours(GetDistance(order) / 4);

        return EvaluateTimeBuffer(timeBuffer, riskRange);
    }

    /// <summary>
    /// Evaluates the time buffer to determine schedule status.
    /// </summary>
    /// <param name="timeBuffer">The available time buffer.</param>
    /// <param name="riskRange">The risk threshold range.</param>
    /// <returns>The appropriate schedule status.</returns>
    private static BO.ScheduleStatus EvaluateTimeBuffer(TimeSpan timeBuffer, TimeSpan riskRange)
    {
        if (timeBuffer > riskRange)
            return BO.ScheduleStatus.ONTYME;

        if (timeBuffer >= TimeSpan.Zero)
            return BO.ScheduleStatus.INRISK;

        return BO.ScheduleStatus.LATE;
    }

    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email address is valid; otherwise, false.</returns>
    /// <remarks>
    /// Uses the .NET MailAddress class to validate the email format.
    /// </remarks>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a phone number format.
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    /// <remarks>
    /// Accepts Israeli phone numbers: 7-10 digits, all numeric characters.
    /// </remarks>
    public static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 7 || phone.Length > 10)
            return false;

        return phone.All(char.IsDigit);
    }

    /// <summary>
    /// Validates password strength based on security requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password meets all strength requirements; otherwise, false.</returns>
    /// <remarks>
    /// A strong password must:
    /// <list type="bullet">
    ///   <item><description>Be at least 8 characters long</description></item>
    ///   <item><description>Contain at least one uppercase letter</description></item>
    ///   <item><description>Contain at least one lowercase letter</description></item>
    ///   <item><description>Contain at least one digit</description></item>
    ///   <item><description>Contain at least one special character (non-alphanumeric)</description></item>
    /// </list>
    /// </remarks>
    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    /// <summary>
    /// Validates an Israeli ID number using the Luhn algorithm (modulo 10).
    /// </summary>
    /// <param name="id">The ID number to validate.</param>
    /// <returns>True if the ID number is valid according to the Israeli ID checksum; otherwise, false.</returns>
    /// <remarks>
    /// Israeli ID numbers use a checksum algorithm where:
    /// <list type="bullet">
    ///   <item><description>Odd-positioned digits (from right) are doubled, and their digits are summed</description></item>
    ///   <item><description>Even-positioned digits are added as-is</description></item>
    ///   <item><description>The last digit must make the total sum divisible by 10</description></item>
    /// </list>
    /// </remarks>
    public static bool IsValidId(int id)
    {
        int tempId = id / 10; // Skip the check digit
        int sum = 0;

        for (int i = 1; i < 9; i++)
        {
            int digit = tempId % 10;

            if (i % 2 == 0)
            {
                sum += digit;
            }
            else
            {
                int doubled = digit * 2;
                sum += (doubled % 10) + (doubled / 10);
            }

            tempId /= 10;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return id % 10 == checkDigit;
    }

    /// <summary>
    /// Validates that a delivery distance is within the allowed maximum range.
    /// </summary>
    /// <param name="distance">The distance in kilometers to validate.</param>
    /// <returns>True if the distance is within the allowed range; otherwise, false.</returns>
    public static bool IsValidDistens(double distance)
    {
        var maxRange = AdminManager.GetConfig().MaxDeliveryRange;
        return maxRange == null || distance <= maxRange;
    }

    /// <summary>
    /// Calculates the estimated delivery time based on shipment type and distance.
    /// </summary>
    /// <param name="typeShipment">The type of vehicle/shipment method.</param>
    /// <param name="actualDistance">The distance to travel in kilometers.</param>
    /// <returns>The estimated time to complete the delivery.</returns>
    public static TimeSpan? GetEstimatedDeliveryTime(BO.TheTypeShipment typeShipment, double actualDistance)
    {
        double avgSpeed = s_getAverageSpeed(typeShipment);
        double estimatedHours = actualDistance / avgSpeed;
        return TimeSpan.FromHours(estimatedHours);
    }

    /// <summary>
    /// Calculates the estimated delivery time for an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to calculate estimated delivery time for.</param>
    /// <returns>
    /// The estimated delivery time if a delivery exists, or null if no delivery has been assigned.
    /// </returns>
    public static async Task<TimeSpan?> GetEstimatedDeliveryTime(DO.Order order)
    {
        var delivery = s_getLatestDelivery(order.Id);
        return delivery == null ? null : await GetEstimatedDeliveryTime(delivery);
    }

    /// <summary>
    /// Calculates the estimated delivery time for an active delivery.
    /// </summary>
    /// <param name="delivery">The delivery record.</param>
    /// <param name="courier">Optional courier information (will be retrieved if not provided).</param>
    /// <returns>The estimated delivery time, or null if the delivery is already completed.</returns>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the courier or order associated with the delivery is not found.
    /// </exception>
    public static async Task<TimeSpan?> GetEstimatedDeliveryTime(DO.Delivery delivery, DO.Courier? courier = null)
    {
        if (delivery.EndDelivery != null)
            return null;
        lock (AdminManager.BlMutex)
            courier ??= s_dal.Courier.Read(delivery.CourierId) ??
                throw new BO.BlDoesNotExistException($"Courier with ID={delivery.CourierId} does not exist");

        double? actualDistance = delivery.ActualDistance;

        if (actualDistance is null or 0)
        {
            DO.Order order;
            lock (AdminManager.BlMutex)
                order = s_dal.Order.Read(delivery.OrderId)
                    ?? throw new BO.BlDoesNotExistException($"Order with ID={delivery.OrderId} does not exist");

            actualDistance = await GoogleMapsService.NetworkKeeper(() =>
                GoogleMapsService.GetActualDistance(order.Latitude, order.Longitude,
                (BO.TheTypeShipment)courier.TypeShipment)) ?? 0;
            lock (AdminManager.BlMutex)
                s_dal.Delivery.Update(delivery with { ActualDistance = actualDistance });
        }

        return GetEstimatedDeliveryTime((BO.TheTypeShipment)courier.TypeShipment, actualDistance ?? 0);
    }

    /// <summary>
    /// Gets the average speed for a shipment type from configuration.
    /// </summary>
    /// <param name="typeShipment">The shipment type.</param>
    /// <returns>The average speed in km/h.</returns>
    private static double s_getAverageSpeed(BO.TheTypeShipment typeShipment)
    {
        var config = AdminManager.GetConfig();
        return typeShipment switch
        {
            BO.TheTypeShipment.CAR => config.AvgSpeedCar,
            BO.TheTypeShipment.MOTORCYCLE => config.AvgSpeedMotorcycle,
            BO.TheTypeShipment.BIKE => config.AvgSpeedBike,
            BO.TheTypeShipment.FOOT => config.AvgSpeedFoot,
            _ => 1.0
        };
    }

    /// <summary>
    /// Calculates the time remaining until an order reaches its maximum delivery deadline.
    /// </summary>
    /// <param name="order">The order to calculate time remaining for.</param>
    /// <param name="status">The current status of the order.</param>
    /// <returns>
    /// The time remaining as a TimeSpan, or TimeSpan.Zero if the order is completed or cancelled.
    /// May return negative TimeSpan if the order is already late.
    /// </returns>
    public static TimeSpan GetTimeLeftForDelivery(DO.Order order, BO.OrderStatus status)
    {
        if (status is BO.OrderStatus.COMPLETED or BO.OrderStatus.CANCELLED or BO.OrderStatus.REFUSED)
            return TimeSpan.Zero;

        return (order.OrderDate + AdminManager.GetConfig().MaxDeliveryTime) - AdminManager.Now;
    }

    /// <summary>
    /// Calculates the time remaining until an order reaches its maximum delivery deadline.
    /// </summary>
    /// <param name="order">The order to calculate time remaining for.</param>
    /// <returns>The time remaining as a TimeSpan.</returns>
    public static TimeSpan GetTimeLeftForDelivery(DO.Order order)
    {
        BO.OrderStatus status = GetOrderStatus(order);
        return GetTimeLeftForDelivery(order, status);
    }

    /// <summary>
    /// Calculates the total time taken for a delivery from order placement to completion.
    /// </summary>
    /// <param name="order">The order to calculate delivery time for.</param>
    /// <param name="status">The current status of the order.</param>
    /// <param name="endDelivery">The delivery completion time.</param>
    /// <returns>
    /// The total delivery time as a TimeSpan if the order is completed,
    /// or TimeSpan.Zero if the order is still active.
    /// </returns>
    public static TimeSpan GetTotalTimeOfDelivery(DO.Order order, BO.OrderStatus status, DateTime? endDelivery)
    {
        if (status is BO.OrderStatus.OPEN or BO.OrderStatus.DELIVERING)
            return TimeSpan.Zero;

        return endDelivery - order.OrderDate ?? TimeSpan.Zero;
    }

    /// <summary>
    /// Retrieves the most recent delivery for an order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>The most recent delivery, or null if no deliveries exist.</returns>
    private static DO.Delivery? s_getLatestDelivery(int orderId)
    {
        DO.Delivery? lastDelivery;
        lock (AdminManager.BlMutex)
            lastDelivery = DeliveryManager.ReadAll(d => d.OrderId == orderId)
                .OrderByDescending(d => d.Id)
                .FirstOrDefault();
        return lastDelivery;
    }

    /// <summary>
    /// Checks if a given ID belongs to the system manager.
    /// </summary>
    /// <param name="id">The ID to verify.</param>
    /// <returns>True if the ID matches the manager ID; otherwise, false.</returns>
    public static bool CheckManger(int id)
        => id == AdminManager.GetConfig().ManagerId;

    ///// <summary>
    ///// Sends an email using SMTP via Gmail's SMTP server.
    ///// </summary>
    ///// <param name="toEmail">The recipient's email address.</param>
    ///// <param name="subject">The email subject line.</param>
    ///// <param name="body">The email body content.</param>
    ///// <exception cref="SmtpException">
    ///// Thrown when the email fails to send or the recipient address is empty.
    ///// </exception>
    ///// <remarks>
    ///// Uses Gmail's SMTP server (smtp.gmail.com) on port 587 with TLS encryption.
    ///// Requires valid Gmail credentials configured in the code.
    ///// </remarks>
    //public static void SendEmail(string toEmail, string subject, string body)
    //{
    //    if (string.IsNullOrWhiteSpace(toEmail))
    //        throw new SmtpException("Recipient email address is empty");

    //    try
    //    {
    //        using MailMessage mail = new MailMessage();
    //        using SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

    //        string fromEmail;
    //        fromEmail = AdminManager.GetConfig().EmailAddress ?? String.Empty;
    //        string password = "1234 5678 @#$% Asdf";

    //        mail.From = new MailAddress(fromEmail);
    //        mail.To.Add(toEmail);
    //        mail.Subject = subject;
    //        mail.Body = body;

    //        smtpServer.Port = 587;
    //        smtpServer.Credentials = new NetworkCredential(fromEmail, password);
    //        smtpServer.EnableSsl = true;

    //        smtpServer.Send(mail);
    //    }
    //    catch (SmtpException ex)

    //    {
    //        throw new SmtpException($"{ex.Message}");
    //    }
    //}
    private static readonly HttpClient client = new HttpClient();
    public static async Task SendEmailSkript(string toEmail, string subject, string body)
    {
        string headUrl = "https://script.google.com/macros/s/";

        string endUrl = "/exec";

        string scriptUrl = $"{headUrl}{AdminManager.GetConfig().ScriptUrl}{endUrl}";          //"https://script.google.com/macros/s/AKfycbzS7AZyOGCduI2uCPFxzLoWJ9TKADvwMJEca8Lm2WZprBMjTj8vAvwL3Y1F-Gdesv-gNg/exec";

        string scriptPass = AdminManager.GetConfig().ScriptPass;                                                                ///"sdfjsak8796978akljdf54gdfgr44";

        string name = "חנות הספרים- מיני פרוייקט";

        string requestUrl = $"{scriptUrl}?pas={scriptPass}" +
                                $"&address={Uri.EscapeDataString(toEmail)}" +
                                $"&sub={Uri.EscapeDataString(subject)}" +
                                $"&body={Uri.EscapeDataString(body)}" +
                                $"&from={Uri.EscapeDataString(name)}";
        try
        {
            HttpResponseMessage response = await client.GetAsync(requestUrl);//אסינכרוני לשלב 7
            if (!response.IsSuccessStatusCode)
            {
                throw new BLNoSendEmailException($"{response.StatusCode}");

            }


        }
        catch (Exception ex)//שלב 7
        {
            throw new BLNoSendEmailException($"{ex.Message}");
            //Console.WriteLine($"Exception in SendEmail: {ex.Message}");
        }

        return;
    }

    public static async Task SendSms(string phone, string name, string body)
    {
        //string encodedMessage = Uri.EscapeDataString(name+" "+body);
        string headUrl = "https://www.call2all.co.il/ym/api/SendSms";
        string token = AdminManager.GetConfig().TokenCallSms;
        string tokenUrl = $"{headUrl}?token={token}&phones={phone}&message={name + " " + body}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(tokenUrl);


            string result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BLNoSendSmsException("שגיאה בשליחת ההודעה: " + response.StatusCode);
            }

        }
        catch (Exception ex)
        {
            throw new BLNoSendSmsException("שגיאה בשליחת ההודעה: " + ex.Message);
        }
    }

    /// <summary>
    /// Retrieves and validates a delivery for completion.
    /// </summary>
    /// <param name="deliveryId">The delivery ID to retrieve.</param>
    /// <param name="courierId">The courier ID attempting to complete the delivery.</param>
    /// <returns>The validated delivery object.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the delivery is not found.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when the courier is not assigned to this delivery.</exception>
    public static DO.Delivery GetAndValidateDelivery(int deliveryId, int courierId)
    {
        DO.Delivery delivery;
        lock (AdminManager.BlMutex)
            delivery = s_dal.Delivery.Read(deliveryId)
            ?? throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found");

        if (delivery.CourierId != courierId)
        {
            throw new BO.BlInvalidValueException(
                $"Courier with ID {courierId} is not assigned to delivery {deliveryId}. " +
                $"Assigned courier: {delivery.CourierId}");
        }

        return delivery;
    }

    public static (string word, string imoje) ConvertTipeOrderToHebrew(DO.TypeOfOrder typeOrder)
    {
        return typeOrder switch
        {
            DO.TypeOfOrder.STANDART => ("רגיל","🚶‍"),
            DO.TypeOfOrder.FAST_DELIVERY => ("משלוח מהיר","🏃‍♂️"),
            DO.TypeOfOrder.DELIVER_IMMEDIATELY => ("משלוח מיידי","🚀"),
            _ => ("לא ידוע","❓")
        };
    }




}
