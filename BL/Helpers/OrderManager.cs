using BO;
using DalApi;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helpers;

/// <summary>
/// Provides business logic operations for managing orders in the delivery system.
/// </summary>
/// <remarks>
/// This static class handles all order-related operations including creation, retrieval,
/// updates, cancellations, and querying. It serves as an intermediary between the data
/// access layer and the business logic layer, performing necessary conversions and validations.
/// </remarks>
internal static class OrderManager
{
    /// <summary>
    /// Data access layer instance for database operations.
    /// </summary>
    private static readonly IDal s_dal = Factory.Get;

    /// <summary>
    /// Observer manager for notifying UI components about order changes.
    /// </summary>
    internal static readonly ObserverManager Observer = new();

    private class OrderCacheInfo
    {
        public BO.OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime MaxDeliveryTime { get; set; }
        public DateTime RiskThreshold { get; set; } // הרגע שבו ההזמנה הופכת ל-INRISK
        public DateTime LateThreshold { get; set; } // הרגע שבו ההזמנה הופכת ל-LATE
        public BO.ScheduleStatus? FinalScheduleStatus { get; set; }
        public DateTime? DeliveryTime { get; set; }
    }

    private static Dictionary<int, OrderCacheInfo>? _ordersCache = null;
    private static readonly object _cacheLock = new object(); // מנעול לסנכרון

    private static async Task InitCache()
    {
        lock (_cacheLock)
        {
            if (_ordersCache != null) return;
            _ordersCache = new Dictionary<int, OrderCacheInfo>();
        }

        List<DO.Order> allOrders;
        List<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
        {
            allOrders = s_dal.Order.ReadAll().ToList();
            allDeliveries = s_dal.Delivery.ReadAll().ToList();
        }

        var deliveriesMap = allDeliveries
            .GroupBy(d => d.OrderId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.Id).FirstOrDefault());

        var tempCache = new Dictionary<int, OrderCacheInfo>();

        foreach (var order in allOrders)
        {
            var delivery = deliveriesMap.GetValueOrDefault(order.Id);
            tempCache[order.Id] = s_createCacheInfo(order, delivery);
        }

        lock (_cacheLock)
        {
            _ordersCache = tempCache;
        }

        await Task.CompletedTask;
    }

    internal static void ResetCache()
    {
        lock (_cacheLock)
        {
            _ordersCache = null; // איפוס המילון - הוא ייבנה מחדש בקריאה הבאה
        }
    }



    public static async Task<int[]> GetAllOrderStatistic()
    {
        // אתחול חד פעמי אם צריך
        if (_ordersCache == null) await InitCache();

        bool isEmpty;
        lock (_cacheLock)
        {
            isEmpty = _ordersCache != null && _ordersCache.Count == 0;
        }

        if (isEmpty)
        {
            lock (_cacheLock)
            {
                _ordersCache = null; 
            }
            await InitCache(); 
        }

        int maxStatusVal = (int)Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>().Max();
        int maxScheduleVal = (int)Enum.GetValues(typeof(BO.ScheduleStatus)).Cast<BO.ScheduleStatus>().Max();
        int[] results = new int[maxStatusVal + 1 + maxScheduleVal + 1];

        DateTime now = AdminManager.Now; // שמירת הזמן הנוכחי לחישוב
        TimeSpan riskRange = AdminManager.GetConfig().RiskRange; // קריאה אחת לקונפיג

        lock (_cacheLock)
        {
            if (_ordersCache == null) return results; // הגנה

            foreach (var item in _ordersCache.Values)
            {
                // 1. ספירת סטטוס הזמנה (פשוט ומהיר)
                results[(int)item.Status]++;

                // 2. ספירת סטטוס לו"ז
                BO.ScheduleStatus currentScheduleStatus;

                if (item.FinalScheduleStatus.HasValue)
                {
                    // אם זה שמור (הזמנה סגורה) - קח מהמטמון
                    currentScheduleStatus = item.FinalScheduleStatus.Value;
                }
                else
                {
                    // אם ההזמנה פתוחה - חשב בזיכרון (פעולה מתמטית פשוטה וללא DB)
                    TimeSpan timeLeft = item.MaxDeliveryTime - now;

                    // לוגיקה מקוצרת לחישוב מצב
                    if (timeLeft < TimeSpan.Zero)
                        currentScheduleStatus = BO.ScheduleStatus.LATE;
                    else if (timeLeft <= riskRange)
                        currentScheduleStatus = BO.ScheduleStatus.INRISK;
                    else
                        currentScheduleStatus = BO.ScheduleStatus.ONTYME;
                }

                results[maxStatusVal + 1 + (int)currentScheduleStatus]++;
            }
        }

        return results;
    }

    /// <summary>
    /// Creates a new order in the system.
    /// </summary>
    /// <param name="boOrder">The business object representing the order to create.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when order details, address, name, or phone number are invalid or empty,
    /// or when geocoding the address fails.
    /// </exception>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when the delivery distance exceeds the maximum allowed range.
    /// </exception>
    /// <remarks>
    /// <para>Validates all required fields and geocodes the delivery address before creation.</para>
    /// <para>The order date is automatically set to the current system time.</para>
    /// <para>The order ID is auto-generated by the data layer.</para>
    /// </remarks>
    public static async Task Create(BO.Order boOrder)
    {
        s_validateOrderFields(boOrder);

        var config = AdminManager.GetConfig();

        var addressCoordinates = await GoogleMapsService.GetGeocodingAsync(boOrder.Addres, config.GoogleApiKey);

        var distance = Tools.GetDistance(
            addressCoordinates?.Lat ?? 0,
            addressCoordinates?.Lng ?? 0,
            config.Latitude ?? 0,
            config.Longitude ?? 0);

        if (distance == 0 || distance > config.MaxDeliveryRange)
            throw new BO.BlInvalidOperationException(
                $"The distance {distance} KM exceeds the delivery range {config.MaxDeliveryRange} KM");

        DO.Order doOrder = new DO.Order
        {
            Id = 0,
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,
            Latitude = addressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address."),
            Longitude = addressCoordinates?.Lng ?? throw new BO.BlInvalidValueException("Failed to geocode address."),
            Name = boOrder.Name,
            Phone = boOrder.Phone,
            Weight = boOrder.Weight,
            OrderDate = AdminManager.Now,
            DistanceKm = distance,
        };

        lock (AdminManager.BlMutex)
            s_dal.Order.Create(doOrder);

        UpdateCacheItem(doOrder.Id);

        Observer.NotifyListUpdated();

        s_sendEmailNewOrder(doOrder);

    }

    /// <summary>
    /// Retrieves a specific order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to retrieve.</param>
    /// <returns>
    /// A business logic Order object with complete details including calculated fields,
    /// or null if no order with the specified ID exists.
    /// </returns>
    /// <remarks>
    /// This method enriches the data layer order with calculated fields such as distance,
    /// estimated delivery time, order status, and associated delivery attempts.
    /// </remarks>
    public static async Task<BO.Order?> Read(int id)
    {
        DO.Order? doOrder;
        lock (AdminManager.BlMutex)
            doOrder = s_dal.Order.Read(id);
        if (doOrder is null)
            return null;

        var deliveryPerOrderInLists = s_createDeliveryPerOrderInList(doOrder.Id);
        DateTime? estimatedDeliveryTime = await s_calculateEstimatedDeliveryTime(doOrder, deliveryPerOrderInLists);

        return new BO.Order
        {
            Id = doOrder.Id,
            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
            Details = doOrder.Details,
            Latitude = doOrder.Latitude,
            Longitude = doOrder.Longitude,
            Addres = doOrder.Addres,
            Name = doOrder.Name,
            Phone = doOrder.Phone,
            Weight = doOrder.Weight,
            OrderDate = doOrder.OrderDate,
            Distance = Tools.GetDistance(doOrder),
            EstimatedDeliveryTime = estimatedDeliveryTime,
            MaxDeliveryTime = doOrder.OrderDate + AdminManager.GetConfig().MaxDeliveryTime,
            OrderStatus = (BO.OrderStatus)doOrder.OrderStatus,
            ScheduleStatus = await Tools.GetScheduleStatus(doOrder),
            TimeLeftForDelivery = Tools.GetTimeLeftForDelivery(doOrder, (BO.OrderStatus)doOrder.OrderStatus),
            DeliveryPerOrderInLists = deliveryPerOrderInLists
        };
    }

    /// <summary>
    /// Updates an existing order in the system.
    /// </summary>
    /// <param name="boOrder">The business logic order object with updated information.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when order details, address, name, or phone number are invalid or empty,
    /// or when geocoding the address fails.
    /// </exception>
    /// <remarks>
    /// Validates all required fields including phone number and updates address coordinates
    /// by geocoding the address. If geocoding fails, an exception is thrown.
    /// </remarks>
    public static async Task Update(BO.Order boOrder)
    {
        s_validateOrderFields(boOrder);

        var apiKey = AdminManager.GetConfig().GoogleApiKey;
        var addressCoordinates = await GoogleMapsService.GetGeocodingAsync(boOrder.Addres, apiKey);

        DO.Order doOrder = new DO.Order
        {
            Id = boOrder.Id,
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,
            Latitude = addressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address."),
            Longitude = addressCoordinates?.Lng ?? throw new BO.BlInvalidValueException("Failed to geocode address."),
            Name = boOrder.Name,
            Phone = boOrder.Phone,
            Weight = boOrder.Weight,
            OrderDate = boOrder.OrderDate,
        };
        lock (AdminManager.BlMutex)
            s_dal.Order.Update(doOrder);

        UpdateCacheItem(doOrder.Id);

        Observer.NotifyItemUpdated(boOrder.Id);
        Observer.NotifyListUpdated();
    }

    /// <summary>
    /// Attempts to delete an order from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <exception cref="BO.BlInvalidOperationException">Always thrown as orders cannot be deleted.</exception>
    /// <remarks>
    /// Order deletion is not permitted in this system. Orders should be cancelled instead
    /// using the <see cref="Cancel(int)"/> method.
    /// </remarks>
    public static void Delete(int id)
    {
        throw new BO.BlInvalidOperationException("Order cannot be deleted. Use Cancel instead.");
    }

    /// <summary>
    /// Retrieves all orders from the system with optional filtering and sorting.
    /// </summary>
    /// <param name="filter">The field to filter by, or null for no filtering.</param>
    /// <param name="filterValue">The value to match for the specified filter field.</param>
    /// <param name="orderBy">The field to sort by (default is OrderStatus).</param>
    /// <returns>A list of orders in list view format, filtered and sorted as specified.</returns>
    /// <remarks>
    /// Converts data layer orders to business logic OrderInList objects,
    /// applies the specified filter predicate, and sorts by the requested field.
    /// </remarks>
    public static async Task<List<BO.OrderInList>> ReadAll(
        BO.OrderInListField? filter,
        object? filterValue,
        BO.OrderInListField? orderBy = BO.OrderInListField.OrderStatus)
    {
        Func<BO.OrderInList, bool> filterPredicate;
        lock (AdminManager.BlMutex)
            filterPredicate = s_getFilterPredicate(filter, filterValue);

        return await ReadAll(filterPredicate, orderBy);
    }

    /// <summary>
    /// Retrieves all orders from the system with a custom filter predicate.
    /// </summary>
    /// <param name="customPredicate">Custom filter function, or null to include all orders.</param>
    /// <param name="orderBy">The field to sort by (default is OrderId).</param>
    /// <returns>A list of orders in list view format, filtered and sorted as specified.</returns>
    public static async Task<List<BO.OrderInList>> ReadAll(
        Func<BO.OrderInList, bool>? customPredicate = null,
        BO.OrderInListField? orderBy = BO.OrderInListField.OrderId)
    {
        Dictionary<int, List<DO.Delivery>>? deliveriesMap;

        List<DO.Order> allDoOrders;

        lock (AdminManager.BlMutex)
        {
            deliveriesMap = s_dal.Delivery.ReadAll()
                .GroupBy(d => d.OrderId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.Id).ToList());
        }

        lock (AdminManager.BlMutex)
            allDoOrders = s_dal.Order.ReadAll().ToList();


        Func<BO.OrderInList, bool> filter = customPredicate ?? (_ => true);
        Func<BO.OrderInList, object> sortSelector = s_getSortSelector(orderBy);

        var conversionTasks = allDoOrders.Select(doOrder =>
            s_convertToBoOrderOptimized(doOrder, deliveriesMap)
        );

        var allBoOrders = await Task.WhenAll(conversionTasks);
        var result = allBoOrders
            .Where(filter)
            .OrderBy(sortSelector);

        return [.. result];
    }


    /// <summary>
    /// Cancels an existing order based on its current status.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to cancel.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the order or associated delivery is not found.
    /// </exception>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when the order cannot be cancelled due to its current status.
    /// </exception>
    /// <remarks>
    /// Cancellation behavior depends on the order status:
    /// <list type="bullet">
    ///   <item><description>OPEN/REFUSED: Creates a cancellation delivery record</description></item>
    ///   <item><description>DELIVERING: Updates the current delivery with cancellation status and notifies courier via email</description></item>
    ///   <item><description>COMPLETED/CANCELLED: Cannot be cancelled (throws exception)</description></item>
    /// </list>
    /// </remarks>
    public static async Task Cancel(int orderId, bool isSmsActive = false)
    {
        DO.Order doOrder;
        lock (AdminManager.BlMutex)
            doOrder = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException("הזמנה לא נמצאה לביטול");

        switch (doOrder.OrderStatus)
        {
            case DO.OrderStatus.COMPLETED:
                throw new BO.BlInvalidOperationException("לא ניתן לבטל הזמנה שנמסרה.");

            case DO.OrderStatus.CONCELLED:
                throw new BO.BlInvalidOperationException("לא ניתן לבטל, ההזמנה בוטלה בעבר.");

            case DO.OrderStatus.OPEN:

            case DO.OrderStatus.REFUSED:
                s_cancelOpenOrder(doOrder);
                break;

            case DO.OrderStatus.DELIVERING:
                try
                {
                    await s_cancelDeliveringOrder(doOrder, orderId, isSmsActive);
                }
                catch (BO.BLNoSendSmsException ex)
                {
                    throw new BO.BLNoSendSmsException(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                break;

            default:
                throw new BO.BlInvalidOperationException("Invalid order status.");
        }


        UpdateCacheItem(doOrder.Id);
        Observer.NotifyItemUpdated(orderId);
        Observer.NotifyListUpdated();
    }

    internal static void UpdateCacheItem(int orderId)
    {
        if (_ordersCache == null) return;

        DO.Order? doOrder;
        DO.Delivery? delivery;

        lock (AdminManager.BlMutex)
        {
            doOrder = s_dal.Order.Read(orderId);
            delivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId)
                                     .OrderByDescending(d => d.Id)
                                     .FirstOrDefault();
        }

        if (doOrder == null) return;

        var newItem = s_createCacheInfo(doOrder, delivery);

        lock (_cacheLock)
        {
            _ordersCache[orderId] = newItem;
        }
    }

    internal static bool CheckStatusChanges(DateTime oldClock, DateTime newClock)
{
    if (_ordersCache == null) return false;

    bool anyChange = false;

    lock (_cacheLock)
    {
        // רצים רק על המילון - אפס קריאות ל-DAL!
        foreach (var kvp in _ordersCache)
        {
            var id = kvp.Key;
            var info = kvp.Value;

            // מדלגים על הזמנות סגורות
            if (info.Status == BO.OrderStatus.COMPLETED || 
                info.Status == BO.OrderStatus.CANCELLED ||
                info.Status == BO.OrderStatus.REFUSED)
                continue;

            bool changed = false;

            // האם חצינו את קו הסיכון?
            if (oldClock < info.RiskThreshold && newClock >= info.RiskThreshold)
                changed = true;

            // האם חצינו את קו האיחור?
            else if (oldClock < info.LateThreshold && newClock >= info.LateThreshold)
                changed = true;

            if (changed)
            {
                Observer.NotifyItemUpdated(id); // עדכון נקודתי לממשק
                anyChange = true;
            }
        }
    }
    return anyChange;
}

    /// <summary>
    /// Validates all required fields of an order.
    /// </summary>
    /// <param name="order">The order to validate.</param>
    /// <exception cref="BO.BlInvalidValueException">Thrown when any required field is invalid.</exception>
    private static void s_validateOrderFields(BO.Order order)
    {
        if (string.IsNullOrEmpty(order.Name))
            throw new BO.BlInvalidValueException("Order name cannot be empty.");

        if (!Tools.IsValidPhone(order.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");

        if (string.IsNullOrEmpty(order.Addres))
            throw new BO.BlInvalidValueException("Order address cannot be empty.");

        if (string.IsNullOrEmpty(order.Details))
            throw new BO.BlInvalidValueException("Order details cannot be empty.");
    }

    /// <summary>
    /// Converts a data layer order to a business logic OrderInList object using pre-fetched deliveries.
    /// </summary>
    /// <param name="doOrder">The data layer order to convert.</param>
    /// <param name="deliveriesMap">Pre-fetched deliveries grouped by order ID.</param>
    /// <returns>An OrderInList object with calculated fields.</returns>
    private static async Task<BO.OrderInList> s_convertToBoOrderOptimized(
        DO.Order doOrder,
        Dictionary<int, List<DO.Delivery>> deliveriesMap)
    {
        List<DO.Delivery> orderDeliveries = deliveriesMap.TryGetValue(doOrder.Id, out var deliveries)
            ? deliveries
            : [];

        DO.Delivery? latestDelivery = orderDeliveries.FirstOrDefault();
        var orderStatus = (BO.OrderStatus)doOrder.OrderStatus;
        double distanceKm = doOrder.DistanceKm ?? Tools.GetDistance(doOrder);

        return new BO.OrderInList
        {
            OrderId = doOrder.Id,
            DeliveryId = latestDelivery?.Id,
            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
            DistanceKm = distanceKm,
            OrderStatus = orderStatus,
            ScheduleStatus = await Tools.GetScheduleStatus(doOrder, latestDelivery),
            TimeLeftForDelivery = Tools.GetTimeLeftForDelivery(doOrder, orderStatus),
            TotalTimeOfDelivery = Tools.GetTotalTimeOfDelivery(doOrder, orderStatus, latestDelivery?.TimeEndDelivery),
            NumberOfDeliveryAttempts = orderDeliveries.Count
        };
    }

    internal static BO.ScheduleStatus? TryGetCachedScheduleStatus(int orderId)
    {
        // אם המטמון לא קיים, אין מנוס מחישוב רגיל
        if (_ordersCache == null) return null;

        lock (_cacheLock)
        {
            if (_ordersCache.TryGetValue(orderId, out var info))
            {
                // אם ההזמנה סגורה, יש לנו סטטוס סופי שמור
                if (info.FinalScheduleStatus.HasValue)
                    return info.FinalScheduleStatus.Value;

                // אם ההזמנה פתוחה, המטמון לא מחזיק סטטוס לו"ז (כי הוא משתנה כל רגע)
                // ולכן נחזיר null כדי שהלוגיקה הרגילה תחשב אותו לפי הזמן הנוכחי
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// Creates a list of all delivery attempts associated with a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>
    /// A list of DeliveryPerOrderInList objects, or null if no deliveries exist.
    /// </returns>
    private static List<BO.DeliveryPerOrderInList>? s_createDeliveryPerOrderInList(int orderId)
    {
        List<DO.Delivery> deliveries;
        lock (AdminManager.BlMutex)
            deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId).ToList();

        if (!deliveries.Any()) return null;

        var courierIds = deliveries.Select(d => d.CourierId).Distinct();
        Dictionary<int, DO.Courier> couriers;

        lock (AdminManager.BlMutex)
            couriers = s_dal.Courier.ReadAll(c => courierIds.Contains(c.Id))
                           .ToDictionary(c => c.Id); // מילון לגישה מהירה

        // 3. יצירת הרשימה בזיכרון
        return deliveries.Select(d =>
        {
            var courier = couriers.GetValueOrDefault(d.CourierId);
            return new BO.DeliveryPerOrderInList
            {
                DeliveryId = d.Id,
                CourierId = d.CourierId,
                CourierName = courier != null ? courier.Name : string.Empty, // טיפול ב-Null למקרה קיצון
                TypeShipment = courier != null ? (BO.TheTypeShipment)courier.TypeShipment : BO.TheTypeShipment.FOOT,
                OrderDate = d.OrderDate,
                EndDelivery = d.EndDelivery.HasValue ? (BO.EndDelivery)d.EndDelivery : null,
                TimeEndDelivery = d.TimeEndDelivery
            };
        }).ToList();
    }

    /// <summary>
    /// Calculates the estimated delivery time for an order.
    /// </summary>
    /// <param name="doOrder">The data layer order.</param>
    /// <param name="deliveries">List of delivery attempts for the order.</param>
    /// <returns>The estimated delivery time, or null if not applicable.</returns>
    private static async Task<DateTime?> s_calculateEstimatedDeliveryTime(
        DO.Order doOrder,
        List<BO.DeliveryPerOrderInList>? deliveries)
    {
        if (deliveries is not List<BO.DeliveryPerOrderInList> orderInProgresses)
            return null;

        var activeDelivery = orderInProgresses
            .OrderByDescending(d => d.DeliveryId)
            .FirstOrDefault(d => d.EndDelivery == null);

        return activeDelivery != null
            ? activeDelivery.OrderDate + await Tools.GetEstimatedDeliveryTime(doOrder)
            : null;
    }

    /// <summary>
    /// Cancels an order that is in OPEN or REFUSED status.
    /// </summary>
    /// <param name="doOrder">The order to cancel.</param>
    private static void s_cancelOpenOrder(DO.Order doOrder)
    {
        doOrder = doOrder with { OrderStatus = DO.OrderStatus.CONCELLED };

        DO.Delivery delivery = new DO.Delivery
        {
            Id = 0,
            OrderId = doOrder.Id,
            CourierId = 0,
            TypeShipment = DO.TheTypeShipment.FOOT,
            OrderDate = AdminManager.Now,
            EndDelivery = DO.EndDelivery.CONCELLED,
            TimeEndDelivery = AdminManager.Now,
            ActualDistance = 0
        };
        lock (AdminManager.BlMutex) { 
            s_dal.Order.Update(doOrder);
            s_dal.Delivery.Create(delivery);
        }

        Observer.NotifyItemUpdated(doOrder.Id);
    }

    /// <summary>
    /// Cancels an order that is currently being delivered.
    /// </summary>
    /// <param name="doOrder">The order to cancel.</param>
    /// <param name="orderId">The order ID for notification purposes.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the delivery or courier is not found.
    /// </exception>
    private static async Task s_cancelDeliveringOrder(DO.Order doOrder, int orderId, bool IsSmsActive)
    {
        doOrder = doOrder with { OrderStatus = DO.OrderStatus.CONCELLED };
 

        DO.Delivery? delivery;
        lock (AdminManager.BlMutex)
            delivery = (from d in s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.Id)
                        orderby d.Id descending
                        select d).FirstOrDefault()
               ?? throw new BO.BlDoesNotExistException("לא נמצא משלוח עבור הזמנה זו");
        lock (AdminManager.BlMutex)
        {
            s_dal.Delivery.Update(delivery with
            {
                EndDelivery = DO.EndDelivery.CONCELLED,
                TimeEndDelivery = AdminManager.Now
            });
            s_dal.Order.Update(doOrder);
        }

        DO.Courier? courier;
        lock (AdminManager.BlMutex)
            courier = s_dal.Courier.Read(delivery.CourierId)
                ?? throw new BO.BlDoesNotExistException("Courier not found");

        Exception? exceptionMail = null;
        Exception? exceptionSms = null;
        try
        {
            await Tools.SendEmailSkript(
                  courier.Email,
                  $"{courier.Name}, ההזמנה בוטלה!!!",
                  $"הזמנה מספר {orderId} בוטלה על ידי המנהל.");

        }
        catch (BO.BLNoSendEmailException ex)
        {
            exceptionMail = new BO.BLNoSendEmailException($"Failed to send email notification {ex.Message}");
        }

        try
        {
            if (IsSmsActive)
            {
                await Tools.SendSms(
                      courier.Phone,
                      $"{courier.Name}, ההזמנה בוטלה!!!",
                      $"הזמנה מספר {orderId} בוטלה על ידי המנהל");
            }
        }
        catch (BO.BLNoSendSmsException)
        {
            exceptionSms = new BO.BLNoSendSmsException("Failed to send sms notification");
        }
        try
        {
            if ((exceptionMail is not null && !IsSmsActive) || (exceptionSms is not null && exceptionMail is not null))
                throw new BO.BLNoSendSmsException($"לא נשלחה הודעה כלל למוביל, {exceptionSms} {exceptionMail}");
        }

        finally
        {
            CourierManager.Observer.NotifyItemUpdated(delivery.CourierId);
            CourierManager.Observer.NotifyListUpdated();
            Observer.NotifyItemUpdated(orderId);
            Observer.NotifyListUpdated();
        }
    }

    private static async void s_sendEmailNewOrder(DO.Order doOrder)
    {

        Dictionary<int, int> deliveriesMap;

        lock (AdminManager.BlMutex)
            deliveriesMap = s_dal.Delivery.ReadAll(d => d.EndDelivery is null)
               .ToDictionary(d => d.CourierId, d => d.Id);

        List<DO.Courier> list_courier;

        lock (AdminManager.BlMutex)
            list_courier = s_dal?.Courier?.ReadAll(courier =>
             courier.Active == true &&
             s_matchTypeShipmentAndOrder(courier.TypeShipment, doOrder.TypeOfOrder) &&
             courier.MaxDistanceDelivery >= doOrder.DistanceKm &&
             !deliveriesMap.ContainsKey(courier.Id)) // סינון שליחים שאין להם משלוח פעיל
             ?.ToList()
                ?? new List<DO.Courier>();


        var (typeOfOrderebrew, emoje) = Tools.ConvertTipeOrderToHebrew(doOrder.TypeOfOrder);

        try
        {
            foreach (var courier in list_courier)
            {
                await GoogleMapsService.NetworkKeeper<object?>(async () =>
                {
                    await Tools.SendEmailSkript(courier.Email, "נכנסה הזמנה מתאימה עבורך ",
          $@"
          <div style='font-family:Lucida Sans Unicode; direction:rtl'>
          <h2>📦 איזה כיף! ראינו שיש הזמנה חדשה שמתאימה לך!</h2>
          <b>שלום {courier.Name} היקר!!!</b><br><br>

          <table style='border-collapse:collapse'>
          <tr><td><b>מספר הזמנה:</b></td><td>{doOrder.Id}</td></tr>
          <tr><td><b>שם:</b></td><td>{doOrder.Name}</td></tr>
          <tr><td><b>כתובת:</b></td><td>{doOrder.Addres}</td></tr>
          <tr><td><b>טלפון:</b></td><td>{doOrder.Phone}</td></tr>
          <tr><td><b>פרטים:</b></td><td>{doOrder.Details}</td></tr>
          <tr><td><b>סוג משלוח:</b></td><td>{typeOfOrderebrew}{emoje}</td></tr>
          <tr><td><b>משקל:</b></td><td>{doOrder.Weight}</td></tr>
          <tr><td><b>תאריך הזמנה:</b></td><td>{doOrder.OrderDate:dd/MM/yyyy HH:mm}</td></tr>
          </table>
          </div>
          "
                    );
                    return null;
                }
                );
            }
        }
        catch
        { }

    }



    /// <summary>
    /// Creates a filter predicate function based on the specified field and value.
    /// </summary>
    /// <param name="filter">The field to filter by, or null for no filtering.</param>
    /// <param name="filterValue">The value to match against the specified field.</param>
    /// <returns>A predicate function for filtering orders.</returns>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when a filter field is specified but no filter value is provided.
    /// </exception>
    private static Func<BO.OrderInList, bool> s_getFilterPredicate(
        BO.OrderInListField? filter,
        object? filterValue)
    {
        if (filter is not null && filterValue is null)
            throw new BO.BlInvalidValueException("Filter value must be provided when a filter field is specified.");

        return filter switch
        {
            BO.OrderInListField.OrderId => o => o.OrderId == (int)filterValue!,
            BO.OrderInListField.TypeOfOrder => o => o.TypeOfOrder == (BO.TypeOfOrder)filterValue!,
            BO.OrderInListField.OrderStatus => o => o.OrderStatus == (BO.OrderStatus)filterValue!,
            BO.OrderInListField.DistanceKm => o => o.DistanceKm == Convert.ToDouble(filterValue),
            BO.OrderInListField.ScheduleStatus => o => o.ScheduleStatus == (BO.ScheduleStatus)filterValue!,
            BO.OrderInListField.TimeLeftForDelivery => o => o.TimeLeftForDelivery <= (TimeSpan)filterValue!,
            BO.OrderInListField.TotalTimeOfDelivery => o => o.TotalTimeOfDelivery <= (TimeSpan)filterValue!,
            BO.OrderInListField.DeliveryAttempts => o => o.NumberOfDeliveryAttempts == (int)filterValue!,
            _ => _ => true
        };
    }

    /// <summary>
    /// Creates a sort selector function based on the specified field.
    /// </summary>
    /// <param name="sort">The field to sort by, or null to use default sorting.</param>
    /// <returns>A selector function that extracts the specified field value.</returns>
    private static Func<BO.OrderInList, object> s_getSortSelector(BO.OrderInListField? sort)
    {
        return sort switch
        {
            BO.OrderInListField.OrderId => o => o.OrderId,
            BO.OrderInListField.TypeOfOrder => o => o.TypeOfOrder,
            BO.OrderInListField.OrderStatus => o => o.OrderStatus,
            BO.OrderInListField.DistanceKm => o => o.DistanceKm,
            BO.OrderInListField.ScheduleStatus => o => o.ScheduleStatus,
            BO.OrderInListField.TimeLeftForDelivery => o => o.TimeLeftForDelivery,
            BO.OrderInListField.TotalTimeOfDelivery => o => o.TotalTimeOfDelivery,
            _ => o => o.OrderStatus
        };
    }

    private static bool s_matchTypeShipmentAndOrder(DO.TheTypeShipment courierType, DO.TypeOfOrder order)
    {
        return order switch
        {
            DO.TypeOfOrder.STANDART => true,
            DO.TypeOfOrder.FAST_DELIVERY => courierType == DO.TheTypeShipment.MOTORCYCLE || courierType == DO.TheTypeShipment.CAR,
            DO.TypeOfOrder.DELIVER_IMMEDIATELY => courierType == DO.TheTypeShipment.MOTORCYCLE,
            _ => false
        };
    }

    private static OrderCacheInfo s_createCacheInfo(DO.Order order, DO.Delivery? delivery)
    {
        var config = AdminManager.GetConfig();
        var status = Tools.GetOrderStatus(order, delivery);
        var maxTime = order.OrderDate + config.MaxDeliveryTime;

        // חישוב זמן מאמץ משוער (כמו ב-Tools, אבל מחושב פעם אחת בלבד)
        TimeSpan estimatedEffort;

        if (status == BO.OrderStatus.DELIVERING && delivery != null)
        {
            // אם במשלוח: מרחק חלקי מהירות קטנוע (או מהירות רכב ממוצעת אם רוצים לדייק יותר)
            double dist = delivery.ActualDistance ?? Tools.GetDistance(order);
            estimatedEffort = TimeSpan.FromHours(dist / config.AvgSpeedMotorcycle);
        }
        else // OPEN
        {
            // אם פתוח: מרחק אווירי חלקי הליכה
            double dist = Tools.GetDistance(order);
            estimatedEffort = TimeSpan.FromHours(dist / config.AvgSpeedFoot);
        }

        // חישוב נקודות הציון בזמן
        DateTime lateThreshold = maxTime - estimatedEffort;
        DateTime riskThreshold = lateThreshold - config.RiskRange;

        var info = new OrderCacheInfo
        {
            Status = status,
            OrderDate = order.OrderDate,
            MaxDeliveryTime = maxTime,
            RiskThreshold = riskThreshold,
            LateThreshold = lateThreshold,
            DeliveryTime = delivery?.TimeEndDelivery,
            FinalScheduleStatus = null
        };

        // אם סגור - מקבעים את הסטטוס הסופי
        if (status == BO.OrderStatus.COMPLETED)
        {
            info.FinalScheduleStatus = (delivery?.TimeEndDelivery <= maxTime)
                ? BO.ScheduleStatus.ONTYME
                : BO.ScheduleStatus.LATE;
        }
        else if (status == BO.OrderStatus.CANCELLED || status == BO.OrderStatus.REFUSED)
        {
            info.FinalScheduleStatus = BO.ScheduleStatus.CANCELLED;
        }

        return info;
    }

    public static Task UpdateDistanceForOrders()
    {
        return Task.Run(() =>
        {
            List<DO.Order> allOrders;

            lock (AdminManager.BlMutex)
                allOrders = s_dal.Order.ReadAll(o => o.OrderStatus == DO.OrderStatus.OPEN).ToList();

            var config = AdminManager.GetConfig();

            foreach (var order in allOrders)
            {
                try
                {
                    if (config.Latitude is double storeLat && config.Longitude is double storeLon)
                    {
                        double newDistance = Tools.GetDistance(storeLat, storeLon,
                            order.Latitude, order.Longitude);

                        lock (AdminManager.BlMutex)
                            s_dal.Order.Update(order with { DistanceKm = newDistance });

                    }
                    else
                        throw new BO.BlDoesNotExistException("כתובת חנות לא מעודכנת");
                }
                catch { }

            }
            Observer.NotifyListUpdated();
        });
    }
}

