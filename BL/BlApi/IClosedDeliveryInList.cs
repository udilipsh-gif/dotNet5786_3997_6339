namespace BlApi;

/// <summary>
/// Defines operations for retrieving closed (ended) delivery list items.
/// </summary>
/// <remarks>
/// Closed deliveries are deliveries that have ended with a final outcome (delivered, refused, cancelled, etc.)
/// and are typically used for history views and reporting.
/// </remarks>
public interface IClosedDeliveryInList
{
    /// <summary>
    /// Retrieves a closed delivery list item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery.</param>
    /// <returns>A <see cref="BO.ClosedDeliveryInList"/> instance.</returns>
    BO.ClosedDeliveryInList Read(int id);
}
