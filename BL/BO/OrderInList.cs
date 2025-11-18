namespace BO
{
    /// <summary>
    /// Represents an order item in a list view with summary information.
    /// </summary>
    /// <remarks>
    /// This class provides a lightweight representation of an order used in list or summary views,
    /// including delivery tracking, status information, and timing metrics. It is optimized for
    /// displaying multiple orders efficiently without loading full order details.
    /// </remarks>
    public class OrderInList
    {
        /// <summary>
        /// Gets the unique identifier of the delivery associated with this order.
        /// </summary>
        /// <value>The delivery ID if a delivery has been assigned, or null if no delivery has been created yet.</value>
        public int? DeliveryId { get; init; } = null;

        /// <summary>
        /// Gets the unique identifier for the order.
        /// </summary>
        /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
        public required int OrderId { get; init; }

        /// <summary>
        /// Gets the type of order based on delivery speed requirements.
        /// </summary>
        /// <value>A <see cref="TypeOfOrder"/> value indicating whether this is a standard, fast, or immediate delivery.</value>
        public required TypeOfOrder TypeOfOrder { get; init; }

        /// <summary>
        /// Gets the distance from the store to the delivery location.
        /// </summary>
        /// <value>The delivery distance in kilometers.</value>
        public required double DistanceKm { get; init; }

        /// <summary>
        /// Gets the current status of the order in the delivery process.
        /// </summary>
        /// <value>An <see cref="OrderStatus"/> value indicating whether the order is open, delivering, completed, refused, or cancelled.</value>
        public required OrderStatus OrderStatus { get; init; }

        /// <summary>
        /// Gets the schedule status indicating if the delivery is on time, at risk, or late.
        /// </summary>
        /// <value>A <see cref="ScheduleStatus"/> value representing the delivery timeline status relative to expected delivery time.</value>
        public required ScheduleStatus ScheduleStatus { get; init; }

        /// <summary>
        /// Gets the time remaining until the maximum delivery deadline.
        /// </summary>
        /// <value>A TimeSpan representing how much time is left before the order becomes late.</value>
        public required TimeSpan TimeLeftForDelivery { get; init; }

        /// <summary>
        /// Gets the total time allocated for delivering this order.
        /// </summary>
        /// <value>A TimeSpan representing the complete duration from order placement to the maximum delivery deadline.</value>
        public required TimeSpan TotalTimeOfDelivery { get; init; }

        /// <summary>
        /// Gets the number of delivery attempts made for this order.
        /// </summary>
        /// <value>An integer count of how many times delivery has been attempted for this order.</value>
        /// <remarks>
        /// This counter increments with each delivery assignment, including failed or refused deliveries.
        /// Multiple attempts may indicate delivery challenges or customer unavailability.
        /// </remarks>
        public required int NumberOfDeliveryAttempts { get; init; }
    }
}
