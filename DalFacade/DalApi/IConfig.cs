namespace DalApi;

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
