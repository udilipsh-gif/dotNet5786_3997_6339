namespace BO;

/// <summary>
/// Represents a courier in the business logic layer who delivers orders to customers.
/// </summary>
/// <remarks>
/// This class contains all the information about a courier including their contact details,
/// delivery capabilities, and work status.
/// </remarks>
internal class Courier
{
    /// <summary>
    /// Gets the unique identifier for the courier.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the courier's full name.
    /// </summary>
    /// <value>The courier's name, or null if not set.</value>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the courier's phone number.
    /// </summary>
    /// <value>The courier's contact phone number, or null if not set.</value>
    public required string Phone { get; set; }

    /// <summary>
    /// Gets or sets the courier's email address.
    /// </summary>
    /// <value>The courier's email address for electronic communication, or null if not set.</value>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the courier's password for authentication.
    /// </summary>
    /// <value>The courier's password, or null if not set.</value>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the courier is currently active and available for deliveries.
    /// </summary>
    /// <value>true if the courier is active; otherwise, false.</value>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the maximum distance (in kilometers) that the courier can travel for a delivery.
    /// </summary>
    /// <value>The maximum delivery distance in kilometers, or null if not specified.</value>
    public double? MaxDistanceDelivery { get; set; }

    /// <summary>
    /// Gets or sets the type of transportation method used by the courier for deliveries.
    /// </summary>
    /// <value>A <see cref="TheTypeShipment"/> value indicating the shipment method (CAR, MOTORCYCLE, BIKE, or FOOT).</value>
    public TheTypeShipment TypeShipment { get; set; }

    /// <summary>
    /// Gets the date and time when the courier started working.
    /// </summary>
    /// <value>A DateTime representing when the courier began their employment.</value>
    public DateTime WorkingSince { get; init; }

    public int DeliveryOnTime { get; init; } = 0;

    public int DeliveryLate { get; init; } = 0;

    public OrderInProgress? OrderInProgress { get; set; }

    /// <summary>
    /// Returns a string representation of the courier's details.
    /// </summary>
    /// <returns>A formatted string containing all courier information including ID, name, contact details, status, and capabilities.</returns>
    public override string ToString() => (@$"
    Courier Details:
        ID: {this.Id}
        Name: {this.Name}
        Phone: {this.Phone}
        Email: {this.Email}
        Active: {this.Active}
        Max Distance Delivery: {this.MaxDistanceDelivery:F1}
        Type Shipment: {this.TypeShipment}
        Working Since: {this.WorkingSince}
    ");

    public List<CourierInList> CourierInLists { get; set; } = new List<CourierInList>();
}
