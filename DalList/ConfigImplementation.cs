

namespace Dal;
using DalApi;
using System.Data;

public class ConfigImplementation : IConfig
{
    public DateTime Clock {
        get => Config.Clock;
        set => Config.Clock = DateTime.Now;
    }
    public int ManagerId {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }
    public string PasswordManager {
        get => Config.PasswordManager;
        set => Config.PasswordManager = value;
    }
    public string? storeAddress {
        get => Config.StoreAddress;
        set => Config.StoreAddress = value;
    }
    public double? maxDeliveryRange {
        get =>Config.MaxDeliveryRange;
        set => Config.MaxDeliveryRange = value;
    }
    public double AvgSpeedCar {
        get => Config.AvgSpeedCar;
        set => Config.AvgSpeedCar = value;
    }
    public double AvgSpeedMotorcycle {
        get => Config.AvgSpeedMotorcycle;
        set => Config.AvgSpeedMotorcycle = value;
    }
    public double AvgSpeedBike {
        get => Config.AvgSpeedBike;
        set => Config.AvgSpeedBike = value;
    }
    public double AvgSpeedFoot { 
        get => Config.AvgSpeedFoot;
        set => Config.AvgSpeedFoot = value;
    }
    public TimeSpan MaxDeliveryTime { 
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }
    public TimeSpan RiskRange {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }
    public TimeSpan MaxTimeInactivity { 
        get => Config.MaxTimeInactivity;
        set => Config.MaxTimeInactivity = value;
    }

    public void Reset()
    {
        Config.Reset();
    }
}
