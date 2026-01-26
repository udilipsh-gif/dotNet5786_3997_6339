using BO;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlApi
{
    public interface IDelivery : IObservable
    {
        /// <summary>
        /// Starts a delivery by assigning a courier to an open order.
        /// </summary>
        /// <param name="id">The ID of the user attempting to start the delivery (must be a manager or the assigned courier).</param>
        /// <param name="courierId">The unique identifier of the courier to assign to the order.</param>
        /// <param name="orderId">The unique identifier of the order to assign for delivery.</param>
        /// <exception cref="BO.BlNoAccessException">Thrown when the user is neither a manager nor the courier being assigned.</exception>
        /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or courier is not found.</exception>
        /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in OPEN or REFUSED status.</exception>
        /// <remarks>
        /// Managers can assign any order to any courier. Couriers can only accept orders for themselves.
        /// </remarks>
        Task StartDelivery(int id, int courierId, int orderId);

        /// <summary>
        /// Marks a delivery as completed with a specific outcome.
        /// </summary>
        /// <param name="id">The ID of the user attempting to complete the delivery (must be a manager or the assigned courier).</param>
        /// <param name="courierId">The unique identifier of the courier completing the delivery.</param>
        /// <param name="deliveryId">The unique identifier of the Delyivery being completed.</param>
        /// <param name="endDelivery">The details of the delivery completion outcome.</param>
        /// <exception cref="BO.BlNoAccessException">Thrown when the user is neither a manager nor the assigned courier.</exception>
        /// <exception cref="BO.BlDoesNotExistException">Thrown when the order or delivery is not found.</exception>
        /// <exception cref="BO.BlInvalidOperationException">Thrown when the order is not in DELIVERING status.</exception>
        /// <remarks>
        /// Managers can complete any delivery. Couriers can only complete their own assigned deliveries.
        /// </remarks>
        void Deliver(int id, int courierId, int deliveryId, BO.EndDelivery endDelivery);

        /// <summary>
        /// Retrieves all completed deliveries for a specific courier with optional filtering and sorting.
        /// </summary>
        /// <param name="id">The ID of the user attempting to get closed deliveries (must be a manager).</param>
        /// <param name="courierId">The unique identifier of the courier whose deliveries to retrieve.</param>
        /// <param name="filter">Optional filter for order type, or null to include all types.</param>
        /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
        /// <returns>An enumerable of <see cref="BO.ClosedDeliveryInList"/> items.</returns>
        /// <exception cref="BO.BlNoAccessException">Thrown when the user does not have manager privileges.</exception>
        Task<IEnumerable<BO.ClosedDeliveryInList>> GetClosed(int id, int courierId, TypeOfOrder? filter, ClosedDeliveryInListField? sort);

        /// <summary>
        /// Retrieves all open orders available for a specific courier to deliver.
        /// </summary>
        /// <param name="id">The ID of the user attempting to get open orders (must be a manager or the specified courier).</param>
        /// <param name="courierId">The unique identifier of the courier for whom to find available orders.</param>
        /// <param name="filter">Optional filter for order type, or null to include all types.</param>
        /// <param name="sort">Optional field to sort the results by, or null for default sorting.</param>
        /// <returns>An enumerable of <see cref="BO.OpenOrderInList"/> items.</returns>
        /// <exception cref="BO.BlNoAccessException">Thrown when the user is neither a manager nor the requested courier.</exception>
        /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier is not found.</exception>
        Task<IEnumerable<BO.OpenOrderInList>> GetOpen(int id, int courierId, TypeOfOrder? filter, OpenOrderInListField? sort);

        IEnumerable<DO.Delivery> ReadAll(Func<DO.Delivery, bool>? customPredicate = null);

        Task<GoogleMapsService.RouteInfo?> GetRouteFromStore(double destLat, double destLng, TheTypeShipment shipmentType);

        Task<string?> GetStaticMapUrlFromStore(double destLat, double destLng, TheTypeShipment shipmentType, int width = 400, int height = 300);
    }
}
