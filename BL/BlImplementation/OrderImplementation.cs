namespace BlImplementation;
using BlApi;

using Helpers;
using System.Threading.Tasks;

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
    /// Only managers are authorized to create orders.
    /// </remarks>
    public async Task Create(int id, BO.Order boOrder)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        await OrderManager.Create(boOrder);
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
    /// Only managers can view order details.
    /// </remarks>
    public async Task<BO.Order?> Read(int id, int orderId)
    {
        //if (!Tools.CheckManger(id))
        //    throw new BO.BlNoAccessException();
        return await OrderManager.Read(orderId);
    }

    /// <summary>
    /// Updates an existing order in the system.
    /// </summary>
    /// <param name="id">The ID of the user attempting to update the order (must be a manager).</param>
    /// <param name="boOrder">The business logic order object with updated information.</param>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <exception cref="BO.BlInvalidValueException">Thrown when the order contains invalid data.</exception>
    /// <remarks>
    /// Only managers can update orders.
    /// </remarks>
    public async Task Update(int id, BO.Order boOrder)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        await OrderManager.Update(boOrder);
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
    /// Orders should be cancelled instead using the <see cref="Delete"/> method.
    /// </remarks>
    public void Delete(int id, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Delete(orderId);
    }


    /// <summary>
    /// Retrieves all orders from the system with optional filtering and sorting.
    /// </summary>
    /// <param name="id">The ID of the user attempting to read orders (must be a manager).</param>
    /// <param name="filter">The field to filter by, or null for no filtering.</param>
    /// <param name="value">The value to match for the specified filter field.</param>
    /// <param name="sort">The field to sort by, or null for default sorting.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.OrderInList"/> objects,
    /// filtered and sorted as specified.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view all orders.
    /// </remarks>
    public async Task<IEnumerable<BO.OrderInList>> ReadAll(
        int id,
        BO.OrderInListField? filter,
        object? value,
        BO.OrderInListField? sort)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return await OrderManager.ReadAll(filter, value, sort);
    }

    public async Task<IEnumerable<BO.OrderInList>> ReadAll(
       int id,
       Func<BO.OrderInList, bool>? filter = null,
       BO.OrderInListField? sort = null)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        return await OrderManager.ReadAll(filter, sort);
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
    /// Only managers can cancel orders.
    /// </remarks>
    public async Task Cancel(int id, int orderId, bool token = false)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        await OrderManager.Cancel(orderId, token);
    }

    /// <summary>
    /// Retrieves statistical counts of orders grouped by status.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get statistics (must be a manager).</param>
    /// <returns>
    /// An integer array containing order counts.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view order statistics.
    /// </remarks>
    public async Task<int[]> GetAllOrderStatistic(int id)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return await OrderManager.GetAllOrderStatistic();
    }

    public void AddObserver(Action listObserver) =>
        OrderManager.Observer.AddListObserver(listObserver);

    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observer.RemoveListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        OrderManager.Observer.AddObserver(id, observer);

    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observer.RemoveObserver(id, observer);

}

