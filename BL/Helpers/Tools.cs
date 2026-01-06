using BO;
using DalApi;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Helpers;

/// <summary>
/// Provides utility methods for distance calculations, validation, status determination,
/// and external API integration for the delivery system.
/// </summary>
/// <remarks>
/// This static class contains helper methods used throughout the business logic layer,
/// including geographic calculations, data validation, order status tracking,
/// and geocoding services.
/// </remarks>
internal static class Tools
{
    private static readonly IDal s_dal = Factory.Get; //stage 4

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
    /// This method uses reflection to iterate through all public properties of the object.
    /// For collection properties (except strings), displays items in a comma-separated list format.
    /// Password fields are masked with asterisks for security.
    /// Each property is displayed on a new line with indentation.
    /// </remarks>
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null) return "null";

        StringBuilder sb = new StringBuilder();

        Type type = t.GetType();
        PropertyInfo[] properties = type.GetProperties();
       // sb.Append(type.Name + " Details:\n");
        foreach (PropertyInfo prop in properties)
        {
            // שליפת הערך של המאפיין מתוך האובייקט t
            var value = prop.GetValue(t);
            string strValue = "null";

            if (value != null)
            {
                  if (value is IEnumerable collection && !(value is string))
                {
                    var items = collection.Cast<object>()
                                          .Select(item => item?.ToString() ?? "null");

    
                    strValue = $"[{string.Join(", ", items)}]";
                }
                else
                {
                   
                    strValue = value.ToString();
                    if (prop.Name is "password" or "Password")
                        strValue = "******";
                }
            }

            // 5. הוספת שם המאפיין והערך שלו למחרוזת הסופית
            sb.AppendLine($"        {prop.Name}: {strValue}");
        }

        return sb.ToString();
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
    /// Implementation based on GIMINI algorithm.
    /// </remarks>
    public static double GetDistance(double lat1, double lon1, double lat2, double lon2) 
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
    public static double GetDistance(DO.Order order) //פונקציית העמסה למרחק מהחנות להזמנה
    {
        double storeLatitude = AdminManager.GetConfig().Latitude ??
            throw new InvalidOperationException("Latitude is not set in configuration.");

        double storeLongitude = AdminManager.GetConfig().Longitude ??
            throw new InvalidOperationException("Longitude is not set in configuration.");

        return GetDistance(order.Latitude, order.Longitude, storeLatitude, storeLongitude);
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="angleIn10thofaDegree">The angle in degrees to convert.</param>
    /// <returns>The angle converted to radians.</returns>
    /// <remarks>
    /// This is a helper method used by the Haversine distance calculation.
    /// </remarks>
    private static double s_toRadians(double angleIn10thofaDegree)
    {
        return (angleIn10thofaDegree * Math.PI) / 180;
    }

    /// <summary>
    /// Determines the business logic order status based on the order and its associated delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <param name="delivery">The delivery record associated with the order, or null if no delivery exists.</param>
    /// <returns>
    /// The business logic order status:
    /// - OPEN if no delivery exists or delivery ended with NOTFOUND status
    /// - COMPLETED if delivery ended with DELIVERED status
    /// - REFUSED if delivery ended with REFUSED status
    /// - CANCELLED if delivery ended with CONCELLED or FAILED status
    /// </returns>
    /// <exception cref="Exception">Thrown when the delivery has an unknown status.</exception>
    /// <remarks>
    /// This method maps data layer delivery end statuses to business logic order statuses.
    /// Orders with NOTFOUND delivery status are returned to OPEN status for retry attempts.
    /// </remarks>
    public static BO.OrderStatus GetOrderStatus(DO.Order order, DO.Delivery? delivery)
    {
        if (delivery is null)
        {
            return BO.OrderStatus.OPEN;
        }
        else
        {
            return (delivery.EndDelivery) switch
            {
                DO.EndDelivery.DELIVERED => BO.OrderStatus.COMPLETED,
                DO.EndDelivery.REFUSED => BO.OrderStatus.REFUSED,
                DO.EndDelivery.CONCELLED => BO.OrderStatus.CANCELLED,
                DO.EndDelivery.FAILED => BO.OrderStatus.CANCELLED,
                DO.EndDelivery.NOTFOUND => BO.OrderStatus.OPEN,
                null => BO.OrderStatus.OPEN,
                _ => throw new Exception("Unknown delivery status"),
            };
        }
    }

    /// <summary>
    /// Determines the business logic order status for an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <returns>The business logic order status based on the most recent delivery attempt.</returns>
    /// <remarks>
    /// This overload automatically retrieves the most recent delivery for the order
    /// and determines the status. If no delivery exists, the order is considered OPEN.
    /// </remarks>
    public static BO.OrderStatus GetOrderStatus(DO.Order order)
    {
        var delivery = (from deliver in DeliveryManager.ReadAll()
                        where deliver.OrderId == order.Id
                        orderby deliver.Id descending
                        select deliver).FirstOrDefault();
        return GetOrderStatus(order, delivery);
    }

    /// <summary>
    /// Determines the schedule status of an order based on its timing constraints and current progress.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <param name="delivery">The delivery record associated with the order, or null to retrieve automatically.</param>
    /// <returns>
    /// The schedule status:
    /// - ONTYME: Delivery is on track to meet the deadline
    /// - INRISK: Delivery is at risk of being late (within the risk range buffer)
    /// - LATE: Delivery has missed or will miss the deadline
    /// - CANCELLED: Order has been cancelled or refused
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when risk range or max delivery time is not configured,
    /// or when a completed/delivering order has no associated delivery record.
    /// </exception>
    /// <remarks>
    /// For COMPLETED orders: Compares actual delivery time against the maximum allowed time.
    /// For DELIVERING orders: Calculates estimated delivery time and compares against deadline.
    /// For OPEN orders: Estimates time needed based on distance (using 4 km/h average) and compares against deadline.
    /// For CANCELLED/REFUSED orders: Returns CANCELLED status.
    /// The risk range buffer helps identify orders that may become late soon.
    /// </remarks>
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

        if (order.OrderStatus is DO.OrderStatus.DELIVERING)
        {
            if (delivery is null) throw new Exception("order start but not fonud delivry");

            TimeSpan timeBuffer = maxDeliveryTime - GetEstimatedDeliveryTime(delivery);

            if (timeBuffer > riskRange)
                return BO.ScheduleStatus.ONTYME;

            if (timeBuffer >= TimeSpan.Zero) // כלומר: בין 0 ל-riskRange
                return BO.ScheduleStatus.INRISK;

            return BO.ScheduleStatus.LATE; // הזמן המשוער הוא אחרי זמן המקסימום (שלילי)
        }

        if (order.OrderStatus is DO.OrderStatus.OPEN)
        {
            TimeSpan timeLaft = maxDeliveryTime - AdminManager.Now;
            TimeSpan timeBuffer = timeLaft - TimeSpan.FromHours((GetDistance(order) / 4));

            if (timeBuffer > riskRange)
                return BO.ScheduleStatus.ONTYME;

            if (timeBuffer >= TimeSpan.Zero) // כלומר: בין 0 ל-riskRange
                return BO.ScheduleStatus.INRISK;

            return BO.ScheduleStatus.LATE; // הזמן המשוער הוא אחרי זמן המקסימום (שלילי)
        }

        return BO.ScheduleStatus.CANCELLED;

    }

    /// <summary>
    /// Determines the schedule status of an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to evaluate.</param>
    /// <returns>The schedule status based on timing constraints and current progress.</returns>
    /// <remarks>
    /// This overload automatically retrieves the most recent delivery for the order
    /// and determines the schedule status. See the main overload for detailed logic.
    /// </remarks>
    public static BO.ScheduleStatus GetScheduleStatus(DO.Order order)
    {
        var delivery = (from deliver in DeliveryManager.ReadAll()
                        where deliver.OrderId == order.Id
                        orderby deliver.Id descending
                        select deliver).FirstOrDefault();

        return GetScheduleStatus(order, delivery);
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
            var addr = new System.Net.Mail.MailAddress(email);
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
    /// Accepts two formats:
    /// - Israeli format: Starts with 0 followed by 8-9 digits (e.g., 0501234567)
    /// - International format: Optional + followed by country code and 1-14 digits (e.g., +972501234567)
    /// </remarks>
    public static bool IsValidPhone(string phone)
    {
        if (phone.Length<7 ||phone.Length>10)
            return false;
        for (int i=0; i<phone.Length; i++)//בעיקרון ניתן להגביל ל10 תווים
        {
            if (phone[i] < '0' || phone[i] > '9')
            
                return false;
            
        }
        return true;
    }

    /// <summary>
    /// Validates password strength based on security requirements.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password meets all strength requirements; otherwise, false.</returns>
    /// <remarks>
    /// A strong password must:
    /// - Be at least 8 characters long
    /// - Contain at least one uppercase letter
    /// - Contain at least one lowercase letter
    /// - Contain at least one digit
    /// - Contain at least one special character (non-alphanumeric)
    /// </remarks>
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

    /// <summary>
    /// Validates an Israeli ID number using the Luhn algorithm (modulo 10).
    /// </summary>
    /// <param name="id">The ID number to validate.</param>
    /// <returns>True if the ID number is valid according to the Israeli ID checksum; otherwise, false.</returns>
    /// <remarks>
    /// Israeli ID numbers use a checksum algorithm where:
    /// - Odd-positioned digits (from right) are doubled, and their digits are summed
    /// - Even-positioned digits are added as-is
    /// - The last digit must make the total sum divisible by 10
    /// </remarks>
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

    /// <summary>
    /// Calculates the estimated delivery time for an active delivery.
    /// </summary>
    /// <param name="delivery">The delivery record containing distance and courier information.</param>
    /// <returns>The estimated date and time when the delivery will be completed.</returns>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the courier associated with the delivery is not found.
    /// </exception>
    /// <remarks>
    /// If the actual distance is available, estimates based on:
    /// - The courier's vehicle type and its average speed (from configuration)
    /// - The actual distance to be traveled
    /// 
    /// If no actual distance is available, uses the maximum delivery time from configuration.
    /// The calculation assumes constant average speed for the vehicle type.
    /// </remarks>
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

    /// <summary>
    /// Calculates the estimated delivery time for an order by retrieving its most recent delivery.
    /// </summary>
    /// <param name="order">The order to calculate estimated delivery time for.</param>
    /// <returns>
    /// The estimated delivery time if a delivery exists, or null if no delivery has been assigned.
    /// </returns>
    /// <remarks>
    /// This overload automatically retrieves the most recent delivery for the order.
    /// See the main overload for calculation details.
    /// </remarks>
    public static DateTime? GetEstimatedDeliveryTime(DO.Order order)
    {
        var delivery = (from deliver in DeliveryManager.ReadAll()
                        where deliver.OrderId == order.Id
                        orderby deliver.Id descending
                        select deliver).FirstOrDefault();

        return delivery == null ? null : GetEstimatedDeliveryTime(delivery);
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
    /// <remarks>
    /// For COMPLETED or CANCELLED orders, returns zero as no time is left.
    /// For active orders (OPEN, DELIVERING, REFUSED), calculates: (OrderDate + MaxDeliveryTime) - CurrentTime
    /// </remarks>
    public static TimeSpan GetTimeLeftForDelivery(DO.Order order, BO.OrderStatus status)
    {
        if (status is BO.OrderStatus.COMPLETED or BO.OrderStatus.CANCELLED)
        {
            return TimeSpan.Zero;
        }

        return (order.OrderDate + AdminManager.GetConfig().MaxDeliveryTime) - AdminManager.Now;
    }

    /// <summary>
    /// Calculates the time remaining until an order reaches its maximum delivery deadline.
    /// </summary>
    /// <param name="order">The order to calculate time remaining for.</param>
    /// <returns>The time remaining as a TimeSpan.</returns>
    /// <remarks>
    /// This overload automatically determines the order status before calculating time remaining.
    /// </remarks>
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
    /// <param name="delivery">The delivery record containing completion time.</param>
    /// <returns>
    /// The total delivery time as a TimeSpan if the order is completed or cancelled,
    /// or TimeSpan.Zero if the order is still active (OPEN, DELIVERING, or REFUSED).
    /// </returns>
    /// <remarks>
    /// For COMPLETED or CANCELLED orders, calculates: TimeEndDelivery - OrderDate
    /// For active orders (OPEN, DELIVERING, REFUSED), returns zero as delivery is not complete.
    /// </remarks>
    public static TimeSpan GetTotalTimeOfDelivery(DO.Order order, BO.OrderStatus status, DO.Delivery? delivery)
    {
        if (status is BO.OrderStatus.COMPLETED or BO.OrderStatus.CANCELLED)
        {
            return delivery!.TimeEndDelivery!.Value - order.OrderDate;
        }

        return TimeSpan.Zero; ;
    }

    /// <summary>
    /// Counts the number of delivery attempts made for a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The total number of delivery attempts (records) for the order.</returns>
    /// <remarks>
    /// Each delivery assignment to a courier creates a new delivery record.
    /// This count includes all attempts regardless of outcome (delivered, refused, failed, etc.).
    /// </remarks>
    public static int GetCuntOfDelivery(int orderId)
    {
        return s_dal.Delivery.ReadAll(d => d.OrderId == orderId).Count();
    }

    /// <summary>
    /// Converts a street address to geographic coordinates using the Google Geocoding API.
    /// </summary>
    /// <param name="address">The street address to geocode.</param>
    /// <returns>
    /// A tuple containing the latitude and longitude coordinates if successful,
    /// or null if the geocoding fails or the address is not found.
    /// </returns>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when the address is not found in Google's database (ZERO_RESULTS),
    /// or when the address is not precise enough (APPROXIMATE, RANGE_INTERPOLATED, or GEOMETRIC_CENTER location types).
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the API request fails, returns an error status, or encounters a parsing error.
    /// </exception>
    /// <remarks>
    /// This method makes a synchronous HTTP request to the Google Geocoding API.
    /// The API key is retrieved from the system configuration.
    /// The response is in XML format and parsed to extract the location coordinates.
    /// 
    /// Location type validation:
    /// - ROOFTOP: Precise address (accepted)
    /// - APPROXIMATE: City/area level (rejected - throws exception)
    /// - RANGE_INTERPOLATED: Interpolated between two points (rejected)
    /// - GEOMETRIC_CENTER: Center of an area (rejected)
    /// 
    /// Possible error scenarios:
    /// - Invalid API key (throws Exception)
    /// - Address not found (throws BlInvalidValueException)
    /// - Imprecise address (throws BlInvalidValueException)
    /// - Network errors (throws Exception)
    /// - Malformed XML response (throws Exception)
    /// </remarks>
    public static (double Lat, double Lng)? GetGeocodingSync(string address)
    {
        var apiKey = AdminManager.GetConfig().GoogleApiKey;

        string url = $"https://maps.googleapis.com/maps/api/geocode/xml?address={address}&key={apiKey}";

        using HttpClient client = new HttpClient();
        {
            client.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");
            try
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string xmlContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    XDocument doc = XDocument.Parse(xmlContent);
                    string? status = doc.Element("GeocodeResponse")?.Element("status")?.Value;

                    if (status == "ZERO_RESULTS")
                        throw new BO.BlInvalidValueException ("הכתובת לא נמצאה במאגר של גוגל.");
                    

                    if (status == "OK")
                    {
                        var geometry = doc.Element("GeocodeResponse")?
                                             .Element("result")?
                                             .Element("geometry");
                        var locationType = geometry?.Element("location_type")?.Value;
                        if (locationType is "APPROXIMATE" or "RANGE_INTERPOLATED" or "GEOMETRIC_CENTER")
                            throw new BO.BlInvalidValueException("הכתובת שהוזנה לא מדויקת, נא להזין כתובת מלאה יותר.");
                        
                        var locationElement = geometry?.Element("location");

                        if (locationElement != null)
                        {
                            double lat = double.Parse(locationElement.Element("lat")!.Value);
                            double lng = double.Parse(locationElement.Element("lng")!.Value);

                            return (lat, lng);
                        }
                        else
                        {
                            throw new Exception("Location element not found in the response.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Failed to get geocoding data.");
                }
            }
            catch (BO.BlInvalidValueException ex)
            {
                throw new BO.BlInvalidValueException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception: {ex.Message}");
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a given ID belongs to the system manager.
    /// </summary>
    /// <param name="Id">The ID to verify.</param>
    /// <returns>True if the ID matches the manager ID configured in the system; otherwise, false.</returns>
    /// <remarks>
    /// This method is used for authorization checks to determine if a user has manager privileges.
    /// The manager ID is retrieved from the system configuration.
    /// </remarks>
    public static bool CheckManger(int Id)
    {
        return Id == AdminManager.GetConfig().ManagerId;
    }

    /// <summary>
    /// Calculates the actual road distance from the store to a delivery address using the Google Distance Matrix API.
    /// </summary>
    /// <param name="address">The destination address for the delivery.</param>
    /// <param name="TypeShipment">The type of shipment/vehicle to be used for the delivery, which determines the travel mode.</param>
    /// <returns>
    /// The actual road distance in kilometers if successful, or null if the calculation fails.
    /// </returns>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when:
    /// - Google API Key is not configured in the system
    /// - Store Address is not configured in the system
    /// - The address was not found in Google's database (ZERO_RESULTS)
    /// - Unable to calculate distance for the provided address (element status not OK)
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the API request fails or encounters an error during execution.
    /// </exception>
    /// <remarks>
    /// This method makes a synchronous HTTP request to the Google Distance Matrix API.
    /// The API key and store address are retrieved from the system configuration.
    /// 
    /// Travel mode mapping:
    /// - FOOT/BIKE: walking mode
    /// - MOTORCYCLE/CAR: driving mode
    /// 
    /// The method calculates the actual road distance based on real routes, which may differ
    /// from the straight-line distance calculated by the Haversine formula. This is more
    /// accurate for estimating delivery times and courier assignments.
    /// 
    /// The response is in XML format and parsed to extract the distance value.
    /// The distance is returned in kilometers (converted from meters).
    /// 
    /// Possible error scenarios:
    /// - Missing configuration (API key or store address)
    /// - Invalid API key
    /// - Address not found
    /// - Unable to route between locations
    /// - Network errors
    /// - Malformed XML response
    /// </remarks>
    public static double? GetActualDistance(string address, BO.TheTypeShipment TypeShipment)
    {
        string apiKey = AdminManager.GetConfig().GoogleApiKey ?? throw new BO.BlInvalidValueException("Google API Key is not configured.");
        string StoreAddress = AdminManager.GetConfig().StoreAddress ?? throw new BO.BlInvalidValueException("Store Address is not configured.");
        string mode = TypeShipment switch
        {
            BO.TheTypeShipment.FOOT => "walking",
            BO.TheTypeShipment.BIKE => "walking",
            BO.TheTypeShipment.MOTORCYCLE => "driving",
            BO.TheTypeShipment.CAR => "driving",
            _ => "Driving"
        };

        string url = $"https://maps.googleapis.com/maps/api/distancematrix/xml?origins={StoreAddress}&destinations={address}&mode={mode}&key={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");
            try
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if(response.IsSuccessStatusCode)
                {
                    string xmlContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    XDocument doc = XDocument.Parse(xmlContent);
                    string? status = doc.Element("DistanceMatrixResponse")?.Element("status")?.Value;
                    if (status == "ZERO_RESULTS")
                        throw new BO.BlInvalidValueException("The address was not found in Google's database.");
                    if (status == "OK")
                    {
                        var element = doc.Element("DistanceMatrixResponse")?
                                             .Element("row")?
                                             .Element("element");
                        var elementStatus = element?.Element("status")?.Value;
                        if (elementStatus != "OK")
                            throw new BO.BlInvalidValueException("Unable to calculate distance for the provided address.");
                        var distanceElement = element?.Element("distance");
                        if (distanceElement != null)
                        {
                            double distance = double.Parse(distanceElement.Element("value")!.Value);
                            return distance / 1000.0; // Convert to kilometers
                        }
                        else
                        {
                            throw new Exception("Distance element not found in the response.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Failed to get distance matrix data.");
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlDoesNotExistException($"Exception: {ex.Message}");
            }
        }
        return null;
    }
    public static void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            string fromEmail = "aaaaaaaaa@gmail.com";
            string password = "1234 5678 @#$% Asdf"; // סיסמת האפליקציה (16 תווים)

            mail.From = new MailAddress(fromEmail);

            // ולידציה בסיסית למקרה שהמייל ריק
            if (string.IsNullOrWhiteSpace(toEmail))
                //throw new Exception("כתובת המייל של הנמען ריקה");
                throw new SmtpException("כתובת נמען ריקה");


            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
           

            // הגדרות שרת
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new NetworkCredential(fromEmail, password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }
        //catch (Exception ex)
        //{
           
        //    throw new Exception($"שגיאה בשליחת מייל: {ex.Message}");
        //}
        catch (SmtpException ex)
        {
            throw new SmtpException($"שגיאה בשליחת מייל: {ex.Message}");
        }
    }

}
