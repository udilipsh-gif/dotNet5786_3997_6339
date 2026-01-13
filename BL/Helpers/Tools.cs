using BO;
using DalApi;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
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
    /// Thread-safe cache for storing distance calculations to avoid repeated API calls.
    /// Key format: "origin|destination|mode"
    /// </summary>
    private static readonly ConcurrentDictionary<string, double> s_distanceCache = new();


    /// <summary>
    /// Indicates whether the cache has been initialized with predefined addresses.
    /// </summary>
    private static bool s_cacheInitialized = false;
    private static readonly object s_cacheLock = new();

    /// <summary>
    /// Initializes the distance cache with predefined address data.
    /// Should be called once at application startup.
    /// </summary>
    public static void InitializeDistanceCache()
    {
        if (s_cacheInitialized) return;

        lock (s_cacheLock)
        {
            if (s_cacheInitialized) return;

            string storeAddress = AdminManager.GetConfig().StoreAddress ?? "";
            string storeAddressLower = storeAddress.ToLowerInvariant();

            foreach (var address in Addresses)
            {
                string destination = ((string)address[0]).ToLowerInvariant();

                // איבר 3 - מרחק רכב (במטרים), להמיר לק"מ
                if (double.TryParse((string)address[3], out double drivingMeters) && drivingMeters > 0)
                {
                    string drivingKey = $"{storeAddressLower}|{destination}|driving";
                    s_distanceCache.TryAdd(drivingKey, drivingMeters / 1000.0);
                }

                // איבר 4 - מרחק הליכה (במטרים), להמיר לק"מ
                if (double.TryParse((string)address[4], out double walkingMeters) && walkingMeters > 0)
                {
                    string walkingKey = $"{storeAddressLower}|{destination}|walking";
                    s_distanceCache.TryAdd(walkingKey, walkingMeters / 1000.0);
                }
            }

            s_cacheInitialized = true;
        }
    }

    /// <summary>
    /// Clears the distance cache and resets initialization flag.
    /// Call this when the store address changes.
    /// </summary>
    public static void ClearDistanceCache()
    {
        lock (s_cacheLock)
        {
            s_distanceCache.Clear();
            s_cacheInitialized = false;
        }
    }

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
                DO.EndDelivery.FAILED => BO.OrderStatus.OPEN,
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



        if (order.OrderStatus is DO.OrderStatus.COMPLETED)
        {
            if (delivery == null)
            {
                delivery = (from deliver in DeliveryManager.ReadAll()
                            where deliver.OrderId == order.Id
                            select deliver).FirstOrDefault();
            }

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
            TimeSpan estimatedTime = GetEstimatedDeliveryTime(order) ?? TimeSpan.Zero;
            TimeSpan timeBuffer = (maxDeliveryTime - AdminManager.Now) - estimatedTime;

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
        if (phone.Length < 7 || phone.Length > 10)
            return false;
        for (int i = 0; i < phone.Length; i++)//בעיקרון ניתן להגביל ל10 תווים
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
    public static TimeSpan? GetEstimatedDeliveryTime(BO.TheTypeShipment typeShipment, double actualDistance)
    {
        double avgSpeed = typeShipment switch
        {
            BO.TheTypeShipment.CAR => AdminManager.GetConfig().AvgSpeedCar,
            BO.TheTypeShipment.MOTORCYCLE => AdminManager.GetConfig().AvgSpeedMotorcycle,
            BO.TheTypeShipment.BIKE => AdminManager.GetConfig().AvgSpeedBike,
            BO.TheTypeShipment.FOOT => AdminManager.GetConfig().AvgSpeedFoot,
            _ => 1.0
        };

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
    /// <remarks>
    /// This overload automatically retrieves the most recent delivery for the order.
    /// See the main overload for calculation details.
    /// </remarks>
    public static TimeSpan? GetEstimatedDeliveryTime(DO.Order order)
    {
        var delivery = (from deliver in DeliveryManager.ReadAll()
                        where deliver.OrderId == order.Id
                        orderby deliver.Id descending
                        select deliver).FirstOrDefault();

        return delivery == null ? null : GetEstimatedDeliveryTime(delivery);
    }

    public static TimeSpan? GetEstimatedDeliveryTime(DO.Delivery Delivery, DO.Courier? courier = null)
    {
        if (Delivery is DO.Delivery delivery && delivery.EndDelivery is null)
        {
            if (courier is null)
            {
                courier = s_dal.Courier.Read(delivery.CourierId)
                  ?? throw new BO.BlDoesNotExistException($"Courier with ID={delivery.CourierId} does Not exist");
            }

            double? actualDistance = delivery.ActualDistance;
            if (actualDistance is null or 0)
            {
                DO.Order order = s_dal.Order.Read(delivery.OrderId)
                  ?? throw new BO.BlDoesNotExistException($"Order with ID={delivery.OrderId} does Not exist");

                actualDistance = GetActualDistance(order.Addres, (BO.TheTypeShipment)courier.TypeShipment) ?? 0;

                s_dal.Delivery.Update(delivery with { ActualDistance = actualDistance });
            }
            return GetEstimatedDeliveryTime((BO.TheTypeShipment)courier.TypeShipment, actualDistance ?? 0);
        }
        else
        {
            return null;
        }

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
        if (status is BO.OrderStatus.COMPLETED or BO.OrderStatus.CANCELLED or BO.OrderStatus.REFUSED)
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
    public static TimeSpan GetTotalTimeOfDelivery(DO.Order order, BO.OrderStatus status, DateTime? endDelivery)
    {
        if (status == BO.OrderStatus.OPEN || status == BO.OrderStatus.DELIVERING)
            return TimeSpan.Zero;
        else
            return endDelivery - order.OrderDate ?? TimeSpan.Zero;
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
                        throw new BO.BlInvalidValueException("הכתובת לא נמצאה במאגר של גוגל.");


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
        Tools.InitializeDistanceCache();

        string apiKey = AdminManager.GetConfig().GoogleApiKey
            ?? throw new BO.BlInvalidValueException("Google API Key is not configured.");

        string storeAddress = AdminManager.GetConfig().StoreAddress
            ?? throw new BO.BlInvalidValueException("Store Address is not configured.");

        string mode = TypeShipment switch
        {
            BO.TheTypeShipment.FOOT => "walking",
            BO.TheTypeShipment.BIKE => "walking",
            BO.TheTypeShipment.MOTORCYCLE => "driving",
            BO.TheTypeShipment.CAR => "driving",
            _ => "driving"
        };

        string cacheKey = $"{storeAddress}|{address}|{mode}".ToLowerInvariant();

        // בדיקה אם המרחק כבר קיים במטמון
        if (s_distanceCache.TryGetValue(cacheKey, out double cachedDistance))
        {
            return cachedDistance;
        }

        string url = $"https://maps.googleapis.com/maps/api/distancematrix/xml?origins={storeAddress}&destinations={address}&mode={mode}&key={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");
            try
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
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
                            double distanceKm = distance / 1000.0;
                            s_distanceCache.TryAdd(cacheKey, distanceKm);
                            return distanceKm; // Return cached distance in kilometers
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
    /// <summary>
    ///  Sends an email using SMTP via Gmail's SMTP server.
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <exception cref="SmtpException"></exception>
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

    private static readonly object[][] Addresses =
   {
         [ " רבי עקיבא 50, בני ברק", "32.0876045", "34.8278091", "912", "898"],
         [ " נחמיה 8, בני ברק", "32.0793696", "34.8352634", "3349", "2361"],
         [ " בן גוריון 25, גבעת שמואל", "32.0952663", "34.8220752", "421", "308"],
         [ " דסלר 4, בני ברק", "32.0818569", "34.8315636", "2697", "1740"],
         [ " סוקולוב 30, בני ברק", "32.0889927", "34.8341474", "1588", "1468"],
         [ " הרב קוק 18, בני ברק", "32.0865247", "34.8262516", "1367", "1047"],
         [ " הירקון 5, בני ברק", "32.0965116", "34.822183", "327", "437"],
         [ " ביאליק 40, רמת גן", "32.0828089", "34.8147672", "1993", "1853"],
         [ " הרצל 60, רמת גן", "32.0850858", "34.8155354", "1949", "1528"],
         [ " אבא הלל 15, רמת גן", "32.085569", "34.803969", "2578", "2267"],
         [ " ז'בוטינסקי 55, רמת גן", "32.0854799", "34.8101674", "1772", "1697"],
         [ " חזון איש 20, בני ברק", "32.0834133", "34.8356933", "2466", "1975"],
         [ " קריניצי 20, רמת גן", "32.0797548", "34.8172104", "1907", "1833"],
         [ " הראה 80, רמת גן", "32.0817288", "34.8254736", "1741", "1599"],
         [ " ארלוזורוב 10, רמת גן", "32.0794038", "34.8138358", "2233", "2159"],
         [ " שדרות ירושלים 30, רמת גן", "32.0847242", "34.8297262", "1349", "1335"],
         [ " בן גוריון 100, רמת גן", "32.0865566", "34.8216913", "923", "842"],
         [ " נגבה 25, רמת גן", "32.070547", "34.8249663", "3026", "2884"],
         [ " הירדן 40, רמת גן", "32.066254", "34.8281993", "3809", "3520"],
         [ " אלוף שדה 15, רמת גן", "32.0594879", "34.8249646", "4698", "4569"],
         [ " רוקח 10, רמת גן", "32.0876832", "34.8111166", "2103", "1626"],
         [ " תרצה 8, רמת גן", "32.0752286", "34.8280531", "2913", "2520"],
         [ " ז'בוטינסקי 100, בני ברק", "32.0917476", "34.8321991", "1109", "1095"],
         [ " המעגל 12, רמת גן", "32.0829031", "34.8124271", "2213", "1935"],
         [ " ויצמן 20, גבעתיים", "32.0737141", "34.8083944", "3465", "3131"],
         [ " כצנלסון 50, גבעתיים", "32.0750742", "34.8076031", "4016", "3061"],
         [ " שיינקין 15, גבעתיים", "32.0748186", "34.8107739", "2974", "2884"],
         [ " רמב''ם 10, גבעתיים", "32.0700634", "34.8038533", "4371", "3933"],
         [ " גורדון 5, גבעתיים", "32.076085", "34.807312", "3383", "2953"],
         [ " סירקין 12, גבעתיים", "32.0775637", "34.814622", "2702", "2297"],
         [ " בורוכוב 8, גבעתיים", "32.0775935", "34.8029331", "3526", "3065"],
         [ " המאבק 25, גבעתיים", "32.0638384", "34.8099736", "4389", "4217"],
         [ " עליית הנוער 10, גבעתיים", "32.0770387", "34.8020075", "3997", "3232"],
         [ " ירושלים 15, בני ברק", "32.0857512", "34.8300664", "1231", "1217"],
         [ " דרך השלום 40, גבעתיים", "32.0694025", "34.8022553", "4712", "4245"],
         [ " דיזנגוף 100, תל אביב", "32.0793963", "34.7740011", "6268", "5571"],
         [ " בן יהודה 50, תל אביב", "32.0780449", "34.7688332", "7231", "6157"],
         [ " אבן גבירול 70, תל אביב", "32.0806057", "34.7815385", "5393", "4886"],
         [ " רוטשילד 45, תל אביב", "32.0642206", "34.7747952", "6615", "6204"],
         [ " אלנבי 80, תל אביב", "32.0678218", "34.7710389", "7120", "6334"],
         [ " המלך ג'ורג' 30, תל אביב", "32.0722931", "34.7741392", "6502", "5666"],
         [ " שינקין 20, תל אביב", "32.0693286", "34.7725233", "7396", "6205"],
         [ " דרך מנחם בגין 120, תל אביב", "32.071223", "34.7898718", "5442", "4494"],
         [ " המסגר 15, תל אביב", "32.0628255", "34.7850239", "7517", "5476"],
         [ " הרב שך 10, בני ברק", "32.0864414", "34.8357055", "2044", "1700"],
         [ " יגאל אלון 60, תל אביב", "32.0619207", "34.7927464", "8326", "5292"],
         [ " דרך ההגנה 40, תל אביב", "32.0540338", "34.7869603", "9038", "6506"],
         [ " הירקון 150, תל אביב", "32.0837404", "34.7693911", "9225", "6139"],
         [ " פרישמן 10, תל אביב", "32.0798034", "34.7688166", "6705", "6009"],
         [ " בוגרשוב 25, תל אביב", "32.0769875", "34.7698549", "10607", "5951"],
         [ " יפת 80, יפו (תל אביב)", "32.0464433", "34.7523983", "12889", "8511"],
         [ " דרך קיבוץ גלויות 30, תל אביב", "32.0517973", "34.7680502", "13038", "7756"],
         [ " לה גווארדיה 20, תל אביב", "32.059112", "34.788742", "7291", "5820"],
         [ " ארלוזורוב 90, תל אביב", "32.0854282", "34.7806379", "5603", "4724"],
         [ " כהנמן 60, בני ברק", "32.0845043", "34.8399999", "2550", "2213"],
         [ " נמיר 50, תל אביב", "32.0850486", "34.7953535", "3666", "3252"],
         [ " חיים עוזר 10, פתח תקווה", "32.0890587", "34.8861437", "7837", "6418"],
         [ " רוטשילד 50, פתח תקווה", "32.0903978", "34.8804676", "7675", "5723"],
         [ " ז'בוטינסקי 80, פתח תקווה", "32.091785", "34.8641828", "5401", "4110"],
         [ " העצמאות 20, פתח תקווה", "32.074777", "34.880136", "11278", "7102"],
         [ " אורלוב 30, פתח תקווה", "32.0934753", "34.8820009", "7199", "5893"],
         [ " שטמפפר 15, פתח תקווה", "32.0901497", "34.8855149", "7737", "6262"],
         [ " עין גנים 40, פתח תקווה", "32.086933", "34.894794", "8738", "7412"],
         [ " סלומון 12, פתח תקווה", "32.0846599", "34.8805418", "8231", "6309"],
         [ " פינסקר 8, פתח תקווה", "32.0907496", "34.8845311", "7920", "6310"],
         [ " אהרונוביץ' 12, בני ברק", "32.090832", "34.8386345", "2020", "1982"],
         [ " גיסין 25, פתח תקווה", "32.0976387", "34.8797159", "6395", "5893"],
         [ " סוקולוב 50, חולון", "32.0227089", "34.7745575", "11404", "10429"],
         [ " שנקר 20, חולון", "32.0261601", "34.7765943", "11279", "10237"],
         [ " דב הוז 30, חולון", "32.0236133", "34.7667955", "11864", "10636"],
         [ " שדרות קוגל 15, חולון", "32.0257767", "34.774554", "11779", "10077"],
         [ " הופיין 25, חולון", "32.0141212", "34.7685729", "12662", "11698"],
         [ " בלפור 40, בת ים", "32.0261767", "34.7449781", "14614", "11734"],
         [ " יוספטל 60, בת ים", "32.0165585", "34.7464493", "16092", "12857"],
         [ " העצמאות 30, בת ים", "32.0231832", "34.7465895", "14429", "11887"],
         [ " רוטשילד 20, בת ים", "32.026609", "34.7454099", "14696", "11652"],
         [ " השומר 5, בני ברק", "32.0815759", "34.8217171", "1970", "1509"],
         [ " אנילביץ' 10, בת ים", "32.0218369", "34.7517549", "13880", "12175"],
         [ " ז'בוטינסקי 40, ראשון לציון", "32.0246214", "34.7821867", "11871", "10753"],
         [ " הרצל 60, ראשון לציון", "31.9656402", "34.8025163", "19145", "18843"],
         [ " רוטשילד 30, ראשון לציון", "31.9641474", "34.803856", "21869", "19060"],
         [ " משה דיין 20, ראשון לציון", "32.0009662", "34.7665794", "16891", "13088"],
         [ " לוי אשכול 15, ראשון לציון", "31.9754762", "34.7770157", "20073", "16880"],
         [ " סוקולוב 40, הרצליה", "32.1669612", "34.844759", "16277", "9419"],
         [ " הרב קוק 20, הרצליה", "32.161374", "34.8408944", "8778", "8642"],
         [ " בן גוריון 30, הרצליה", "32.1613286", "34.8423012", "13900", "8531"],
         [ " שבעת הכוכבים 10, הרצליה", "32.1636245", "34.8243901", "12923", "9945"],
         [ " עזרא 25, בני ברק", "32.0782597", "34.8377103", "3640", "2686"],
         [ " אוסישקין 50, רמת השרון", "32.1422739", "34.8434807", "7103", "7016"],
         [ " סוקולוב 20, רמת השרון", "32.1398478", "34.8362248", "6149", "6062"],
         [ " ביאליק 15, רמת השרון", "32.137793", "34.840278", "6589", "6503"],
         [ " לוי אשכול 30, קריית אונו", "32.0661125", "34.8604378", "8484", "5571"],
         [ " שלמה המלך 20, קריית אונו", "32.0544159", "34.8603455", "8697", "7055"],
         [ " העצמאות 15, יהוד", "32.0745531", "34.8811245", "11185", "7358"],
         [ " ויצמן 10, יהוד", "32.0300273", "34.8924598", "18607", "11744"],
         [ " העצמאות 40, אור יהודה", "32.0289963", "34.8655017", "12632", "9877"],
         [ " אליהו סעדון 20, אור יהודה", "32.0255384", "34.8549182", "12238", "9905"]
};

}
