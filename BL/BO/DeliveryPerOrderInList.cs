using Helpers;

namespace BO
{
    /// <summary>
    /// Represents a delivery item in a list view for order tracking purposes.
    /// </summary>
    /// <remarks>
    /// This class provides a lightweight representation of a delivery associated with an order,
    /// including courier information, timing details, and delivery outcome. It is typically used
    /// in list or summary views where full delivery details are not required.
    /// </remarks>
    public class DeliveryPerOrderInList
    {
        /// <summary>
        /// Gets the unique identifier for the delivery.
        /// </summary>
        /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
        public required int DeliveryId { get; init; }
        
        /// <summary>
        /// Gets the unique identifier of the courier assigned to this delivery.
        /// </summary>
        /// <value>The courier's ID, or null if no courier has been assigned yet.</value>
        public int? CourierId { get; init; }
        
        /// <summary>
        /// Gets the name of the courier assigned to this delivery.
        /// </summary>
        /// <value>The full name of the courier handling the delivery.</value>
        public required string CourierName { get; init; }
        
        /// <summary>
        /// Gets the shipment type (vehicle type) used to perform the delivery.
        /// </summary>
        /// <value>
        /// A <see cref="TheTypeShipment"/> value indicating the courier vehicle used
        /// (car, motorcycle, bike, or foot).
        /// </value>
        public required TheTypeShipment TypeShipment { get; init; }
        
        /// <summary>
        /// Gets the date and time when the delivery was created (order assignment time).
        /// </summary>
        /// <value>A DateTime representing when the delivery was created/started.</value>
        public required DateTime OrderDate { get; init; }
        
        /// <summary>
        /// Gets the final status of the delivery attempt.
        /// </summary>
        /// <value>
        /// An <see cref="EndDelivery"/> enum value indicating the outcome (delivered, refused,
        /// cancelled, not found, or failed), or null if the delivery is still in progress.
        /// </value>
        public EndDelivery? EndDelivery { get; init; }

        /// <summary>
        /// Gets the date and time when the delivery was completed or ended.
        /// </summary>
        /// <value>
        /// A DateTime representing when the delivery attempt concluded, or null if the delivery is still in progress.
        /// </value>
        public DateTime? TimeEndDelivery { get; init; }

        /// <summary>
        /// Returns a string representation of the current object using reflective property formatting.
        /// </summary>
        /// <returns>A formatted string containing the object's property names and values.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
