namespace BlImplementation;
using BlApi;

using Helpers;
using System.Threading.Tasks;

internal class DeliveryImplementation : IDelivery
{

    /// <summary>
    /// Starts a delivery by assigning a courier to an open order.
    /// </summary>
    /// <param name="id">The ID of the user attempting to start the delivery (must be a manager or the specified courier).</param>
    /// <param name="courierId">The unique identifier of the courier to assign to the order.</param>
    /// <param name="orderId">The unique identifier of the order to assign for delivery.</param>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier being assigned to the order.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or courier is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in OPEN or REFUSED status.</exception>
    /// <remarks>
    /// Managers can assign any order to any courier. Couriers can only accept orders for themselves.
    /// </remarks>
    public async Task StartDelivery(int id, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id) && id != courierId)
            throw new BO.BlNoAccessException();
        await DeliveryManager.StartDelivery(courierId, orderId);
    }

    /// <summary>
    /// Marks a delivery as completed with a specific outcome.
    /// </summary>
    /// <param name="id">The ID of the user attempting to complete the delivery (must be a manager or the specified courier).</param>
    /// <param name="courierId">The unique identifier of the courier completing the delivery.</param>
    /// <param name="orderId">The unique identifier of the order being completed.</param>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier assigned to the delivery.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or delivery is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in DELIVERING status.</exception>
    /// <remarks>
    /// Managers can complete any delivery. Couriers can only complete their own assigned deliveries.
    /// </remarks>
    public void DeliverEnd(int id, int courierId, int deliveryId, BO.EndDelivery endDelivery)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        if (!Tools.CheckManger(id) && id != courierId)
            throw new BO.BlNoAccessException();
        DeliveryManager.DeliverEnd(courierId, deliveryId, endDelivery);
    }

    /// <summary>
    /// Retrieves all completed deliveries for a specific courier with optional filtering and sorting.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get closed deliveries (must be a manager).</param>
    /// <param name="courierId">The unique identifier of the courier whose deliveries to retrieve.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.ClosedDeliveryInList"/> objects.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
    /// <remarks>
    /// Only managers can view closed delivery history.
    /// </remarks>
    public async Task<IEnumerable<BO.ClosedDeliveryInList>> GetClosed(int id, int courierId, BO.TypeOfOrder? filter, BO.ClosedDeliveryInListField? sort)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return await DeliveryManager.GetClosed(courierId, filter, sort);
    }

    /// <summary>
    /// Retrieves all open orders available for a specific courier to deliver.
    /// </summary>
    /// <param name="id">The ID of the user attempting to get open orders (must be a manager or the specified courier).</param>
    /// <param name="courierId">The unique identifier of the courier for whom to find available orders.</param>
    /// <param name="filter">Optional filter for order type, or null to include all types.</param>
    /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="BO.OpenOrderInList"/> objects.
    /// </returns>
    /// <exception cref="BO.BlNoAccessException">
    /// Thrown when the user is neither a manager nor the courier whose orders are being requested.
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier is not found.</exception>
    /// <remarks>
    /// Managers can view open orders for any courier. Couriers can only view their own available orders.
    /// </remarks>
    public async Task<IEnumerable<BO.OpenOrderInList>> GetOpen(int id, int courierId, BO.TypeOfOrder? filter, BO.OpenOrderInListField? sort)
    {
        if (!Tools.CheckManger(id) && id != courierId)
            throw new BO.BlNoAccessException();

        return await DeliveryManager.GetOpen(courierId, filter, sort);
    }

    public async Task<GoogleMapsService.RouteInfo?> GetRouteFromStore(double destLat, double destLng, BO.TheTypeShipment shipmentType)
        => await DeliveryManager.GetRouteFromStore(destLat, destLng, shipmentType);

    public IEnumerable<DO.Delivery> ReadAll(Func<DO.Delivery, bool>? customPredicate = null)
        => DeliveryManager.ReadAll(customPredicate);

    /// <summary>
    /// Retrieves a static map URL from the store to the destination using Google Maps.
    /// </summary>
    /// <param name="destLat">Destination latitude.</param>
    /// <param name="destLng">Destination longitude.</param>
    /// <param name="shipmentType">Type of shipment.</param>
    /// <param name="width">Width of the map image (default 400).</param>
    /// <param name="height">Height of the map image (default 300).</param>
    /// <returns>URL string of the static map, or null if not available.</returns>
    public async Task<string?> GetStaticMapUrlFromStore(double destLat, double destLng, BO.TheTypeShipment shipmentType, int width = 400, int height = 300)
        => await DeliveryManager.GetStaticMapUrlFromStore(destLat, destLng, shipmentType, width, height);
    


    public void AddObserver(Action listObserver) =>
        OrderManager.Observer.AddListObserver(listObserver);

    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observer.RemoveListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        OrderManager.Observer.AddObserver(id, observer);

    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observer.RemoveObserver(id, observer);

}
