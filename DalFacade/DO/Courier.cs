namespace DO;

/// <summary>
/// Represents a courier responsible for delivering packages, including their personal details,  delivery preferences,
/// and operational status.
/// </summary>
/// <remarks>This record encapsulates information about a courier, such as their contact details,  delivery
/// capabilities, and working status. It is designed to be used in systems that manage  delivery personnel and their
/// assignments.</remarks>
public record Courier
{
    /// <summary>
    /// Gets or sets the unique identifier for the courier.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// Gets or sets the name of the courier.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the phone number of the courier.
    /// </summary>
    public required string Phone { get; set; }
    /// <summary>
    /// Gets or sets the email address of the courier.
    /// </summary>
    public required string Email { get; set; }
    /// <summary>
    /// Gets or sets the password for the courier's account.
    /// </summary>
    public required string Password { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the courier is currently active and available for deliveries.
    /// </summary>
    public required bool Active { get; set; }
    /// <summary>
    /// Gets or sets the maximum distance (in kilometers) that the courier is willing to deliver.
    /// </summary>
    public double? MaxDistanceDelivery { get; set; }
    /// <summary>
    /// Gets or sets the type of shipment the courier is capable of handling (e.g., bike, motorcycle, car).
    /// </summary>
    public required TheTypeShipment TypeShipment { get; set; }
    /// <summary>
    /// Gets or sets the date and time since the courier has been working.
    /// </summary>
    public required DateTime WorkingSince { get; init; }
    /// <summary>
    /// Gets a string representation of the courier details.
    /// </summary>
    /// <returns>Returns a formatted string containing the courier information.
    /// id, Name, Phone, Email, Active, MaxDistanceDelivery, TypeShipment, WorkingSince.
    /// </returns>
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
}

