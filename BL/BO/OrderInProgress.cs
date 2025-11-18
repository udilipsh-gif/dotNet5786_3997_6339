using DO;

namespace BO;


/// <summary>
/// Represents an order that is currently being delivered by a courier.
/// </summary>
/// <remarks>
/// This class contains real-time information about an active delivery, including customer details,
/// location information, timing metrics, and status tracking. It is used to monitor and manage
/// deliveries that are currently in progress.
/// </remarks>
internal class OrderInProgress
{
    /// <summary>
    /// Gets the unique identifier for the delivery.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int DeliveryId { get; init; }
    
    /// <summary>
    /// Gets the unique identifier for the order being delivered.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int OrderId { get; init; }
    
    /// <summary>
    /// Gets the type of order based on delivery speed requirements.
    /// </summary>
    /// <value>A <see cref="DO.TypeOfOrder"/> value indicating whether this is a standard, fast, or immediate delivery.</value>
    public required TypeOfOrder TypeOfOrder { get; init; }
    
    /// <summary>
    /// Gets additional details or special instructions for the order.
    /// </summary>
    /// <value>Optional text containing order details or customer instructions, or null if not specified.</value>
    public string? Details { get; init; }
    
    /// <summary>
    /// Gets the delivery address for the order.
    /// </summary>
    /// <value>The street address where the order is being delivered.</value>
    public required string Address { get; init; }
    
    /// <summary>
    /// Gets the calculated straight-line distance from the store to the delivery location.
    /// </summary>
    /// <value>The delivery distance in kilometers based on geographical coordinates.</value>
    public required double Distance { get; init; }
    
    /// <summary>
    /// Gets the actual travel distance for the delivery route.
    /// </summary>
    /// <value>The actual route distance in kilometers, or null if not yet measured.</value>
    /// <remarks>
    /// This value may differ from <see cref="Distance"/> as it accounts for actual road routes
    /// rather than straight-line distance. It is typically recorded when the delivery is completed.
    /// </remarks>
    public double? ActualDistance { get; init; }
    
    /// <summary>
    /// Gets the customer's name.
    /// </summary>
    /// <value>The full name of the customer receiving the order.</value>
    public required string CustomerName { get; init; }
    
    /// <summary>
    /// Gets the customer's phone number.
    /// </summary>
    /// <value>The contact phone number for the customer.</value>
    public required string CustomerPhone { get; init; }
    
    /// <summary>
    /// Gets the date and time when the order was placed.
    /// </summary>
    /// <value>A DateTime representing when the customer created the order.</value>
    public required DateTime OrderTime { get; init; }
    
    /// <summary>
    /// Gets the date and time when the delivery started.
    /// </summary>
    /// <value>A DateTime representing when the courier began delivering this order.</value>
    public required DateTime StartDeliveryTime { get; init; }
    
    /// <summary>
    /// Gets the estimated delivery completion time.
    /// </summary>
    /// <value>A DateTime representing when the order is expected to be delivered.</value>
    /// <remarks>
    /// This estimate is based on distance, courier vehicle type, average travel speeds, and current time.
    /// </remarks>
    public required DateTime EstimatedDeliveryTime { get; init; }
    
    /// <summary>
    /// Gets the maximum acceptable delivery time for the order.
    /// </summary>
    /// <value>A DateTime representing the deadline by which the order must be delivered.</value>
    public required DateTime MaxDeliveryTime { get; init; }
    
    /// <summary>
    /// Gets the current status of the order in the delivery process.
    /// </summary>
    /// <value>An <see cref="OrderStatus"/> value indicating the order's current state (typically DELIVERING for orders in progress).</value>
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
    /// <remarks>
    /// This value is dynamically calculated and helps couriers prioritize deliveries.
    /// A negative value indicates the order is already late.
    /// </remarks>
    public required TimeSpan TimeRemaining { get; init; }
}
