namespace DO;
/// <summary>
/// Represents the details of a delivery, including its associated order, courier, and delivery status.
/// </summary>
/// <remarks>This class encapsulates information about a delivery, such as the order it is associated with,  the
/// courier responsible for the delivery, the type of order, and the delivery's current status. It also includes
/// optional details like the actual distance traveled and the time the delivery was completed.</remarks>
/// <param name="Id"> The unique identifier for the delivery.</param>
/// <param name="OrderId"> The unique identifier of the order associated with the delivery.</param>
/// <param name="CourierId"> The unique identifier of the courier responsible for the delivery.</param>
/// <param name="TypeOfOrder"> The type of the order being delivered (e.g., book, toy, newspaper).</param>
/// <param name="OrderData"> The date and time when the order was placed.</param>
/// <param name="ActualDistance"> The actual distance traveled for the delivery (optional).</param>
/// <param name="EndDelivery"> The current status of the delivery (e.g., pending, in progress, delivered).</param>
/// <param name="TimeEndDelivery"> The date and time when the delivery was completed (optional).</param>
/// <param name="EndDelivery"> The status of the delivery.</param>
public record Delivery
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }
    public required int CourierId { get; init; }
    public required TypeOfOrder TypeOfOrder { get; init; }
    public required DateTime OrderDate { get; init; }
    public double? ActualDistance { get; init; }
    public EndDelivery? EndDelivery { get; init; } = null;
    public DateTime? TimeEndDelivery { get; init; } = null;

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