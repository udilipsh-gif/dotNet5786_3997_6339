using BO;
using DalApi;
using System;

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
    private static readonly IDal s_dal = Factory.Get;

    internal static ObserverManager Observer = new();

    /// <summary>
    /// Authenticates a user by ID and password, determining if they are a manager or courier.
    /// </summary>
    /// <param name="id">The unique identifier of the user attempting to log in.</param>
    /// <param name="password">The password provided for authentication.</param>
    /// <returns>
    /// Returns "Manager" if the credentials match the manager account,
    /// or "Courier" if they match a courier account.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when a courier with the specified ID does not exist.</exception>
    /// <exception cref="BO.BlIncorrectPasswordException">Thrown when the password is incorrect for the given ID.</exception>
    /// <remarks>
    /// This method first checks if the ID matches the manager ID from configuration.
    /// If not, it attempts to authenticate as a courier.
    /// </remarks>
    internal static string? Login(int id, string password)
    {
        var _config = AdminManager.GetConfig();
        if (id == _config.ManagerId)
        {
            if (password == _config.PasswordManager)
                return "Manager";
            else
                throw new BO.BlIncorrectPasswordException();
        }

        DO.Courier? doCourier = s_dal.Courier.Read(id)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");

        if (doCourier.Password == password)
            return "Courier";
        else
            throw new BO.BlIncorrectPasswordException();
    }

    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    /// <param name="boCourier">The business logic courier object containing courier details to create.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when:
    /// - The courier ID is not valid according to Israeli ID validation rules
    /// - The phone number format is invalid
    /// - The email address format is invalid
    /// - The password does not meet strength requirements
    /// </exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown when a courier with the specified ID already exists in the system.</exception>
    /// <remarks>
    /// This method validates all courier information before creating the record:
    /// <list type="bullet">
    /// <item><description>ID must pass Israeli ID checksum validation</description></item>
    /// <item><description>Phone number must be in valid Israeli or international format</description></item>
    /// <item><description>Email must be in valid email format</description></item>
    /// <item><description>Password must meet strength requirements (uppercase, lowercase, digit, special character, min 8 chars)</description></item>
    /// </list>
    /// The courier is automatically set to the active status specified in the input.
    /// </remarks>
    internal static void Create(BO.Courier boCourier)
    {
        if (!Tools.IsValidId(boCourier.Id))
            throw new BO.BlInvalidValueException(boCourier.Id);
        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");
        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.BlInvalidValueException("Invalid email address.");
        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.BlInvalidValueException("סיסמה חלשה מידי. הכנס 8 תווים בהם אות גדולה, קטנה, ספרה וסימן.");

        DO.Courier doCourier = new DO.Courier
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
        try
        {
            s_dal.Courier.Create(doCourier);
        }
        catch (Exception ex)
        {
            throw new BO.BlAlreadyExistsException($"courier with id {boCourier.Id} is alredy exists", ex);
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
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    /// <remarks>
    /// This method enriches the courier data with calculated fields:
    /// <list type="bullet">
    /// <item><description>DeliveryOnTime: Count of deliveries completed within the maximum delivery time</description></item>
    /// <item><description>DeliveryLate: Count of deliveries completed after the maximum delivery time</description></item>
    /// <item><description>OrderInProgress: Details of the current active delivery, if any</description></item>
    /// </list>
    /// </remarks>
    internal static BO.Courier? Read(int id)
    {
        DO.Courier doCourier = s_dal.Courier.Read(id)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");

        BO.Courier boCourier = new BO.Courier
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
            DeliveryOnTime = s_getDeliveryOnTime(doCourier),
            DeliveryLate = s_getDeliveryLate(doCourier),
            OrderInProgress = s_getOrderInProgres(doCourier.Id)
        };
        return boCourier;
    }

    /// <summary>
    /// Updates an existing courier's information in the system.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the update. Determines if Active status can be changed.</param>
    /// <param name="boCourier">The business logic courier object with updated information.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when:
    /// - The phone number format is invalid
    /// - The email address format is invalid
    /// - The password does not meet strength requirements
    /// </exception>
    /// <remarks>
    /// This method validates all courier information before updating.
    /// Only managers can change the Active status of a courier.
    /// The WorkingSince date is preserved from the original record.
    /// </remarks>
    internal static void Update(int requesterId, BO.Courier boCourier)
    {
        bool manager = requesterId == AdminManager.GetConfig().ManagerId;

        DO.Courier courier = s_dal.Courier.Read(boCourier.Id)
            ?? throw new BO.BlDoesNotExistException(
                $"Courier with ID={boCourier.Id} does not exist, you can't update");

        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");

        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.BlInvalidValueException("Invalid email address.");

        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.BlInvalidValueException("Password is not strong enough.");

        DO.Courier doCourier = new DO.Courier
        {
            Id = courier.Id,
            Name = boCourier.Name,
            Phone = boCourier.Phone,
            Email = boCourier.Email,
            Password = boCourier.Password,
            Active = manager ? boCourier.Active : courier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = courier.WorkingSince
        };
        try
        {
            s_dal.Courier.Update(doCourier);
            Observer.NotifyItemUpdated(boCourier.Id);
            Observer.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BO.BlDoesNotExistException($"courier with id {boCourier.Id} is not found", ex);
        }

    }

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier with the specified ID does not exist.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the courier has active (non-delivered) deliveries.</exception>
    /// <remarks>
    /// This method performs the following validations before deletion:
    /// <list type="bullet">
    /// <item><description>Verifies that the courier exists in the system</description></item>
    /// <item><description>Checks that the courier has no active deliveries (deliveries that are not yet completed)</description></item>
    /// </list>
    /// A courier with pending or in-progress deliveries cannot be deleted.
    /// </remarks>
    internal static void Delete(int id)
    {
        DO.Courier? courier = s_dal.Courier.Read(id);
        if (courier == null)
            throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist, you can't delete");

        //IEnumerable<DO.Delivery> activeDeliveries = s_dal.Delivery.ReadAll(d =>
        //d.CourierId == id &&
        //(d.EndDelivery == null || d.EndDelivery != DO.EndDelivery.DELIVERED)
        //);
        //if (activeDeliveries.Any())
        //throw new BO.BlInvalidOperationException("Cannot delete courier with active deliveries.");

        //IEnumerable<DO.Delivery> allDeliveries = s_dal.Delivery.ReadAll(d =>
        //    d.CourierId == id
        //);
        //if (allDeliveries.Any())
        //    foreach (var item in allDeliveries)
        //    {
        //        var order = s_dal.Order.Read(item.OrderId);
        //        if (order != null && order.OrderStatus == DO.OrderStatus.DELIVERING)
        //            throw new BO.BlInvalidOperationException("קיים משלוח פעיל ");
        //        else
        //            throw new BO.BlInvalidOperationException("בוצעו משלוחים בעבר ");

        //    }

        var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == id);
        if (courierDeliveries.Any())
        {

            bool hasActiveDelivery = courierDeliveries.Any(d =>
            {
                var order = s_dal.Order.Read(d.OrderId);
                return order?.OrderStatus == DO.OrderStatus.DELIVERING;
            });

            if (hasActiveDelivery)
                throw new BO.BlInvalidOperationException(" .קיים משלוח פעיל לשליח זה");
            throw new BO.BlInvalidOperationException(" .לשליח זה בוצעו משלוחים בעבר");
        }
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
    /// - true: Returns only active couriers
    /// - false: Returns only inactive couriers
    /// - null: Returns all couriers regardless of status
    /// </param>
    /// <param name="sort">The field to sort results by. Defaults to Id.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.CourierInList"/> objects
    /// containing courier summary information including delivery statistics.
    /// </returns>
    /// <remarks>
    /// Each courier in the list includes:
    /// <list type="bullet">
    /// <item><description>Basic courier information (Id, Name, Active status, TypeShipment, WorkingSince)</description></item>
    /// <item><description>DeliveryOnTime: Count of on-time completed deliveries</description></item>
    /// <item><description>DeliveryLate: Count of late completed deliveries</description></item>
    /// <item><description>DeliveryId: The ID of the current active delivery, if any</description></item>
    /// </list>
    /// </remarks>
    internal static IEnumerable<BO.CourierInList> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        return s_dal.Courier.ReadAll(c => isActive == null || c.Active == isActive)
            .OrderBy(c => sort switch
            {
                BO.CourierFieldSort.Id => (IComparable)c.Id,
                BO.CourierFieldSort.Name => (IComparable)c.Name,
                BO.CourierFieldSort.Phone => (IComparable)c.Phone,
                BO.CourierFieldSort.TypeShipment => (IComparable)c.TypeShipment,
                _ => c.Id
            }).Select(c => new BO.CourierInList
            {
                Id = c.Id,
                Name = c.Name,
                Active = c.Active,
                TypeShipment = (BO.TheTypeShipment)c.TypeShipment,
                WorkingSince = c.WorkingSince,
                DeliveryOnTime = s_getDeliveryOnTime(c),
                DeliveryLate = s_getDeliveryLate(c),
                DeliveryId = s_getOrderInProgres(c.Id)?.DeliveryId
            });
    }

    /// <summary>
    /// Calculates the number of on-time deliveries completed by a courier.
    /// </summary>
    /// <param name="doCourier">The data object courier to calculate statistics for.</param>
    /// <returns>The count of deliveries completed within the maximum allowed delivery time.</returns>
    /// <remarks>
    /// A delivery is considered on-time if the time between OrderDate and TimeEndDelivery
    /// is less than or equal to the MaxDeliveryTime configured in the system.
    /// Only deliveries with EndDelivery status of DELIVERED are counted.
    /// </remarks>
    private static int s_getDeliveryOnTime(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&
               d.TimeEndDelivery - d.OrderDate <= AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }

    /// <summary>
    /// Calculates the number of late deliveries completed by a courier.
    /// </summary>
    /// <param name="doCourier">The data object courier to calculate statistics for.</param>
    /// <returns>The count of deliveries completed after the maximum allowed delivery time.</returns>
    /// <remarks>
    /// A delivery is considered late if the time between OrderDate and TimeEndDelivery
    /// exceeds the MaxDeliveryTime configured in the system.
    /// Only deliveries with EndDelivery status of DELIVERED are counted.
    /// </remarks>
    private static int s_getDeliveryLate(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesLate = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&
               d.TimeEndDelivery - d.OrderDate > AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesLate.Count();
    }

    /// <summary>
    /// Creates an OrderInProgress object from a delivery record.
    /// </summary>
    /// <param name="delivery">The delivery record to convert.</param>
    /// <returns>An <see cref="BO.OrderInProgress"/> object containing complete delivery and order information.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order associated with the delivery is not found.</exception>
    /// <remarks>
    /// This method retrieves the associated order and calculates:
    /// <list type="bullet">
    /// <item><description>Distance from store to delivery address</description></item>
    /// <item><description>Estimated delivery time based on courier speed and distance</description></item>
    /// <item><description>Maximum allowed delivery time</description></item>
    /// <item><description>Schedule status (on-time, at-risk, or late)</description></item>
    /// <item><description>Time remaining until the delivery deadline</description></item>
    /// </list>
    /// </remarks>
    private static BO.OrderInProgress s_createOrderInProgress(DO.Delivery delivery)
    {
        DO.Order? order = s_dal.Order.Read(delivery.OrderId)
         ?? throw new BO.BlDoesNotExistException("Order not found");

        var estimatedDeliveryTime = Tools.GetEstimatedDeliveryTime(delivery);
        var maxDeliveryTime = delivery.OrderDate.Add(s_dal.Config.MaxDeliveryTime);

        BO.OrderInProgress orderInProgress = new BO.OrderInProgress
        {
            DeliveryId = delivery.Id,
            OrderId = delivery.OrderId,
            TypeOfOrder = (BO.TypeOfOrder)order.TypeOfOrder,
            Details = order.Details,
            Address = order.Addres,
            Distance = Tools.GetDistance(order),
            ActualDistance = Tools.GetActualDistance(order.Addres,(BO.TheTypeShipment) s_dal.Courier.Read(delivery.CourierId).TypeShipment),// delivery.ActualDistance,
            CustomerName = order.Name,
            CustomerPhone = order.Phone,
            OrderTime = order.OrderDate,
            StartDeliveryTime = delivery.OrderDate,
            EstimatedDeliveryTime = estimatedDeliveryTime,
            MaxDeliveryTime = maxDeliveryTime,
            OrderStatus = BO.OrderStatus.DELIVERING,
            ScheduleStatus = Tools.GetScheduleStatus(order, delivery),
            TimeRemaining = (delivery.OrderDate.Add(s_dal.Config.MaxDeliveryTime) - AdminManager.Now)
        };
        return orderInProgress;
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
    /// <item><description>Are assigned to the specified courier</description></item>
    /// <item><description>Have not yet ended (EndDelivery is null)</description></item>
    /// <item><description>Are associated with an order in DELIVERING status</description></item>
    /// </list>
    /// If multiple active deliveries exist, returns the first one found.
    /// </remarks>
    private static BO.OrderInProgress? s_getOrderInProgres(int courierId)
    {
        IEnumerable<DO.Delivery> allDeliveries = s_dal.Delivery.ReadAll(d =>
            d.CourierId == courierId &&
            d.EndDelivery == null &&
            s_dal.Order.Read(d.OrderId)?.OrderStatus == DO.OrderStatus.DELIVERING
        );

        if (!allDeliveries.Any())
            return null;

        return s_createOrderInProgress(allDeliveries.First());
    }
}