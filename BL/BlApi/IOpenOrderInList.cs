namespace BlApi;

/// <summary>
/// Defines operations for retrieving open order list items.
/// </summary>
/// <remarks>
/// Open orders are orders that are available for assignment/acceptance and are typically presented
/// to couriers or managers as a list of candidate deliveries.
/// </remarks>
public interface IOpenOrderInList : IObservable
{
    /// <summary>
    /// Retrieves an open order list item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>A <see cref="BO.OpenOrderInList"/> instance.</returns>
    BO.OpenOrderInList Read(int id);
}
