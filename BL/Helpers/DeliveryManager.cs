using DalApi;

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
    private static IDal s_dal = Factory.Get; //stage 4

    internal static ObserverManager Observer = new();

    /// <summary>
    /// Retrieves all deliveries from the data access layer.
    /// </summary>
    /// <param name="sort">Optional sorting criterion for deliveries. Currently not implemented - parameter is ignored.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="DO.Delivery"/> objects representing all deliveries in the system.
    /// </returns>
    /// <remarks>
    /// Note: The sort parameter is currently not utilized in the implementation.
    /// All deliveries are returned in their default order from the data access layer.
    /// This method is intended for internal use within the business logic layer.
    /// </remarks>
    internal static IEnumerable<DO.Delivery> ReadAll(
    BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        return s_dal.Delivery.ReadAll();
    }

    /// <summary>
    /// Retrieves a specific delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to retrieve.</param>
    /// <returns>
    /// The <see cref="DO.Delivery"/> object with the specified ID, or null if not found.
    /// </returns>
    /// <remarks>
    /// This method provides read-only access to delivery information.
    /// It queries the data access layer for the delivery with the matching ID.
    /// </remarks>
    internal static DO.Delivery? Read(int id)
    {
        return s_dal.Delivery.Read(id);
    }

    /// <summary>
    /// Marks a delivery as completed by the assigned courier.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier completing the delivery.</param>
    /// <param name="deliveryId">The unique identifier of the delivery being completed.</param>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the delivery with the specified ID does not exist in the system.
    /// </exception>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when the courier attempting to complete the delivery is not the assigned courier for that delivery.
    /// </exception>
    /// <remarks>
    /// This method performs the following operations:
    /// <list type="number">
    /// <item><description>Validates that the delivery exists</description></item>
    /// <item><description>Verifies that the courier is assigned to this delivery</description></item>
    /// <item><description>Updates the delivery status to DELIVERED</description></item>
    /// <item><description>Records the completion time using the current system clock</description></item>
    /// </list>
    /// </remarks>
    public static void Deliver(int courierId, int deliveryId, BO.EndDelivery endDelivery)
    {
        DO.Delivery delivery = s_dal.Delivery.Read(deliveryId)
            ?? throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found");
        if (delivery.CourierId != courierId)
            throw new BO.BlInvalidValueException($"Courier with ID {courierId} is not assigned to this delivery  ");
        delivery = delivery with
        {
            EndDelivery = (DO.EndDelivery)endDelivery,
            TimeEndDelivery = AdminManager.Now
        };

        s_dal.Delivery.Update(delivery);

        DO.Order order = s_dal.Order.Read(delivery.OrderId)
                ?? throw new BO.BlDoesNotExistException($"Order with ID {delivery.OrderId} not found");
        s_dal.Order.Update(order with
        {
            OrderStatus = endDelivery switch
            {
                BO.EndDelivery.DELIVERED => DO.OrderStatus.COMPLETED,
                BO.EndDelivery.CANCELLED => DO.OrderStatus.CONCELLED,
                BO.EndDelivery.REFUSED => DO.OrderStatus.REFUSED,
                BO.EndDelivery.FAILED => DO.OrderStatus.OPEN,
                BO.EndDelivery.NOTFOUND => DO.OrderStatus.OPEN,
                _ => order.OrderStatus
            }
        });

        Observer.NotifyItemUpdated(deliveryId);
        OrderManager.Observer.NotifyItemUpdated(delivery.OrderId);
        CourierManager.Observer.NotifyItemUpdated(courierId);
        Observer.NotifyListUpdated();
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
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
    /// <item><description>Validates that the courier can handle the delivery distance</description></item>
    /// <item><description>Creates a new delivery record with the current timestamp</description></item>
    /// <item><description>Calculates the actual road/walking distance based on the order address and shipment type using Google Distance Matrix API</description></item>
    /// <item><description>Assigns the courier to the delivery</description></item>
    /// <item><description>Sets TypeShipment based on the courier's vehicle type (not order type)</description></item>
    /// <item><description>Persists the delivery to the data access layer with ID 0 (auto-generated)</description></item>
    /// </list>
    /// The delivery is created with null end status and time, indicating it is in progress.
    /// </remarks>
    public static void Create(DO.Order order, DO.Courier courier)
    {
        if (order.DistanceKm > courier.MaxDistanceDelivery)
            throw new BO.BlInvalidValueException($"Courier with ID {courier.Id} cannot deliver to distance {order.DistanceKm} km");

        DO.Delivery delivery = new DO.Delivery
        {
            Id = 0,
            OrderId = order.Id,
            CourierId = courier.Id,
            TypeShipment = courier.TypeShipment,
            OrderDate = AdminManager.Now,
            ActualDistance = Tools.GetActualDistance(order.Addres, (BO.TheTypeShipment)courier.TypeShipment),
            EndDelivery = null,
            TimeEndDelivery = null

        };
        s_dal.Delivery.Create(delivery);
        s_dal.Order.Update(order with { OrderStatus = DO.OrderStatus.DELIVERING });
        OrderManager.Observer.NotifyItemUpdated(delivery.OrderId);
        CourierManager.Observer.NotifyItemUpdated(courier.Id);
        Observer.NotifyListUpdated();
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }




    ///// <summary>
    ///// Periodically updates the status of ongoing deliveries based on clock changes.
    ///// </summary>
    ///// <param name="oldClock">The previous system time before the clock was advanced.</param>
    ///// <param name="newClock">The new system time after the clock was advanced.</param>
    ///// <remarks>
    ///// This method is currently commented out but would be used to automatically complete
    ///// deliveries that have exceeded their maximum delivery time when the system clock is advanced.
    ///// <para>
    ///// The method would:
    ///// <list type="bullet">
    ///// <item><description>Query all deliveries that have not yet ended</description></item>
    ///// <item><description>Check if the new clock time exceeds the maximum delivery time for each delivery</description></item>
    ///// <item><description>Automatically mark overdue deliveries as DELIVERED</description></item>
    ///// <item><description>Set the completion time to the new clock time</description></item>
    ///// </list>
    ///// </para>
    ///// </remarks>
    //internal static void PeriodicDeliveriesUpdates(DateTime oldClock, DateTime newClock)
    //{
    //    // קריאת המשלוחים שעדיין לא הסתיימו
    //    var deliveries = s_dal.Delivery.ReadAll(d => d.EndDelivery == null);

    //    TimeSpan maxTime = s_dal.Config.MaxDeliveryTime;

    //    foreach (var delivery in deliveries)
    //    {
    //        // אם הזמן החדש עבר את זמן המשלוח המקסימלי
    //        if (newClock >= delivery.OrderDate.Add(maxTime))
    //        {
    //            var updated = delivery with
    //            {
    //                EndDelivery = DO.EndDelivery.DELIVERED,
    //                TimeEndDelivery = newClock
    //            };

    //            s_dal.Delivery.Update(updated);
    //        }
    //    }
    //}


}
