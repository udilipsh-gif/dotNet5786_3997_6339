

namespace DalApi;


public interface IConfig
{
    DateTime Clock { get; set; }
    int ManagerId { get; set; }
    int MaxRange { get; set; }
    string PasswordManager { get; set; }
    string storeAddress { get; set; }
    double maxDeliveryRange { get; set; }
    double AvgSpeedCar { get; set; }
    double AvgSpeedMotorcycle { get; set; }
    double AvgSpeedBike { get; set; }
    double AvgSpeedFoot { get; set; }   
    TimeSpan MaxDeliveryTime { get; set; }
    TimeSpan RiskRange { get; set; }
    TimeSpan MaxTimeInactivity { get; set; }
    void Reset();
}
