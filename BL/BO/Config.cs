using Helpers;

namespace BO;

/// <summary>
/// Represents configuration values exposed by the business logic layer.
/// </summary>
/// <remarks>
/// This model is used to view and update system configuration through the presentation layer.
/// It is a business-level representation of the underlying DAL configuration.
/// </remarks>
public class Config
{
    /// <summary>
    /// Gets or sets the system clock time.
    /// </summary>
    /// <remarks>
    /// This value may be used to simulate time for testing.
    /// </remarks>
    public DateTime Clock { get; set; }

    /// <summary>
    /// Gets or sets the manager unique identifier.
    /// </summary>
    /// <value>The manager ID used for authentication/authorization.</value>
    public int ManagerId { get; set; }

    /// <summary>
    /// Gets or sets the manager's password.
    /// </summary>
    public required string PasswordManager { get; set; }

    /// <summary>
    /// Gets or sets the store address.
    /// </summary>
    public string? StoreAddress { get; set; } = null;

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// </summary>
    public double? Latitude { get; set; } = null;

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// </summary>
    public double? Longitude { get; set; } = null;

    /// <summary>
    /// Gets or sets the maximum delivery range in kilometers.
    /// </summary>
    public double? MaxDeliveryRange { get; set; } = null;

    /// <summary>
    /// Gets or sets the average speed for car deliveries (km/h).
    /// </summary>
    public double AvgSpeedCar { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries (km/h).
    /// </summary>
    public double AvgSpeedMotorcycle { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the average speed for bike deliveries (km/h).
    /// </summary>
    public double AvgSpeedBike { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the average speed for foot deliveries (km/h).
    /// </summary>
    public double AvgSpeedFoot { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the maximum time allowed for a delivery.
    /// </summary>
    public TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the time buffer that indicates a delivery is at risk of being late.
    /// </summary>
    public TimeSpan RiskRange { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed before a courier is flagged.
    /// </summary>
    public TimeSpan MaxTimeInactivity { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the Google API key used to access Google services.
    /// </summary>
    /// <remarks>
    /// Do not commit real API keys to source control.
    /// Prefer loading this value from user secrets, environment variables, or secured configuration.
    /// </remarks>
    public string GoogleApiKey { get; set; } = string.Empty;
    /// <summary>
    /// gets or sets the email address used for system notifications.
    /// </summary>
    public string? EmailAddress { get; set; } = null;
    /// <summary>
    /// gets or sets the URL of the script for external integrations.
    /// </summary>
    public string ScriptUrl { get; set; } = string.Empty;
    /// <summary>
    /// gets or sets the script password used for authentication or encryption purposes.
    /// </summary>
    public string ScriptPass { get; set; } = string.Empty;
    /// <summary>
    /// gets or sets the SMS API token used for sending SMS notifications.
    /// </summary>
    public string TokenCallSms { get; set; } = string.Empty;
    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    public void Reset()
    {
        Clock = DateTime.Now;
        PasswordManager = "Admin1234$";
        StoreAddress = null;
        Latitude = null;
        Longitude = null;
        MaxDeliveryRange = null;
        AvgSpeedCar = 0.0;
        AvgSpeedMotorcycle = 0.0;
        AvgSpeedBike = 0.0;
        AvgSpeedFoot = 0.0;
        MaxDeliveryTime = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        MaxTimeInactivity = TimeSpan.Zero;
        GoogleApiKey = string.Empty;
        EmailAddress = null;
        ScriptUrl = string.Empty;
        ScriptPass = string.Empty;
        TokenCallSms = string.Empty;
    }

    /// <summary>
    /// Returns a string representation of the current configuration with all property values.
    /// </summary>
    /// <returns>A formatted string containing all configuration properties and their values.</returns>
    public override string ToString() => this.ToStringProperty();
}

