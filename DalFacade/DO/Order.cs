namespace DO;

/// <summary>
/// Represents an order with details such as location, contact information, and order type.
/// </summary>
/// <remarks>This record is used to encapsulate the details of an order, including its unique identifier,  type,
/// location coordinates, contact information, and other relevant details.  It is designed to be immutable for certain
/// properties (e.g., <see cref="Id"/>, <see cref="Latitude"/>,  <see cref="Longitude"/>, and <see cref="OrderData"/>),
/// while allowing updates to others.</remarks>

public record Order
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the order.
    /// </summary>
    public required TypeOfOrder TypeOfOrder { get; set; }

    /// <summary>
    /// Gets or sets additional details about the order.
    /// </summary>
    public string? Details { get; set; }
    /// <summary>
    /// Gets or sets the address for the order delivery.
    /// </summary>
    public required string Addres { get; set; }
    /// <summary>
    /// Gets or sets the latitude coordinate for the order location.
    /// </summary>
    public required double Latitude { get; init; }
    /// <summary>
    /// Gets or sets the longitude coordinate for the order location.
    /// </summary>
    public required double Longitude { get; init; }
    /// <summary>
    /// Gets or sets the name of the recipient for the order.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the phone number of the recipient for the order.
    /// </summary>
    public required string Phone { get; set; }
    /// <summary>
    /// Gets or sets the weight of the order.
    /// </summary>
    public required int Weight { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the order was placed.
    /// </summary>
    public required DateTime OrderDate { get; init; }
    /// <summary>
    /// Gets or sets the current status of the order.
    /// </summary>
    public OrderStatus OrderStatus { get; set; } = OrderStatus.OPEN;
    /// <summary>
    /// Gets or sets the distance to the order location in kilometers.
    /// </summary>
   
    ///
    
    public double? DistanceKm { get; set; } = null;
    /// <summary>
    /// Gets or sets the road distance to the order location in kilometers.
    /// </summary>
    public double? DistanceKmRoad { get; init; } = null;
    /// <summary>
    /// Gets or sets the walking distance to the order location in kilometers.
    /// </summary>
    public double? DistanceKmWalk { get; init; } = null;
    /// <summary>
    /// Gets a string representation of the order details.
    /// </summary>
    /// <returns>Returns a formatted string containing the order details.
    /// id, TypeOfOrder, Details, Addres, Name, Phone, Weight, OrderDate, OrderStatus, DistanceKm.
    /// </returns>
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