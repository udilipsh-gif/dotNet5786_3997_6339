namespace DO;

/// <summary>
/// Represents an order with details such as location, contact information, and order type.
/// </summary>
/// <remarks>This record is used to encapsulate the details of an order, including its unique identifier,  type,
/// location coordinates, contact information, and other relevant details.  It is designed to be immutable for certain
/// properties (e.g., <see cref="Id"/>, <see cref="Latitude"/>,  <see cref="Longitude"/>, and <see cref="OrderData"/>),
/// while allowing updates to others.</remarks>
/// <param name="Id"> The unique identifier for the order.</param>
/// <param name="TypeOfOrder"> The type of the order (e.g., book, toy, newspaper).</param>
/// <param name="Details"> Additional details about the order.</param>
/// <param name="addres"> The address where the order is to be delivered.</param>
/// <param name="Latitude"> The latitude coordinate of the delivery location.</param>
/// <param name="Longitude"> The longitude coordinate of the delivery location.</param>
/// <param name="Name"> The name of the recipient.</param>
/// <param name="Phone"> The contact phone number of the recipient.</param>
/// <param name="Weight"> The weight of the order.</param>
/// <param name="OrderData"> The date and time when the order was placed.</param>
/// <param name="OrderStatus"> The current status of the order (e.g., open, in progress, delivered).</param>

public record Order
{
    public required int Id { get; init; }
    public required TypeOfOrder TypeOfOrder { get; set; }
    public string? Details { get; set; }
    public required string Addres { get; set; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required int Weight { get; set; }
    public required DateTime OrderDate { get; init; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.OPEN;
    public double? DistanceKm { get; init; } = null;
    public double? DistanceKmRoad { get; init; } = null;
    public double? DistanceKmWalk { get; init; } = null;
    public override string ToString() => ($@"
    Order Details:
        ID: {Id}
        Type of order: {TypeOfOrder}
        Details: {Details}
        Address: {Addres}
        Name: {Name}
        Phone: {Phone}
        Weight: {Weight}    
        OrderDate: {OrderDate}
        OrderStatus: {OrderStatus}
        Distance Km: {DistanceKm:F1}
    ");
}