namespace DalApi;

/// <summary>
/// Interface IConfig defines the configuration settings for the delivery system.
/// </summary>
/// <remarks>
/// Provides centralized access to delivery system parameters including location,
/// vehicle speeds, and operational time constraints.
/// </remarks>
/// <param name="Clock"> The current system time.</param>
/// <param name="ManagerId"> The unique identifier for the manager.</param>
/// <param name="PasswordManager"> The password for the manager's account.</param>
/// <param name="storeAddress"> The physical address of the store (optional).</param>
/// <param name="Latitude"> The latitude coordinate of the store (optional).</param>
/// <param name="Longitude"> The longitude coordinate of the store (optional).</param>
/// <param name="MaxDeliveryRange"> The maximum delivery range in kilometers (optional).</param>
/// <param name="AvgSpeedCar"> The average speed of delivery vehicles using cars in km/h.</param>
/// <param name="AvgSpeedMotorcycle"> The average speed of delivery vehicles using motorcycles in km/h.</param>
/// <param name="AvgSpeedBike"> The average speed of delivery vehicles using bikes in km/h.</param>
/// <param name="AvgSpeedFoot"> The average speed of delivery personnel on foot in km/h.</param>
/// <param name="MaxDeliveryTime"> The maximum allowable time for deliveries.</param>
/// <param name="RiskRange"> The time range considered risky for deliveries.</param>
/// <param name="MaxTimeInactivity"> The maximum time a courier can be inactive before being flagged.</param>

public interface IConfig
{
    DateTime Clock { get; set; }
    int ManagerId { get; set; }
    string PasswordManager { get; set; }
    string? storeAddress { get; set; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }
    double? MaxDeliveryRange { get; set; }
    double AvgSpeedCar { get; set; }
    double AvgSpeedMotorcycle { get; set; }
    double AvgSpeedBike { get; set; }
    double AvgSpeedFoot { get; set; }   
    TimeSpan MaxDeliveryTime { get; set; }
    TimeSpan RiskRange { get; set; }
    TimeSpan MaxTimeInactivity { get; set; }
    void Reset();
}
