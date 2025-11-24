namespace BO;

/// <summary>
/// Represents a customer order in the business logic layer.
/// </summary>
/// <remarks>
/// This class contains all information about an order including customer details,
/// delivery location, timing constraints, status tracking, and associated deliveries.
/// </remarks>
public class Order
{
    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public int Id { get; init; }
    
    /// <summary>
    /// Gets the type of order based on delivery speed requirements.
    /// </summary>
    /// <value>A <see cref="DO.TypeOfOrder"/> value indicating whether this is a standard, fast, or immediate delivery.</value>
    public required TypeOfOrder TypeOfOrder { get; init; }
    
    /// <summary>
    /// Gets or sets additional details or special instructions for the order.
    /// </summary>
    /// <value>Optional text containing order details, or null if not specified.</value>
    public string? Details { get; set; }
    
    /// <summary>
    /// Gets or sets the delivery address for the order.
    /// </summary>
    /// <value>The street address where the order should be delivered.</value>
    public required string Addres { get; set; }

    /// <summary>
    /// Gets or sets the weight value.
    /// </summary>
    public required int Weight { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate of the delivery location.
    /// </summary>
    /// <value>The latitude in decimal degrees.</value>
    public required double Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the longitude coordinate of the delivery location.
    /// </summary>
    /// <value>The longitude in decimal degrees.</value>
    public required double Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets the distance from the store to the delivery location.
    /// </summary>
    /// <value>The delivery distance in kilometers.</value>
    public required double Distance { get; set; }
    
    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    /// <value>The full name of the customer placing the order.</value>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    /// <value>The contact phone number for the customer.</value>
    public required string Phone { get; set; }
    
    /// <summary>
    /// Gets or sets the weight of the order.
    /// </summary>
    /// <value>The weight in kilograms, or null if not specified.</value>
   
    
    /// <summary>
    /// Gets the date and time when the order was placed.
    /// </summary>
    /// <value>A DateTime representing when the customer created the order.</value>
    public required DateTime OrderDate { get; init; }
    
    /// <summary>
    /// Gets or sets the estimated delivery time for the order.
    /// </summary>
    /// <value>A DateTime representing when the order is expected to be delivered.</value>
    public required DateTime EstimatedDeliveryTime { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum acceptable delivery time for the order.
    /// </summary>
    /// <value>A DateTime representing the deadline by which the order must be delivered.</value>
    public required DateTime MaxDeliveryTime { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order in the delivery process.
    /// </summary>
    /// <value>An <see cref="OrderStatus"/> value indicating whether the order is open, delivering, completed, refused, or cancelled.</value>
    public required OrderStatus OrderStatus { get; set; } 

    /// <summary>
    /// Gets or sets the schedule status indicating if the delivery is on time, at risk, or late.
    /// </summary>
    /// <value>A <see cref="ScheduleStatus"/> value representing the delivery timeline status.</value>
    public required ScheduleStatus ScheduleStatus { get; set; }

    /// <summary>
    /// Gets or sets the time remaining until the maximum delivery deadline.
    /// </summary>
    /// <value>A TimeSpan representing how much time is left before the order becomes late.</value>
    public required TimeSpan TimeLeftForDelivery { get; set; }

    /// <summary>
    /// Gets or sets the list of delivery attempts associated with this order.
    /// </summary>
    /// <value>A list of <see cref="DeliveryPerInList"/> objects representing all delivery attempts for this order.</value>
    /// <remarks>
    /// This collection tracks the history of delivery assignments and attempts for the order.
    /// Initialized to an empty list by default.
    /// </remarks>
    public List<DeliveryPerOrderInList> DeliveryPerOrderInLists { get; set; } = new List<DeliveryPerOrderInList>();//= new List<Delivery>();
}
