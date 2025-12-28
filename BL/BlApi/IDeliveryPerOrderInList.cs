namespace BlApi;

/// <summary>
/// Defines operations for retrieving delivery summary items as part of an order list view.
/// </summary>
/// <remarks>
/// The returned model is typically used to show delivery attempts and their outcomes for a given order.
/// </remarks>
public interface IDeliveryPerOrderInList : IObservable
{
    /// <summary>
    /// Retrieves a delivery summary item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery.</param>
    /// <returns>A <see cref="BO.DeliveryPerOrderInList"/> instance.</returns>
    BO.DeliveryPerOrderInList Read(int id);
}
