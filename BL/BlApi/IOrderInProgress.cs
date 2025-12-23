namespace BlApi;

/// <summary>
/// Defines operations for retrieving an order that is currently in progress (active delivery).
/// </summary>
public interface IOrderInProgress
{
    /// <summary>
    /// Retrieves the current order in progress for a specified courier.
    /// </summary>
    /// <param name="id">The unique identifier of the courier.</param>
    /// <returns>
    /// An <see cref="BO.OrderInProgress"/> instance if the courier has an active delivery;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    BO.OrderInProgress? Read(int id);
}
