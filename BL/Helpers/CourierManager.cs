using BO;
using DalApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helpers;

/// <summary>
/// Manages courier-related operations in the business logic layer.
/// </summary>
/// <remarks>
/// This static class provides functionality for courier management including authentication,
/// CRUD operations, and delivery statistics tracking. It acts as an intermediary between
/// the business logic layer and the data access layer, performing necessary validations
/// and conversions.
/// </remarks>
internal static class CourierManager
{
    /// <summary>
    /// Data access layer instance for database operations.
    /// </summary>
    private static readonly IDal s_dal = Factory.Get;

    private static readonly Random s_rand = new();

    /// <summary>
    /// Observer manager for notifying UI components about courier changes.
    /// </summary>
    internal static readonly ObserverManager Observer = new();

    /// <summary>
    /// Authenticates a user by ID and password, determining if they are a manager or courier.
    /// </summary>
    /// <param name="id">The unique identifier of the user attempting to log in.</param>
    /// <param name="password">The password provided for authentication.</param>
    /// <returns>
    /// Returns "Manager" if the credentials match the manager account,
    /// or "Courier" if they match a courier account.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when a courier with the specified ID does not exist.
    /// </exception>
    /// <exception cref="BO.BlIncorrectPasswordException">
    /// Thrown when the password is incorrect for the given ID.
    /// </exception>
    /// <remarks>
    /// This method first checks if the ID matches the manager ID from configuration.
    /// If not, it attempts to authenticate as a courier.
    /// </remarks>
    internal static string? Login(int id, string password)
    {
        var config = AdminManager.GetConfig();

        // Check if user is manager
        if (id == config.ManagerId)
        {
            if (password == config.PasswordManager)
                return "Manager";
            throw new BO.BlIncorrectPasswordException();
        }

        DO.Courier? doCourier;
        // Check if user is courier
        lock (AdminManager.BlMutex)
        {
            doCourier = s_dal.Courier.Read(id)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");
        }

        if (doCourier.Password == password)
            return "Courier";

        throw new BO.BlIncorrectPasswordException();
    }

    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    /// <param name="boCourier">The business logic courier object containing courier details to create.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>The courier ID is not valid according to Israeli ID validation rules</description></item>
    ///   <item><description>The phone number format is invalid</description></item>
    ///   <item><description>The email address format is invalid</description></item>
    ///   <item><description>The password does not meet strength requirements</description></item>
    ///   <item><description>The maximum delivery distance exceeds the allowed limit</description></item>
    /// </list>
    /// </exception>
    /// <exception cref="BO.BlAlreadyExistsException">
    /// Thrown when a courier with the specified ID already exists in the system.
    /// </exception>
    /// <remarks>
    /// Password requirements: minimum 8 characters, including uppercase, lowercase, digit, and special character.
    /// </remarks>
    internal static void Create(BO.Courier boCourier)
    {
        s_validateCourierFields(boCourier, validateId: true);

        DO.Courier doCourier = s_convertToDataObject(boCourier);

        try
        {
            lock (AdminManager.BlMutex)
                s_dal.Courier.Create(doCourier);
        }
        catch (Exception ex)
        {
            throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists", ex);
        }

        Observer.NotifyListUpdated();
    }

    /// <summary>
    /// Retrieves a specific courier by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to retrieve.</param>
    /// <returns>
    /// A <see cref="BO.Courier"/> object with complete courier details including delivery statistics
    /// and current order in progress.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the courier with the specified ID does not exist.
    /// </exception>
    /// <remarks>
    /// This method enriches the courier data with calculated fields:
    /// <list type="bullet">
    ///   <item><description>DeliveryOnTime: Count of deliveries completed within the maximum delivery time</description></item>
    ///   <item><description>DeliveryLate: Count of deliveries completed after the maximum delivery time</description></item>
    ///   <item><description>OrderInProgress: Details of the current active delivery, if any</description></item>
    /// </list>
    /// </remarks>
    //internal static async Task<BO.Courier?> Read(int id)
    //{
    //    DO.Courier doCourier;
    //    lock (AdminManager.BlMutex)
    //        doCourier = s_dal.Courier.Read(id)
    //            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

    //    return await s_convertToBObject(doCourier);
    //}
    public static async IAsyncEnumerable<BO.Courier> Read(int id)
    {
        // 1. שליפת הנתונים הגולמיים (מהירה ובטוחה בתוך נעילה)
        DO.Courier doCourier;
        IEnumerable<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
        {
            doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

            allDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == id).ToList();
        }

        // 2. בניית האובייקט הבסיסי (ללא חישובים כבדים)
        var boCourier = new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
            Active = doCourier.Active,
            MaxDistanceDelivery = doCourier.MaxDistanceDelivery,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment,
            WorkingSince = doCourier.WorkingSince,
            DeliveryOnTime = s_getDeliveryOnTimeCount(allDeliveries),
            DeliveryLate = s_getDeliveryLateCount(allDeliveries),
            OrderInProgress = null // בינתיים ריק
        };

        // === הזרמה ראשונה: נתונים בסיסיים ל-UI ===
        yield return boCourier;

        // 3. בדיקה האם יש צורך בחישוב כבד (רק אם יש משלוח פעיל)
        var activeDelivery = allDeliveries.FirstOrDefault(d => d.EndDelivery == null);

        if (activeDelivery != null)
        {
            // ביצוע החישוב הכבד (פנייה לגוגל וכו')
            // שים לב: זה קורה מחוץ לנעילה הראשית כי s_getOrderInProgress מטפל בנעילות בעצמו איפה שצריך
            var heavyOrderDetails = await s_getOrderInProgress(allDeliveries);

            // עדכון האובייקט הקיים
            boCourier.OrderInProgress = heavyOrderDetails;

            // === הזרמה שנייה: נתונים מלאים ===
            yield return boCourier;
        }
    }
//#####################################################################################################################תוספת שלי לחישוב קל
    internal static BO.Courier? Read(int id, string light)
    {
        DO.Courier doCourier;
        lock (AdminManager.BlMutex)
            doCourier = s_dal.Courier.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

        IEnumerable<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
            allDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.Id).ToList();

        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
            Active = doCourier.Active,
            MaxDistanceDelivery = doCourier.MaxDistanceDelivery,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment,
            WorkingSince = doCourier.WorkingSince,
            DeliveryOnTime = s_getDeliveryOnTimeCount(allDeliveries),
            DeliveryLate = s_getDeliveryLateCount(allDeliveries),
            OrderInProgress = new OrderInProgress()
            {
                DeliveryId = -1,
                OrderId = -1,
                TypeOfOrder =BO.TypeOfOrder.STANDART,
                Address = "",
                Distance = 0,
                CustomerName = "",
                CustomerPhone = "",
                OrderTime = DateTime.MinValue,
                StartDeliveryTime = DateTime.MinValue,
                EstimatedDeliveryTime = DateTime.MinValue,
                MaxDeliveryTime = DateTime.MinValue,
                OrderStatus = BO.OrderStatus.OPEN ,
                ScheduleStatus = BO.ScheduleStatus.ONTYME ,
                TimeRemaining = TimeSpan.Zero
            }
        };
    }
//###############################################################################################################עד כאן

    /// <summary>
    /// Updates an existing courier's information in the system.
    /// </summary>
    /// <param name="requesterId">
    /// The ID of the user requesting the update. Determines if Active status can be changed.
    /// </param>
    /// <param name="boCourier">The business logic courier object with updated information.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the courier with the specified ID does not exist.
    /// </exception>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when phone number, email, password, or max distance is invalid.
    /// </exception>
    /// <remarks>
    /// <para>Only managers can change the Active status of a courier.</para>
    /// <para>The WorkingSince date is preserved from the original record.</para>
    /// </remarks>
    internal static void Update(int requesterId, BO.Courier boCourier)
    {
        bool isManager = requesterId == AdminManager.GetConfig().ManagerId;

        DO.Courier existingCourier;
        lock (AdminManager.BlMutex)
            existingCourier = s_dal.Courier.Read(boCourier.Id)
            ?? throw new BO.BlDoesNotExistException(
                $"Courier with ID={boCourier.Id} does not exist");

        s_validateCourierFields(boCourier, validateId: false);

        DO.Courier doCourier = new DO.Courier
        {
            Id = existingCourier.Id,
            Name = boCourier.Name,
            Phone = boCourier.Phone,
            Email = boCourier.Email,
            Password = boCourier.Password,
            Active = isManager ? boCourier.Active : existingCourier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = existingCourier.WorkingSince // Preserve original date
        };

        try
        {
            lock (AdminManager.BlMutex)
                s_dal.Courier.Update(doCourier);
            Observer.NotifyItemUpdated(boCourier.Id);
            Observer.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {boCourier.Id} not found", ex);
        }
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the courier with the specified ID does not exist.
    /// </exception>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>The courier has an active delivery in progress</description></item>
    ///   <item><description>The courier has completed deliveries in the past (historical records exist)</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// A courier can only be deleted if they have never made any deliveries.
    /// This preserves data integrity and delivery history.
    /// </remarks>
    internal static void Delete(int id)
    {
        lock (AdminManager.BlMutex)
            _ = s_dal.Courier.Read(id)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist");

        s_validateCourierCanBeDeleted(id);

        lock (AdminManager.BlMutex)
            s_dal.Courier.Delete(id);
        Observer.NotifyItemUpdated(id);
        Observer.NotifyListUpdated();
    }

    /// <summary>
    /// Retrieves all couriers with optional filtering and sorting.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list (used for authorization).</param>
    /// <param name="isActive">
    /// Optional filter for courier active status:
    /// <list type="bullet">
    ///   <item><description>true: Returns only active couriers</description></item>
    ///   <item><description>false: Returns only inactive couriers</description></item>
    ///   <item><description>null: Returns all couriers regardless of status</description></item>
    /// </list>
    /// </param>
    /// <param name="sort">The field to sort results by. Defaults to Id.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.CourierInList"/> objects
    /// containing courier summary information including delivery statistics.
    /// </returns>
    internal static async Task<IEnumerable<BO.CourierInList>> ReadAll(
     int requesterId,
     bool? isActive,
     BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        IEnumerable<DO.Courier> couriers;
        IEnumerable<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
            couriers = s_dal.Courier.ReadAll(c => isActive == null || c.Active == isActive).ToList();
        lock (AdminManager.BlMutex)
            allDeliveries = s_dal.Delivery.ReadAll().ToList();

        var deliveriesByCourier = allDeliveries.ToLookup(d => d.CourierId);

        var sortedCouriers = s_sortCouriers(couriers, sort);

        var resultList = sortedCouriers.Select(c =>
            s_convertToCourierInList(c, deliveriesByCourier[c.Id])
        );

        return await Task.Run(() => resultList.ToList());
    }

    /// <summary>
    /// Validates all required fields of a courier.
    /// </summary>
    /// <param name="courier">The courier to validate.</param>
    /// <param name="validateId">Whether to validate the ID (only needed for creation).</param>
    /// <exception cref="BO.BlInvalidValueException">Thrown when any field is invalid.</exception>
    private static void s_validateCourierFields(BO.Courier courier, bool validateId)
    {
        if (validateId && !Tools.IsValidId(courier.Id))
            throw new BO.BlInvalidValueException(courier.Id);

        if (!Tools.IsValidPhone(courier.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");

        if (!Tools.IsValidEmail(courier.Email))
            throw new BO.BlInvalidValueException("Invalid email address.");

        if (!Tools.IsStrongPassword(courier.Password))
            throw new BO.BlInvalidValueException(
                "Password too weak. Required: 8 characters with uppercase, lowercase, digit, and special character.");

        if (!Tools.IsValidDistens(courier.MaxDistanceDelivery ?? 0))
            throw new BO.BlInvalidValueException("Maximum delivery distance exceeds the allowed limit.");
    }

    /// <summary>
    /// Validates that a courier can be safely deleted.
    /// </summary>
    /// <param name="courierId">The courier ID to validate.</param>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when the courier has active or historical deliveries.
    /// </exception>
    private static void s_validateCourierCanBeDeleted(int courierId)
    {
        IEnumerable<DO.Delivery>? courierDeliveries;
        lock (AdminManager.BlMutex)
            courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId).ToList();

        if (!courierDeliveries.Any())
            return;

        // Check for active deliveries
        bool hasActiveDelivery = courierDeliveries.Any(delivery =>
        {
            DO.Order? order;
            lock (AdminManager.BlMutex)
                order = s_dal.Order.Read(delivery.OrderId);
            return order != null && order.OrderStatus == DO.OrderStatus.DELIVERING;
        });

        if (hasActiveDelivery)
            throw new BO.BlInvalidOperationException("Cannot delete courier with active delivery.");

        throw new BO.BlInvalidOperationException("Cannot delete courier with delivery history.");
    }

    /// <summary>
    /// Converts a business object courier to a data object courier.
    /// </summary>
    /// <param name="boCourier">The business object to convert.</param>
    /// <returns>A data object courier.</returns>
    private static DO.Courier s_convertToDataObject(BO.Courier boCourier)
    {
        return new DO.Courier
        {
            Id = boCourier.Id,
            Name = boCourier.Name,
            Phone = boCourier.Phone,
            Email = boCourier.Email,
            Password = boCourier.Password,
            Active = boCourier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = boCourier.WorkingSince
        };
    }

    /// <summary>
    /// Converts a data object courier to a business object courier with enriched data.
    /// </summary>
    /// <param name="doCourier">The data object to convert.</param>
    /// <returns>A business object courier with calculated statistics.</returns>
    private static async Task<BO.Courier> s_convertToBObject(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> allDeliveries;

        lock (AdminManager.BlMutex)
            allDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == doCourier.Id).ToList();

        var orderInProgress = await s_getOrderInProgress(allDeliveries);

        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
            Active = doCourier.Active,
            MaxDistanceDelivery = doCourier.MaxDistanceDelivery,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment,
            WorkingSince = doCourier.WorkingSince,
            DeliveryOnTime = s_getDeliveryOnTimeCount(allDeliveries),
            DeliveryLate = s_getDeliveryLateCount(allDeliveries),
            OrderInProgress = orderInProgress
        };
    }

    /// <summary>
    /// Converts a data object courier to a CourierInList summary object.
    /// </summary>
    /// <param name="doCourier">The data object to convert.</param>
    /// <returns>A CourierInList object with summary information.</returns>
    private static BO.CourierInList s_convertToCourierInList(DO.Courier doCourier, IEnumerable<DO.Delivery> courierDeliveries)
    {
        var activeDelivery = courierDeliveries.FirstOrDefault(d => d.EndDelivery == null);

        return new BO.CourierInList
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Active = doCourier.Active,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment,
            WorkingSince = doCourier.WorkingSince,

            DeliveryOnTime = s_getDeliveryOnTimeCount(courierDeliveries),
            DeliveryLate = s_getDeliveryLateCount(courierDeliveries),

            DeliveryId = activeDelivery?.Id
        };
    }

    /// <summary>
    /// Sorts couriers by the specified field.
    /// </summary>
    /// <param name="couriers">The couriers to sort.</param>
    /// <param name="sort">The field to sort by.</param>
    /// <returns>Sorted enumerable of couriers.</returns>
    private static IEnumerable<DO.Courier> s_sortCouriers(
        IEnumerable<DO.Courier> couriers,
        BO.CourierFieldSort? sort)
    {
        return sort switch
        {
            BO.CourierFieldSort.Name => couriers.OrderBy(c => c.Name),
            BO.CourierFieldSort.Phone => couriers.OrderBy(c => c.Phone),
            BO.CourierFieldSort.TypeShipment => couriers.OrderBy(c => c.TypeShipment),
            _ => couriers.OrderBy(c => c.Id)
        };
    }

    /// <summary>
    /// Calculates the number of on-time deliveries completed by a courier.
    /// </summary>
    /// <param name="doCourier">The courier to calculate statistics for.</param>
    /// <returns>The count of deliveries completed within the maximum allowed delivery time.</returns>
    /// <remarks>
    /// A delivery is considered on-time if the time between OrderDate and TimeEndDelivery
    /// is less than or equal to the MaxDeliveryTime configured in the system.
    /// Only deliveries with EndDelivery status of DELIVERED are counted.
    /// </remarks>
    private static int s_getDeliveryOnTimeCount(IEnumerable<DO.Delivery>? courierDeliveries)
    {
        var maxDeliveryTime = AdminManager.GetConfig().MaxDeliveryTime;

        if (courierDeliveries is null)
            return 0;

        return courierDeliveries.Count(d =>
            d.EndDelivery == DO.EndDelivery.DELIVERED &&
            (d.TimeEndDelivery - d.OrderDate) <= maxDeliveryTime
        );
    }

    /// <summary>
    /// Calculates the number of late deliveries completed by a courier.
    /// </summary>
    /// <param name="doCourier">The courier to calculate statistics for.</param>
    /// <returns>The count of deliveries completed after the maximum allowed delivery time.</returns>
    /// <remarks>
    /// A delivery is considered late if the time between OrderDate and TimeEndDelivery
    /// exceeds the MaxDeliveryTime configured in the system.
    /// Only deliveries with EndDelivery status of DELIVERED are counted.
    /// </remarks>
    private static int s_getDeliveryLateCount(IEnumerable<DO.Delivery>? courierDeliveries)
    {
        var maxDeliveryTime = AdminManager.GetConfig().MaxDeliveryTime;

        if (courierDeliveries is null)
            return 0;

        return courierDeliveries.Count(d =>
            d.EndDelivery == DO.EndDelivery.DELIVERED &&
            (d.TimeEndDelivery - d.OrderDate) > maxDeliveryTime
        );
    }

    /// <summary>
    /// Retrieves the current order in progress for a specific courier.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier.</param>
    /// <returns>
    /// An <see cref="BO.OrderInProgress"/> object if the courier has an active delivery,
    /// or null if no delivery is currently in progress.
    /// </returns>
    /// <remarks>
    /// This method searches for deliveries that:
    /// <list type="bullet">
    ///   <item><description>Are assigned to the specified courier</description></item>
    ///   <item><description>Have not yet ended (EndDelivery is null)</description></item>
    ///   <item><description>Are associated with an order in DELIVERING status</description></item>
    /// </list>
    /// </remarks>
    private static async Task<BO.OrderInProgress?> s_getOrderInProgress(IEnumerable<DO.Delivery> deliveries)
    {

        if (!deliveries.Any())
            return null;

        var activeDelivery = deliveries.FirstOrDefault(d => d.EndDelivery == null);

        return activeDelivery is null
            ? null
            : await s_createOrderInProgress(activeDelivery);
    }

    /// <summary>
    /// Creates an OrderInProgress object from a delivery record.
    /// </summary>
    /// <param name="delivery">The delivery record to convert.</param>
    /// <returns>An <see cref="BO.OrderInProgress"/> object with complete delivery information.</returns>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the order or courier associated with the delivery is not found.
    /// </exception>
    /// <remarks>
    /// This method retrieves the associated order and calculates:
    /// <list type="bullet">
    ///   <item><description>Distance from store to delivery address</description></item>
    ///   <item><description>Estimated delivery time based on courier speed and distance</description></item>
    ///   <item><description>Maximum allowed delivery time</description></item>
    ///   <item><description>Schedule status (on-time, at-risk, or late)</description></item>
    ///   <item><description>Time remaining until the delivery deadline</description></item>
    /// </list>
    /// </remarks>
    private static async Task<BO.OrderInProgress> s_createOrderInProgress(DO.Delivery delivery)
    {
        DO.Order order;
        lock (AdminManager.BlMutex)
            order = s_dal.Order.Read(delivery.OrderId)
            ?? throw new BO.BlDoesNotExistException("Order not found");

        DO.Courier courier;
        lock (AdminManager.BlMutex)
            courier = s_dal.Courier.Read(delivery.CourierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        var estimatedTimeTask = Tools.GetEstimatedDeliveryTime(delivery);


        var actualDistanceTask = GoogleMapsService.NetworkKeeper(() =>
            GoogleMapsService.GetActualDistance(
            order.Latitude,
            order.Longitude,
            (BO.TheTypeShipment)courier.TypeShipment));

        var scheduleStatusTask = Tools.GetScheduleStatus(order, delivery);

        await Task.WhenAll(estimatedTimeTask, actualDistanceTask, scheduleStatusTask);

        TimeSpan estimatedDeliveryTime = await estimatedTimeTask ?? TimeSpan.Zero;
        var actualDistance = await actualDistanceTask;
        var scheduleStatus = await scheduleStatusTask;

        DateTime maxDeliveryTime;
        lock (AdminManager.BlMutex)
            maxDeliveryTime = order.OrderDate.Add(s_dal.Config.MaxDeliveryTime);

        TimeSpan timeRemaining;

        timeRemaining = maxDeliveryTime - AdminManager.Now;

        // 4. יצירת האובייקט
        return new BO.OrderInProgress
        {
            DeliveryId = delivery.Id,
            OrderId = delivery.OrderId,
            TypeOfOrder = (BO.TypeOfOrder)order.TypeOfOrder,
            Details = order.Details,
            Address = order.Addres,
            Distance = Tools.GetDistance(order),
            ActualDistance = actualDistance,
            CustomerName = order.Name,
            CustomerPhone = order.Phone,
            OrderTime = order.OrderDate,
            StartDeliveryTime = delivery.OrderDate,
            EstimatedDeliveryTime = delivery.OrderDate + estimatedDeliveryTime,
            MaxDeliveryTime = maxDeliveryTime,
            OrderStatus = BO.OrderStatus.DELIVERING,
            ScheduleStatus = scheduleStatus,
            TimeRemaining = timeRemaining
        };
    }

    public static async Task CourierSimulation()
    {
        int managerId = AdminManager.GetConfig().ManagerId;

        var allCouriers = await ReadAll(managerId, true, BO.CourierFieldSort.Id)
            ?? new List<BO.CourierInList>();

        bool anyListChange = false;

        var simulationTasks = new List<Task>();

        foreach (var courier in allCouriers)
        {
            if (courier.DeliveryId is null)
            {
                if (s_rand.Next(1, 100) <= 100)
                {
                    simulationTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var openOrders = await DeliveryManager.GetOpen(courier.Id, null, null);

                            if (openOrders != null && openOrders.Any())
                            {
                                var randomOrder = openOrders[s_rand.Next(openOrders.Count)];

                                if (s_rand.Next(1, 100) <= 50 && randomOrder is BO.OpenOrderInList order)
                                {
                                    await DeliveryManager.StartDelivery(courier.Id, order.OrderId);
                                    anyListChange = true;
                                }
                            }
                        }
                        catch { }
                    }));
                }
            }
            else
            {
                int currentDeliveryId = courier.DeliveryId.Value;

                simulationTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        DO.Delivery? delivery;
                        lock (AdminManager.BlMutex)
                            delivery = s_dal.Delivery.Read(currentDeliveryId);

                        if (delivery is not null && delivery.EndDelivery == null)
                        {
                            TimeSpan? duration = await Tools.GetEstimatedDeliveryTime(delivery);

                            if (duration is not null)
                            {
                                DateTime estimatedArrival = delivery.OrderDate + duration.Value;

                                if (AdminManager.Now >= estimatedArrival)
                                {
                                    int chance = s_rand.Next(1, 100);
                                    BO.EndDelivery endStatus;

                                    if (chance <= 5) endStatus = BO.EndDelivery.REFUSED;       // 5%
                                    else if (chance <= 20) endStatus = BO.EndDelivery.NOTFOUND; // 15%
                                    else endStatus = BO.EndDelivery.DELIVERED;                  // 80%

                                    s_completeDeliveryNotObserv(courier.Id, currentDeliveryId, endStatus);
                                    anyListChange = true;
                                }
                                else if (s_rand.Next(1, 100) <= 10)
                                {
                                    await OrderManager.Cancel(delivery.OrderId);
                                    anyListChange = true;
                                }
                            }
                        }
                    }
                    catch { }
                }));
            }
        }
        await Task.WhenAll(simulationTasks);

        if (anyListChange)
        {
            Observer.NotifyListUpdated();
            OrderManager.Observer.NotifyListUpdated();
        }
    }

    private static void s_completeDeliveryNotObserv(int courierId, int deliveryId, BO.EndDelivery endDelivery)
    {
        try
        {
            DO.Delivery? delivery;
            {
                lock (AdminManager.BlMutex)
                    delivery = s_dal.Delivery.Read(deliveryId);

                if (delivery != null)
                {
                    var updatedDelivery = delivery with
                    {
                        EndDelivery = (DO.EndDelivery)endDelivery,
                        TimeEndDelivery = AdminManager.Now
                    };


                    DeliveryManager.UpdateOrderStatusAfterDelivery(delivery.OrderId, endDelivery);
                    s_dal.Delivery.Update(updatedDelivery);


                    Observer.NotifyItemUpdated(courierId);
                    OrderManager.Observer.NotifyItemUpdated(delivery.OrderId);
                    DeliveryManager.Observer.NotifyItemUpdated(deliveryId);
                }
            }
        }
        catch (Exception) { }
    }

}