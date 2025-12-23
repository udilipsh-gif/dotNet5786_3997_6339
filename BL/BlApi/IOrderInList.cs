namespace BlApi;

/// <summary>
/// Defines operations for retrieving order summary/list information.
/// </summary>
public interface IOrderInList
{
    /// <summary>
    /// Retrieves an order summary item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>A <see cref="BO.OrderInList"/> instance.</returns>
    BO.OrderInList Read(int id);
}
