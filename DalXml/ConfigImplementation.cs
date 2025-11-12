namespace Dal;
using DalApi;
using System.Data;

/// <summary>
/// Implementation of the IConfig interface that provides access to system configuration settings.
/// Acts as a facade to the internal Config class.
/// </summary>
internal class ConfigImplementation : IConfig
{
    /// <summary>
    /// Gets or sets the current system clock time.
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <summary>
    /// Gets or sets the manager's unique identifier.
    /// Validates the ID using Israeli ID validation algorithm.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the ID is not valid according to Israeli ID validation rules.</exception>
    public int ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }

    /// <summary>
    /// Gets or sets the manager's password for authentication.
    /// </summary>
    public string PasswordManager
    {
        get => Config.PasswordManager;
        set => Config.PasswordManager = value;
    }

    /// <summary>
    /// Gets or sets the physical address of the store.
    /// </summary>
    public string? storeAddress
    {
        get => Config.StoreAddress;
        set => Config.StoreAddress = value;
    }

    /// <summary>
    /// Gets or sets the latitude coordinate of the store location.
    /// </summary>
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }

    /// <summary>
    /// Gets or sets the longitude coordinate of the store location.
    /// </summary>
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }

    /// <summary>
    /// Gets or sets the maximum delivery range in distance units (typically kilometers).
    /// </summary>
    public double? MaxDeliveryRange
    {
        get => Config.MaxDeliveryRange;
        set => Config.MaxDeliveryRange = value;
    }

    /// <summary>
    /// Gets or sets the average speed for car deliveries (in km/h).
    /// </summary>
    public double AvgSpeedCar
    {
        get => Config.AvgSpeedCar;
        set => Config.AvgSpeedCar = value;
    }

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries (in km/h).
    /// </summary>
    public double AvgSpeedMotorcycle
    {
        get => Config.AvgSpeedMotorcycle;
        set => Config.AvgSpeedMotorcycle = value;
    }

    /// <summary>
    /// Gets or sets the average speed for bike deliveries (in km/h).
    /// </summary>
    public double AvgSpeedBike
    {
        get => Config.AvgSpeedBike;
        set => Config.AvgSpeedBike = value;
    }

    /// <summary>
    /// Gets or sets the average speed for foot deliveries (in km/h).
    /// </summary>
    public double AvgSpeedFoot
    {
        get => Config.AvgSpeedFoot;
        set => Config.AvgSpeedFoot = value;
    }

    /// <summary>
    /// Gets or sets the maximum time allowed for completing a delivery.
    /// </summary>
    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed before a courier is flagged.
    /// </summary>
    public TimeSpan MaxTimeInactivity
    {
        get => Config.MaxTimeInactivity;
        set => Config.MaxTimeInactivity = value;
    }

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}
