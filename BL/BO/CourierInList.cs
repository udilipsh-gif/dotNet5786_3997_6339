using Helpers;

namespace BO
{
    /// <summary>
    /// Represents a courier in a list with summary information.
    /// </summary>
    /// <remarks>
    /// This class provides a lightweight list view of couriers, including identity fields
    /// and performance metrics such as on-time and late delivery counts.
    /// </remarks>
    public class CourierInList
    {
        /// <summary>
        /// Gets the unique identifier of the courier.
        /// </summary>
        /// <value>A unique integer ID that identifies the courier.</value>
        public required int Id { get; init; }

        /// <summary>
        /// Gets the courier's display name.
        /// </summary>
        /// <value>The courier name.</value>
        public required string Name { get; init; }

        /// <summary>
        /// Gets a value indicating whether the courier is currently active.
        /// </summary>
        /// <value><c>true</c> if the courier is active; otherwise, <c>false</c>.</value>
        public required bool Active { get; init; }

        /// <summary>
        /// Gets the shipment type (vehicle type) used by the courier.
        /// </summary>
        /// <value>A <see cref="TheTypeShipment"/> value describing the courier vehicle type.</value>
        public required TheTypeShipment TypeShipment { get; init; }

        /// <summary>
        /// Gets the date when the courier started working.
        /// </summary>
        /// <value>A DateTime representing when the courier began working.</value>
        public DateTime WorkingSince { get; init; }

        /// <summary>
        /// Gets the number of deliveries completed on time.
        /// </summary>
        /// <value>The count of deliveries completed within the maximum delivery time.</value>
        public int DeliveryOnTime { get; init; }

        /// <summary>
        /// Gets the number of deliveries completed late.
        /// </summary>
        /// <value>The count of deliveries completed after the maximum delivery time.</value>
        public int DeliveryLate { get; init; }

        /// <summary>
        /// Gets the current delivery identifier if the courier has an active delivery.
        /// </summary>
        /// <value>The delivery ID, or null if the courier has no active delivery.</value>
        public int? DeliveryId { get; init; }

        /// <summary>
        /// Returns a string representation of the current object using reflective property formatting.
        /// </summary>
        /// <returns>A formatted string containing the object's property names and values.</returns>
        public override string ToString() => this.ToStringProperty();

    }
}
