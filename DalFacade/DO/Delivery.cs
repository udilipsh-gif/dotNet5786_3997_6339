namespace DO;
/// <summary>
/// Represents the details of a delivery, including its associated order, courier, and delivery status.
/// </summary>
/// <remarks>This class encapsulates information about a delivery, such as the order it is associated with,  the
/// courier responsible for the delivery, the type of order, and the delivery's current status. It also includes
/// optional details like the actual distance traveled and the time the delivery was completed.</remarks>

public record Delivery
{
    /// <summary>
    /// Gets or sets the unique identifier for the delivery.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// Gets or sets the unique identifier for the associated order.
    /// </summary>
    public required int OrderId { get; init; }
    /// <summary>
    /// Gets or sets the unique identifier for the courier responsible for the delivery.
    /// </summary>
    public required int CourierId { get; init; }
    /// <summary>
    /// Gets or sets the type of shipment being delivered.
    /// </summary>
    public required TheTypeShipment TypeShipment { get; init; }
    /// <summary>
    /// Gets or sets the date and time when the order was placed.
    /// </summary>
    public required DateTime OrderDate { get; init; }
    /// <summary>
    /// Gets or sets the actual distance traveled for the delivery, if available.
    /// </summary>
    public double? ActualDistance { get; init; }
    /// <summary>
    /// Gets or sets the end delivery status, if the delivery has been completed.
    /// </summary>
    public EndDelivery? EndDelivery { get; init; } = null;
    /// <summary>
    /// Gets or sets the date and time when the delivery was completed, if available.
    /// </summary>
    public DateTime? TimeEndDelivery { get; init; } = null;
    /// <summary>
    /// Gets a string representation of the delivery details.
    /// </summary>
    /// <returns>Returns a formatted string containing the delivery information.
    /// id, OrderId, CourierId, TypeOfOrder, OrderDate, ActualDistance, EndDelivery, TimeEndDelivery.
    /// </returns>
    public override string ToString() => ($@"
    Delivery Details:
        Delivery ID: {Id}
        Order ID: {OrderId}
        Courier ID: {CourierId}
        Type of Order: {TypeOfOrder}
        Order Date: {OrderDate}
        Actual Distance: {ActualDistance}
        End Delivery: {EndDelivery}
        Time End Delivery: {TimeEndDelivery}
    ");
}