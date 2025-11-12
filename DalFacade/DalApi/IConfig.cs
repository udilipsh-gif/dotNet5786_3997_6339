namespace DalApi;
/// <summary>
/// Represents a configuration interface for managing delivery settings and operational parameters.
/// </summary>
/// <remarks>This interface provides properties to configure various aspects of a delivery system, such as
/// location, speed, and time constraints. It also includes a method to reset the configuration to its default
/// state.</remarks>
public interface IConfig
{
    /// <summary>
    /// Gets or sets the current time represented by the clock.
    /// </summary>
    DateTime Clock { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the manager.
    /// </summary>
    int ManagerId { get; set; }
    /// <summary>
    /// Gets or sets the password manager used for storing and retrieving user credentials.
    /// </summary>
    string PasswordManager { get; set; }
    /// <summary>
    /// Gets or sets the address of the store.
    /// </summary>
    string? storeAddress { get; set; }
    /// <summary>
    /// Gets or sets the latitude coordinate of the location.
    /// </summary>
    double? Latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude coordinate of the location.
    /// </summary>
    double? Longitude { get; set; }
    /// <summary>
    /// Gets or sets the maximum delivery range for the service in kilometers.
    /// </summary>
    double? MaxDeliveryRange { get; set; }
    /// <summary>
    /// Gets or sets the average speed of the car in kilometers per hour.
    /// </summary>
    double AvgSpeedCar { get; set; }
    /// <summary>
    /// Gets or sets the average speed of the motorcycle in kilometers per hour.
    /// </summary>
    double AvgSpeedMotorcycle { get; set; }
    /// <summary>
    /// Gets or sets the average speed of the bike in kilometers per hour.
    /// </summary>
    double AvgSpeedBike { get; set; }
    /// <summary>
    /// Gets or sets the average speed of foot delivery in kilometers per hour.
    /// </summary>
    double AvgSpeedFoot { get; set; }
    /// <summary>
    /// Gets or sets the maximum delivery time allowed for an order.
    /// </summary>
    TimeSpan MaxDeliveryTime { get; set; }
    /// <summary>
    /// Gets or sets the risk range time span for deliveries.
    /// </summary>
    TimeSpan RiskRange { get; set; }
    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed.
    /// </summary>
    TimeSpan MaxTimeInactivity { get; set; }
    /// <summary>
    /// Resets the configuration settings to their default values.
    /// </summary>
    void Reset();
}
