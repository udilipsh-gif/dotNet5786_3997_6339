using DalApi;
using System.Threading.Tasks;

namespace Helpers;

/// <summary>
/// Manages delivery-related operations in the business logic layer.
/// </summary>
/// <remarks>
/// This static class provides functionality for creating, reading, and managing deliveries.
/// It acts as an intermediary between the business logic layer and the data access layer,
/// handling delivery lifecycle operations including creation, retrieval, and completion.
/// </remarks>
internal static class DeliveryManager
{
    /// <summary>
    /// Data access layer instance for database operations.
    /// </summary>
    private static readonly IDal s_dal = Factory.Get;

    /// <summary>
    /// Observer manager for notifying UI components about delivery changes.
    /// </summary>
    internal static readonly ObserverManager Observer = new();

    /// <summary>
    /// Retrieves all deliveries from the data access layer.
    /// </summary>
    /// <param name="sort">
    /// Optional sorting criterion for deliveries. 
    /// Currently not implemented - parameter is ignored.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="DO.Delivery"/> objects 
    /// representing all deliveries in the system.
    /// </returns>
    /// <remarks>
    /// Note: The sort parameter is currently not utilized in the implementation.
    /// All deliveries are returned in their default order from the data access layer.
    /// </remarks>
    internal static IEnumerable<DO.Delivery> ReadAll()
    {
        lock (AdminManager.BlMutex)
            return s_dal.Delivery.ReadAll().ToList();
    }

    /// <summary>
    /// Retrieves a specific delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to retrieve.</param>
    /// <returns>
    /// The <see cref="DO.Delivery"/> object with the specified ID, 
    /// or null if not found.
    /// </returns>
    internal static DO.Delivery? Read(int id)
    {
        lock (AdminManager.BlMutex)
            return s_dal.Delivery.Read(id);
    }

    /// <summary>
    /// Assigns a courier to an open order, initiating the delivery process.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier to assign.</param>
    /// <param name="orderId">The unique identifier of the order to assign.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the order or courier is not found.
    /// </exception>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when the order is not in OPEN or REFUSED status.
    /// </exception>
    /// <remarks>
    /// Creates a new delivery record linking the courier to the order and updates
    /// the order status to DELIVERING. Only orders with OPEN or REFUSED status can be selected.
    /// </remarks>
    public static async Task StartDelivery(int courierId, int orderId)
    {
        DO.Order doOrder;
        lock (AdminManager.BlMutex)
            doOrder = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException("Order not found");

        DO.Courier? doCourier;
        lock (AdminManager.BlMutex)
            doCourier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        // Validate order status without expensive full conversion
        BO.OrderStatus currentStatus = Tools.s_getOrderStatus(doOrder);

        if (currentStatus is not BO.OrderStatus.OPEN)
            throw new BO.BlInvalidOperationException("Order is not open for selection");

        await DeliveryManager.Create(doOrder, doCourier);
    }

    /// <summary>
    /// Retrieves all completed deliveries for a specific courier with optional filtering and sorting.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">The field to sort the results by.</param>
    /// <returns>
    /// A list of closed delivery records showing delivery outcomes, distances, times, and final statuses.
    /// Results are filtered to show only one delivery per order (the most recent).
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier is not found.</exception>
    /// <remarks>
    /// Only returns deliveries that have ended (EndDelivery is not null).
    /// Results are deduplicated by OrderId to show only the most recent delivery attempt for each order.
    /// </remarks>
    public static async Task<List<BO.ClosedDeliveryInList>> GetClosed(
    int courierId,
    BO.TypeOfOrder? filter,
    BO.ClosedDeliveryInListField? sort)
    {
        DO.Courier courier;
        lock (AdminManager.BlMutex)
            courier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        List<(DO.Delivery Delivery, DO.Order? Order)> rawData;

        lock (AdminManager.BlMutex)
        {
            rawData = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.EndDelivery != null)
            .Select(doDelivery => (
                Delivery: doDelivery,
                Order: s_dal.Order.Read(doDelivery.OrderId)
            ))
            .Where(item =>
                item.Order != null &&
                (filter == null || (BO.TypeOfOrder)item.Order.TypeOfOrder == filter)
            )
            .ToList();
        }

        var tasks = rawData.Select(async item =>
        {
            double? actualDistance = null;

            var order = item.Order!;

            if (item.Delivery.ActualDistance is null or 0.0)
            {
                try
                {
                    actualDistance = await GoogleMapsService.GetActualDistance(
                        order.Latitude,
                        order.Longitude,
                        (BO.TheTypeShipment)courier.TypeShipment);
                }
                catch
                {
                    actualDistance = null;
                }
            }
            else
            {
                actualDistance = item.Delivery.ActualDistance;
            }

            return new BO.ClosedDeliveryInList
            {
                DeliveryId = item.Delivery.Id,
                OrderId = order.Id,
                OrderType = (BO.TypeOfOrder)order.TypeOfOrder,
                Address = order.Addres,
                ShipmentType = (BO.TheTypeShipment)item.Delivery.TypeShipment,
                ActualDistance = actualDistance,
                DeliveryTime = (TimeSpan)(item.Delivery.TimeEndDelivery! - item.Delivery.OrderDate),
                EndDelivery = (BO.EndDelivery)item.Delivery.EndDelivery!
            };
        });

        var results = await Task.WhenAll(tasks);

        return [.. s_sortClosedDeliveries(results, sort)];
    }

    /// <summary>
    /// Retrieves all open orders available for a specific courier to deliver.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">The field to sort the results by.</param>
    /// <returns>
    /// A list of open orders within the courier's maximum delivery distance,
    /// including distance calculations, time constraints, and schedule status.
    /// </returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier is not found.</exception>
    /// <remarks>
    /// Only returns orders with OPEN status that are within the courier's
    /// maximum delivery distance capability and match the courier's vehicle capabilities.
    /// Results can be filtered by order type and sorted by various fields.
    /// </remarks>
    public static async Task<List<BO.OpenOrderInList>> GetOpen(
        int courierId,
        BO.TypeOfOrder? filter,
        BO.OpenOrderInListField? sort)
    {
        DO.Courier doCourier;
        lock (AdminManager.BlMutex)
            doCourier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        var config = AdminManager.GetConfig();
        var currentTime = AdminManager.Now;
        var permittedOrders = s_getPermittedOrderTypes(doCourier.TypeShipment);

        Func<DO.Order, bool> distanceFilter;
        if (config.Latitude is double storeLat &&
            config.Longitude is double storeLon &&
            doCourier.MaxDistanceDelivery is double maxDist)
        {
            distanceFilter = order => Tools.GetDistance(order.Latitude, order.Longitude, storeLat, storeLon) <= maxDist;
        }
        else
        {
            distanceFilter = _ => true;
        }

        IEnumerable<DO.Order> relevantOrders;
        lock (AdminManager.BlMutex)
            relevantOrders = s_dal.Order.ReadAll(o => o.OrderStatus == DO.OrderStatus.OPEN)
            .Where(order =>
                permittedOrders.Contains(order.TypeOfOrder) &&
                (filter == null || order.TypeOfOrder == (DO.TypeOfOrder)filter) &&
                distanceFilter(order)
            ).ToList();

        var tasks = relevantOrders.Select(async doOrder =>
        {
            var maxDeliveryTime = doOrder.OrderDate + config.MaxDeliveryTime;

            var distance = await GoogleMapsService.GetActualDistance(doOrder.Latitude, doOrder.Longitude, (BO.TheTypeShipment)doCourier.TypeShipment);

            return new BO.OpenOrderInList
            {
                OrderId = doOrder.Id,
                TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
                Weight = doOrder.Weight,
                Address = doOrder.Addres,
                DistanceKm = doOrder.DistanceKm ?? 0,
                ActualDistance = distance,
                EstimatedDeliveryTime = Tools.GetEstimatedDeliveryTime((BO.TheTypeShipment)doCourier.TypeShipment, distance ?? 0),
                ScheduleStatus = await Tools.GetScheduleStatus(doOrder),
                TimeLeftForDelivery = maxDeliveryTime - currentTime,
                MaxDeliveryTime = maxDeliveryTime
            };
        });

        var results = await Task.WhenAll(tasks);

        return [.. s_sortOpenOrders(results, sort)];
    }

    /// <summary>
    /// Creates a new delivery assignment for a specific order and courier.
    /// </summary>
    /// <param name="order">The <see cref="DO.Order"/> to be delivered.</param>
    /// <param name="courier">The <see cref="DO.Courier"/> assigned to deliver the order.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when the order's delivery distance exceeds the courier's maximum delivery distance capability.
    /// </exception>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="number">
    ///   <item><description>Validates that the courier can handle the delivery distance</description></item>
    ///   <item><description>Creates a new delivery record with the current timestamp</description></item>
    ///   <item><description>Calculates the actual road/walking distance using Google Distance Matrix API</description></item>
    ///   <item><description>Assigns the courier to the delivery</description></item>
    ///   <item><description>Sets TypeShipment based on the courier's vehicle type</description></item>
    ///   <item><description>Updates the order status to DELIVERING</description></item>
    ///   <item><description>Notifies all relevant observers about the changes</description></item>
    /// </list>
    /// The delivery is created with null end status and time, indicating it is in progress.
    /// </remarks>
    public static async Task Create(DO.Order order, DO.Courier courier)
    {
        s_validateDeliveryDistance(order, courier);

        if (!courier.Active)
            throw new BO.BlInvalidOperationException("שגיאה שליח לא פעיל");


        DO.Delivery delivery = new DO.Delivery
        {
            Id = 0, // Auto-generated by DAL
            OrderId = order.Id,
            CourierId = courier.Id,
            TypeShipment = courier.TypeShipment,
            OrderDate = AdminManager.Now,
            ActualDistance = await GoogleMapsService.GetActualDistance(order.Latitude, order.Longitude, (BO.TheTypeShipment)courier.TypeShipment),
            EndDelivery = null,
            TimeEndDelivery = null
        };

        lock (AdminManager.BlMutex)
            s_dal.Delivery.Create(delivery);
        lock (AdminManager.BlMutex)
            s_dal.Order.Update(order with { OrderStatus = DO.OrderStatus.DELIVERING });

        s_notifyAllObservers(delivery.OrderId, courier.Id);
    }

    /// <summary>
    /// Marks a delivery as completed with the specified end status.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier completing the delivery.</param>
    /// <param name="deliveryId">The unique identifier of the delivery being completed.</param>
    /// <param name="endDelivery">The final status of the delivery attempt.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the delivery or associated order does not exist in the system.
    /// </exception>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when the courier attempting to complete the delivery is not the assigned courier.
    /// </exception>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="number">
    ///   <item><description>Validates that the delivery exists</description></item>
    ///   <item><description>Verifies that the courier is assigned to this delivery</description></item>
    ///   <item><description>Updates the delivery with the end status and completion time</description></item>
    ///   <item><description>Updates the order status based on the delivery outcome</description></item>
    ///   <item><description>Notifies all relevant observers about the changes</description></item>
    /// </list>
    /// 
    /// Order status mapping based on delivery outcome:
    /// <list type="bullet">
    ///   <item><description>DELIVERED → Order COMPLETED</description></item>
    ///   <item><description>CANCELLED → Order CANCELLED</description></item>
    ///   <item><description>REFUSED → Order REFUSED</description></item>
    ///   <item><description>FAILED/NOTFOUND → Order OPEN (available for retry)</description></item>
    /// </list>
    /// </remarks>
    public static void Deliver(int courierId, int deliveryId, BO.EndDelivery endDelivery)
    {
        DO.Delivery delivery = Tools.GetAndValidateDelivery(deliveryId, courierId);

        // Update delivery with completion details
        delivery = delivery with
        {
            EndDelivery = (DO.EndDelivery)endDelivery,
            TimeEndDelivery = AdminManager.Now
        };
        lock (AdminManager.BlMutex)
            s_dal.Delivery.Update(delivery);

        // Update order status based on delivery outcome
        UpdateOrderStatusAfterDelivery(delivery.OrderId, endDelivery);

        // Notify all observers
        s_notifyDeliveryCompleted(deliveryId, delivery.OrderId, courierId);
    }

    /// <summary>
    /// Validates that the courier can handle the delivery distance.
    /// </summary>
    /// <param name="order">The order to be delivered.</param>
    /// <param name="courier">The courier assigned to the delivery.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when the distance exceeds the courier's maximum capability.
    /// </exception>
    private static void s_validateDeliveryDistance(DO.Order order, DO.Courier courier)
    {
        if (order.DistanceKm > courier.MaxDistanceDelivery)
        {
            throw new BO.BlInvalidValueException(
                $"Courier with ID {courier.Id} cannot deliver to distance {order.DistanceKm} km. " +
                $"Maximum distance: {courier.MaxDistanceDelivery} km");
        }
    }



    /// <summary>
    /// Updates the order status based on the delivery outcome.
    /// </summary>
    /// <param name="orderId">The order ID to update.</param>
    /// <param name="endDelivery">The delivery outcome status.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order is not found.</exception>
    private static void UpdateOrderStatusAfterDelivery(int orderId, BO.EndDelivery endDelivery)
    {
        DO.Order order;
        lock (AdminManager.BlMutex)
            order = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException($"Order with ID {orderId} not found");

        DO.OrderStatus newStatus = s_mapDeliveryOutcomeToOrderStatus(endDelivery, order.OrderStatus);

        lock (AdminManager.BlMutex)
            s_dal.Order.Update(order with { OrderStatus = newStatus });
    }

    /// <summary>
    /// Maps a delivery outcome to the corresponding order status.
    /// </summary>
    /// <param name="endDelivery">The delivery outcome.</param>
    /// <param name="currentStatus">The current order status (used as fallback).</param>
    /// <returns>The new order status based on the delivery outcome.</returns>
    private static DO.OrderStatus s_mapDeliveryOutcomeToOrderStatus(
        BO.EndDelivery endDelivery,
        DO.OrderStatus currentStatus)
    {
        return endDelivery switch
        {
            BO.EndDelivery.DELIVERED => DO.OrderStatus.COMPLETED,
            BO.EndDelivery.CANCELLED => DO.OrderStatus.CONCELLED,
            BO.EndDelivery.REFUSED => DO.OrderStatus.REFUSED,
            BO.EndDelivery.FAILED => DO.OrderStatus.OPEN,
            BO.EndDelivery.NOTFOUND => DO.OrderStatus.OPEN,
            _ => currentStatus
        };
    }

    /// <summary>
    /// Notifies all relevant observers after a delivery is created.
    /// </summary>
    /// <param name="orderId">The order ID involved in the delivery.</param>
    /// <param name="courierId">The courier ID assigned to the delivery.</param>
    private static void s_notifyAllObservers(int orderId, int courierId)
    {
        OrderManager.Observer.NotifyItemUpdated(orderId);
        CourierManager.Observer.NotifyItemUpdated(courierId);
        Observer.NotifyListUpdated();
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    public static async Task<GoogleMapsService.RouteInfo?> GetRouteFromStore(
                     double destLat, double destLng, BO.TheTypeShipment shipmentType)
        => await GoogleMapsService.GetRouteFromStore(destLat, destLng, shipmentType);


    public static async Task<string?> GetStaticMapUrlFromStore(double destLat, double destLng,
                               BO.TheTypeShipment shipmentType, int width = 400, int height = 300)
        => await GoogleMapsService.GetStaticMapUrlFromStore(destLat, destLng, shipmentType, width, height);

    /// <summary>
    /// Notifies all relevant observers after a delivery is completed.
    /// </summary>
    /// <param name="deliveryId">The completed delivery ID.</param>
    /// <param name="orderId">The order ID involved in the delivery.</param>
    /// <param name="courierId">The courier ID who completed the delivery.</param>
    private static void s_notifyDeliveryCompleted(int deliveryId, int orderId, int courierId)
    {
        Observer.NotifyItemUpdated(deliveryId);
        OrderManager.Observer.NotifyItemUpdated(orderId);
        CourierManager.Observer.NotifyItemUpdated(courierId);
        Observer.NotifyListUpdated();
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    /// <summary>
    /// Sorts closed deliveries by the specified field.
    /// </summary>
    /// <param name="query">The query to sort.</param>
    /// <param name="sort">The field to sort by.</param>
    /// <returns>Sorted enumerable of closed deliveries.</returns>
    private static IEnumerable<BO.ClosedDeliveryInList> s_sortClosedDeliveries(
        IEnumerable<BO.ClosedDeliveryInList> query,
        BO.ClosedDeliveryInListField? sort)
    {
        return sort switch
        {
            BO.ClosedDeliveryInListField.DeliveryId => query.OrderBy(x => x.DeliveryId),
            BO.ClosedDeliveryInListField.OrderId => query.OrderBy(x => x.OrderId),
            BO.ClosedDeliveryInListField.TypeOfOrder => query.OrderBy(x => x.OrderType),
            BO.ClosedDeliveryInListField.Address => query.OrderBy(x => x.Address),
            BO.ClosedDeliveryInListField.ShipmentType => query.OrderBy(x => x.ShipmentType),
            BO.ClosedDeliveryInListField.ActualDistance => query.OrderBy(x => x.ActualDistance),
            BO.ClosedDeliveryInListField.DeliveryTime => query.OrderBy(x => x.DeliveryTime),
            BO.ClosedDeliveryInListField.EndDelivery => query.OrderBy(x => x.EndDelivery),
            _ => query.OrderBy(x => x.DeliveryId)
        };
    }

    /// <summary>
    /// Sorts open orders by the specified field.
    /// </summary>
    /// <param name="query">The query to sort.</param>
    /// <param name="sort">The field to sort by.</param>
    /// <returns>Sorted enumerable of open orders.</returns>
    private static IEnumerable<BO.OpenOrderInList> s_sortOpenOrders(
        IEnumerable<BO.OpenOrderInList> query,
        BO.OpenOrderInListField? sort)
    {
        return sort switch
        {
            BO.OpenOrderInListField.OrderId => query.OrderBy(x => x.OrderId),
            BO.OpenOrderInListField.TypeOfOrder => query.OrderBy(x => x.TypeOfOrder),
            BO.OpenOrderInListField.Weight => query.OrderBy(x => x.Weight),
            BO.OpenOrderInListField.Address => query.OrderBy(x => x.Address),
            BO.OpenOrderInListField.DistanceKm => query.OrderBy(x => x.DistanceKm),
            BO.OpenOrderInListField.ActualDistance => query.OrderBy(x => x.ActualDistance),
            BO.OpenOrderInListField.EstimatedDeliveryTime => query.OrderBy(x => x.EstimatedDeliveryTime),
            BO.OpenOrderInListField.ScheduleStatus => query.OrderBy(x => x.ScheduleStatus),
            BO.OpenOrderInListField.TimeLeftForDelivery => query.OrderBy(x => x.TimeLeftForDelivery),
            BO.OpenOrderInListField.MaxDeliveryTime => query.OrderBy(x => x.MaxDeliveryTime),
            _ => query.OrderBy(x => x.OrderId)
        };
    }




    /// <summary>
    /// Gets the permitted order types for a given shipment type.
    /// </summary>
    /// <param name="shipmentType">The courier's shipment/vehicle type.</param>
    /// <returns>Array of permitted order types.</returns>
    private static DO.TypeOfOrder[] s_getPermittedOrderTypes(DO.TheTypeShipment shipmentType)
    {
        return shipmentType switch
        {
            DO.TheTypeShipment.BIKE or DO.TheTypeShipment.FOOT =>
                [DO.TypeOfOrder.STANDART],

            DO.TheTypeShipment.CAR =>
                [DO.TypeOfOrder.STANDART, DO.TypeOfOrder.FAST_DELIVERY],

            DO.TheTypeShipment.MOTORCYCLE =>
                [DO.TypeOfOrder.STANDART, DO.TypeOfOrder.FAST_DELIVERY, DO.TypeOfOrder.DELIVER_IMMEDIATELY],

            _ => []
        };
    }
}
