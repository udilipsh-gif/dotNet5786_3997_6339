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

public record Order
{
    public int Id { get; init; } 
    public TypeOfOrder TypeOfOrder { get; set; }
    public string? Details { get; set; }
    public  string Addres { get; set; }
    public  double Latitude { get; init; }
    public  double Longitude { get; init; }
    public  string Name { get; set; }
    public  string Phone { get; set; }
    public  int Weight { get; set; }
    public  DateTime OrderData { get; init;}


}
