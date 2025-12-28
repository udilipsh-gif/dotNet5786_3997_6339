using Helpers;

namespace BO;

/// <summary>
/// Represents a courier in the business logic layer who delivers orders to customers.
/// </summary>
/// <remarks>
/// This class contains the courier identity, contact information, delivery capabilities,
/// activity status, and optional real-time delivery state.
/// </remarks>
public class Courier
{
    /// <summary>
    /// Gets the unique identifier for the courier.
    /// </summary>
    /// <value>A unique integer ID that identifies the courier.</value>
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the courier's full name.
    /// </summary>
    /// <value>The courier's name.</value>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the courier's phone number.
    /// </summary>
    /// <value>The courier's contact phone number.</value>
    public required string Phone { get; set; }

    /// <summary>
    /// Gets or sets the courier's email address.
    /// </summary>
    /// <value>The courier's email address for electronic communication.</value>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the courier's password for authentication.
    /// </summary>
    /// <value>The courier's password (plain text in the current implementation).</value>
    /// <remarks>
    /// Storing raw passwords is not recommended. Prefer storing a salted hash.
    /// </remarks>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the courier is currently active and available for deliveries.
    /// </summary>
    /// <value><c>true</c> if the courier is active; otherwise, <c>false</c>.</value>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the maximum distance (in kilometers) that the courier can travel for a delivery.
    /// </summary>
    /// <value>The maximum delivery distance in kilometers, or null if not specified.</value>
    public double? MaxDistanceDelivery { get; set; }

    /// <summary>
    /// Gets or sets the type of transportation method used by the courier for deliveries.
    /// </summary>
    /// <value>A <see cref="TheTypeShipment"/> value indicating the shipment method (car, motorcycle, bike, or foot).</value>
    public TheTypeShipment TypeShipment { get; set; }

    /// <summary>
    /// Gets the date and time when the courier started working.
    /// </summary>
    /// <value>A DateTime representing when the courier began their employment.</value>
    public required DateTime WorkingSince { get; init; }

    /// <summary>
    /// Gets the number of deliveries completed on time.
    /// </summary>
    /// <value>The count of deliveries completed within the maximum delivery time.</value>
    public required int DeliveryOnTime { get; init; } = 0;

    /// <summary>
    /// Gets the number of deliveries completed late.
    /// </summary>
    /// <value>The count of deliveries completed after the maximum delivery time.</value>
    public required int DeliveryLate { get; init; } = 0;

    /// <summary>
    /// Gets or sets the current active order/delivery information for the courier.
    /// </summary>
    /// <value>
    /// An <see cref="OrderInProgress"/> instance describing the delivery currently handled by the courier,
    /// or null if the courier has no active delivery.
    /// </value>
    public OrderInProgress? OrderInProgress { get; set; }

    /// <summary>
    /// Returns a string representation of the courier with all property values.
    /// </summary>
    /// <returns>A formatted string containing all courier properties and their values.</returns>
    public override string ToString() => this.ToStringProperty();

    /// <summary>
    /// Gets or sets a list of couriers intended for list views.
    /// </summary>
    /// <value>A list of <see cref="CourierInList"/> items. Defaults to an empty list.</value>
    /// <remarks>
    /// This property is typically not expected to be part of the Courier entity itself.
    /// If it is not used, consider removing it to avoid confusion.
    /// </remarks>
    public List<CourierInList> CourierInLists { get; set; } = new List<CourierInList>();
}
