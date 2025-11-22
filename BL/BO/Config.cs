namespace BO;
public class Config
{
   
    //TO_DO: //stage 4
    //add props from DalApi.IConfig that we want to show/change in PL
    //...
     

    /// <summary>
    /// Gets or sets the system clock time.
    /// </summary>
    public  DateTime Clock { get; set; }


    public int ManagerId {  get; set; }

    /// <summary>
    /// Gets or sets the manager's password.
    /// </summary>
    public string PasswordManager { get; set; }

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
    /// Gets or sets the maximum delivery range in distance units.
    /// </summary>
    public double? MaxDeliveryRange { get; set; } = null;

    /// <summary>
    /// Gets or sets the average speed for car deliveries.
    /// </summary>
    public double AvgSpeedCar { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for motorcycle deliveries.
    /// </summary>
    public double AvgSpeedMotorcycle { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for bike deliveries.
    /// </summary>
    public double AvgSpeedBike { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the average speed for foot deliveries.
    /// </summary>
    public double AvgSpeedFoot { get; set; } = 00.0;

    /// <summary>
    /// Gets or sets the maximum time allowed for a delivery.
    /// </summary>
    public TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the time range that indicates a delivery is at risk of being late.
    /// </summary>
    public TimeSpan RiskRange { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Gets or sets the maximum time of inactivity allowed.
    /// </summary>
    public TimeSpan MaxTimeInactivity { get; set; } = TimeSpan.FromDays(0);

    /// <summary>
    /// Resets all configuration values to their default initial state.
    /// </summary>
    public void Reset()
    {
       
        Clock = DateTime.Now;
        //manager_id = StartManagerId;
        PasswordManager = "Admin1234$";
        StoreAddress = null;
        Latitude = null;
        Longitude = null;
        MaxDeliveryRange = null;
        AvgSpeedCar = 00.0;
        AvgSpeedMotorcycle = 00.0;
        AvgSpeedBike = 00.0;
        AvgSpeedFoot = 00.0;
        MaxDeliveryTime = TimeSpan.FromDays(0);
        RiskRange = TimeSpan.FromDays(0);
        MaxTimeInactivity = TimeSpan.FromDays(0);
    }


}

