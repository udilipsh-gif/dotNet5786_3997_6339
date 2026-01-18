using Helpers;

namespace BO;

/// <summary>
/// Represents a completed delivery in a list view with final outcome details.
/// </summary>
/// <remarks>
/// This class provides information about deliveries that have ended (completed or terminated),
/// including the final status, actual distance traveled, and total delivery time.
/// Used for historical tracking, reporting, and performance analysis.
/// </remarks>
public class ClosedDeliveryInList
{
    /// <summary>
    /// Gets the unique identifier for the delivery.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int DeliveryId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the order that was delivered.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int OrderId { get; init; }

    /// <summary>
    /// Gets the type of order based on delivery speed requirements.
    /// </summary>
    /// <value>A <see cref="TypeOfOrder"/> value indicating whether this was a standard, fast, or immediate delivery.</value>
    public required TypeOfOrder OrderType { get; init; }

    /// <summary>
    /// Gets the delivery address where the order was sent.
    /// </summary>
    /// <value>The street address of the delivery destination.</value>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the shipment type (vehicle type) that was used for this delivery.
    /// </summary>
    /// <value>A <see cref="TheTypeShipment"/> value indicating the vehicle used (car, motorcycle, bike, or foot).</value>
    public required TheTypeShipment ShipmentType { get; init; }

    /// <summary>
    /// Gets the actual travel distance for the completed delivery route.
    /// </summary>
    /// <value>The actual route distance in kilometers, or null if not measured or recorded.</value>
    /// <remarks>
    /// This represents the real distance traveled by the courier, which may differ from
    /// the estimated straight-line distance.
    /// </remarks>
    public double? ActualDistance { get; init; } = null;

    /// <summary>
    /// Gets the total time taken to complete the delivery.
    /// </summary>
    /// <value>A TimeSpan representing the duration from delivery start to completion.</value>
    /// <remarks>
    /// Used for performance metrics and future delivery time estimates.
    /// </remarks>
    public required TimeSpan DeliveryTime { get; init; }

    /// <summary>
    /// Gets the final outcome status of the delivery attempt.
    /// </summary>
    /// <value>
    /// An <see cref="EndDelivery"/> enum value indicating whether the delivery was delivered,
    /// refused, cancelled, not found, or failed.
    /// </value>
    public required EndDelivery EndDelivery { get; init; }

    /// <summary>
    /// Returns a string representation of the current object using reflective property formatting.
    /// </summary>
    /// <returns>A formatted string containing the object's property names and values.</returns>
    public override string ToString() => this.ToStringProperty();
}

