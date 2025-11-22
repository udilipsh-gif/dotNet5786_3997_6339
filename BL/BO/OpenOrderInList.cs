namespace BO;

/// <summary>
/// Represents an open order in a list view, available for courier assignment.
/// </summary>
/// <remarks>
/// This class provides information about orders that are open and awaiting delivery assignment,
/// including distance calculations, time constraints, and delivery feasibility metrics.
/// Used primarily for matching orders with available couriers.
/// </remarks>
public class OpenOrderInList
{
    /// <summary>
    /// Gets the unique identifier of the courier assigned to this order.
    /// </summary>
    /// <value>The courier's ID if a courier has been assigned, or null if the order is still unassigned.</value>
    public int? CourierId { get; init; }
    
    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int OrderId { get; init; }
    
    /// <summary>
    /// Gets the type of order based on delivery speed requirements.
    /// </summary>
    /// <value>A <see cref="TypeOfOrder"/> value indicating whether this is a standard, fast, or immediate delivery.</value>
    public TypeOfOrder TypeOfOrder { get; init; }
    
    /// <summary>
    /// Gets the weight of the order.
    /// </summary>
    /// <value>The weight in kilograms.</value>
    public required double Weight { get; init; }
    
    /// <summary>
    /// Gets the delivery address for the order.
    /// </summary>
    /// <value>The street address where the order should be delivered.</value>
    public required string Address { get; init; }
    
    /// <summary>
    /// Gets the calculated straight-line distance from the store to the delivery location.
    /// </summary>
    /// <value>The delivery distance in kilometers based on geographical coordinates.</value>
    public required double DistanceKm { get; init; }
    
    /// <summary>
    /// Gets the actual travel distance for the delivery route.
    /// </summary>
    /// <value>The actual route distance in kilometers, or null if not yet calculated or measured.</value>
    /// <remarks>
    /// This value may differ from <see cref="DistanceKm"/> as it accounts for actual road routes
    /// rather than straight-line distance.
    /// </remarks>
    public double? ActualDistance { get; init; }
    
    /// <summary>
    /// Gets the estimated time required to complete the delivery.
    /// </summary>
    /// <value>A TimeSpan representing the estimated delivery duration, or null if not yet calculated.</value>
    /// <remarks>
    /// This estimate is based on distance, courier vehicle type, and average travel speeds.
    /// </remarks>
    public TimeSpan? EstimatedDeliveryTime { get; init; }
    
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
    /// Gets the maximum acceptable delivery date and time for the order.
    /// </summary>
    /// <value>A DateTime representing the deadline by which the order must be delivered.</value>
    public required DateTime MaxDeliveryTime { get; init; }
}
