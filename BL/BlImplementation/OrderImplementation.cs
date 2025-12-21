namespace BlImplementation;
using BlApi;

using Helpers;

/// <summary>
/// Implements the <see cref="IOrder"/> interface, providing business logic operations for order management
/// with authorization and access control.
/// </summary>
/// <remarks>
/// This class serves as the implementation layer for order-related operations, enforcing
/// security by validating user permissions before delegating to the appropriate manager classes.
/// All operations require either manager privileges or specific user authorization (e.g., courier access).
/// </remarks>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order in the system.
    /// </summary>
    /// <param name="id">The ID of the user attempting to create the order (must be a manager).</param>
    /// <param name="boOrder">The business logic order object containing order details to create.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers are authorized to create orders. The order date is automatically set to the current system time.
    /// </remarks>
    public void Create(int id, BO.Order boOrder)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Create(boOrder);
    }

    /// <summary>
    /// Retrieves a specific order by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the user attempting to read the order (must be a manager).</param>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>
    /// A <see cref="BO.Order"/> object with complete order details including calculated fields,
    /// or null if no order with the specified ID exists.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view order details. The returned order includes enriched information
    /// such as distance calculations, delivery status, and timing constraints.
    /// </remarks>
    public BO.Order? Read(int id, int orderId)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        return OrderManager.Read(orderId);
    }

    /// <summary>
    /// Updates an existing order in the system.
    /// </summary>
    /// <param name="id">The ID of the user attempting to update the order (must be a manager).</param>
    /// <param name="boOrder">The business logic order object with updated information.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when the order contains invalid data (e.g., invalid phone number).</exception>
    /// <remarks>
    /// Only managers can update orders. The method validates phone numbers and updates
    /// address coordinates if the address has changed.
    /// </remarks>
    public void Update(int id, BO.Order boOrder)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Update(boOrder);
    }

    /// <summary>
    /// Attempts to delete an order from the system.
    /// </summary>
    /// <param name="id">The ID of the user attempting to delete the order (must be a manager).</param>
    /// <param name="orderId">The unique identifier of the order to delete.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Always thrown as orders cannot be deleted in this system.</exception>
    /// <remarks>
    /// Order deletion is not permitted in this system regardless of authorization.
    /// Orders should be cancelled instead using the <see cref="Cancel"/> method.
    /// </remarks>
    public void Delete(int id, int orderId)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Delete(orderId);
    }

    /// <summary>
    /// Starts a delivery by assigning a courier to an open order.
    /// </summary>
    /// <param name="id">The ID of the user attempting to start the delivery (must be a manager or the assigned courier).</param>
    /// <param name="courierId">The unique identifier of the courier to assign to the order.</param>
    /// <param name="orderId">The unique identifier of the order to assign for delivery.</param>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier being assigned to the order.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or courier is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in OPEN or REFUSED status.</exception>
    /// <remarks>
    /// Managers can assign any order to any courier. Couriers can only accept orders for themselves.
    /// Creates a new delivery record and updates the order status to DELIVERING.
    /// </remarks>
    public void StartDelivery(int id, int courierId, int orderId)
    {
        if (!Tools.CheckManger(id) && id != courierId) //העברה ל DELYVERY MANGER
            throw new BO.BlNoAccessException();
        OrderManager.StartDelivery(courierId, orderId);
    }

    /// <summary>
    /// Retrieves all orders from the system with optional filtering and sorting.
    /// </summary>
    /// <param name="id">The ID of the user attempting to read orders (must be a manager).</param>
    /// <param name="filter">The field to filter by, or null for no filtering.</param>
    /// <param name="value">The value to match for the specified filter field.</param>
    /// <param name="sort">The field to sort by, or null for default sorting by OrderStatus.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.OrderInList"/> objects,
    /// filtered and sorted as specified.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view all orders. The method returns lightweight order representations
    /// suitable for list views, including delivery tracking and status information.
    /// </remarks>
    public IEnumerable<BO.OrderInList> ReadAll(
        int id,
        BO.OrderInListField? filter,
        object? value,
        BO.OrderInListField? sort)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        return OrderManager.ReadAll(filter, value, sort);
    }

    /// <summary>
    /// Marks a delivery as completed with a specific outcome.
    /// </summary>
    /// <param name="id">The ID of the user attempting to complete the delivery (must be a manager or the assigned courier).</param>
    /// <param name="courierId">The unique identifier of the courier completing the delivery.</param>
    /// <param name="orderId">The unique identifier of the order being completed.</param>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier assigned to the delivery.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or delivery is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in DELIVERING status.</exception>
    /// <remarks>
    /// Managers can complete any delivery. Couriers can only complete their own assigned deliveries.
    /// Updates the delivery record with the final outcome (delivered, refused, failed, etc.)
    /// and completion time, then updates the order status accordingly.
    /// </remarks>
    public void Deliver(int id, int courierId, int orderId)
    {
        if (!Tools.CheckManger(id) && id != courierId) //העברה ל DELYVERY MANGER
            throw new BO.BlNoAccessException();
        DeliveryManager.Deliver(courierId, orderId);
    }

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <param name="id">The ID of the user attempting to cancel the order (must be a manager).</param>
    /// <param name="orderId">The unique identifier of the order to cancel.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">
    /// Thrown when the order is already completed or cancelled, or has an invalid status.
    /// </exception>
    /// <remarks>
    /// Only managers can cancel orders. Cancellation behavior depends on order status:
    /// - OPEN/REFUSED: Creates a cancellation delivery record
    /// - DELIVERING: Updates the current delivery with cancellation status
    /// - COMPLETED/CANCELLED: Cannot be cancelled (throws exception)
    /// </remarks>
    public void Cancel(int id, int orderId)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        OrderManager.Cancel(orderId);
    }

    /// <summary>
    /// Retrieves statistical counts of orders grouped by status.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get statistics (must be a manager).</param>
    /// <returns>
    /// An integer array containing order counts:
    /// - First indices contain counts for each OrderStatus value
    /// - Later indices contain counts for each ScheduleStatus value
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view order statistics. This provides a comprehensive overview
    /// of order distribution across different states and schedule statuses.
    /// </remarks>
    public int[] GetAllOrderStatistic(int id)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return OrderManager.GetAllOrderStatistic();
    }

    /// <summary>
    /// Retrieves all completed deliveries for a specific courier with optional filtering and sorting.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get closed deliveries (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier whose deliveries to retrieve.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.ClosedDeliveryInList"/> objects
    /// showing delivery outcomes, distances, times, and final statuses.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view closed delivery history. Returns only deliveries that have ended
    /// (with statuses like DELIVERED, REFUSED, CANCELLED, etc.), deduplicated by OrderId.
    /// </remarks>
    public IEnumerable<BO.ClosedDeliveryInList> GetClosed(int id, int courierId, BO.TypeOfOrder? filter, BO.ClosedDeliveryInListField? sort)
    {
        if(!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return OrderManager.GetClosed(courierId, filter, sort);
    }

    /// <summary>
    /// Retrieves all open orders available for a specific courier to deliver.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get open orders (must be a manager or the specified courier).</param>
    /// <param name="courierId">The unique identifier of the courier for whom to find available orders.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.OpenOrderInList"/> objects
    /// representing orders within the courier's delivery range and capability.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier whose orders are being requested.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier is not found.</exception>
    /// <remarks>
    /// Managers can view open orders for any courier. Couriers can only view their own available orders.
    /// Returns only orders with OPEN or REFUSED status that are within the courier's maximum
    /// delivery distance capability, with calculated distances, timing, and feasibility information.
    /// </remarks>
    public IEnumerable<BO.OpenOrderInList> GetOpen(int id, int courierId, BO.TypeOfOrder? filter, BO.OpenOrderInListField? sort)
    {
        if(!Tools.CheckManger(id) && id != courierId)
            throw new BO.BlNoAccessException();

        return OrderManager.GetOpen(courierId, filter, sort);
    }


}
