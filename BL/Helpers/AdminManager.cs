//using BO;
using DO;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4


        if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        {
            _periodicTask = Task.Run(() =>
            {
                // קריאה לפונקציה החדשה שיצרנו ב-Tools
                bool dataChanged = Tools.PeriodicSystemUpdates();

                // אם בוצעו שינויים בנתונים (למשל שליח הפך ללא פעיל), נרצה להודיע על כך
                if (dataChanged)
                {
                    // אופציונלי: קריאה לאירוע עדכון קונפיגורציה או אירוע ייעודי אחר לריענון רשימות
                    //ConfigUpdatedObservers?.Invoke();
                    //CourierManager.Observer.NotifyListUpdated();
                }
                //OrderManager.Observer.NotifyListUpdated();
            });
        }

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => StudentManager.PeriodicStudentsUpdates(oldClock, newClock));
        //...

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4//i did, yuda
    => new BO.Config()
    {
        ManagerId = s_dal.Config.ManagerId,
        PasswordManager = s_dal.Config.PasswordManager,
        StoreAddress = s_dal.Config.StoreAddress,
        Latitude = s_dal.Config.Latitude,
        Longitude = s_dal.Config.Longitude,
        MaxDeliveryRange = s_dal.Config.MaxDeliveryRange,
        AvgSpeedBike = s_dal.Config.AvgSpeedBike,
        AvgSpeedCar = s_dal.Config.AvgSpeedCar,
        AvgSpeedFoot = s_dal.Config.AvgSpeedFoot,
        AvgSpeedMotorcycle = s_dal.Config.AvgSpeedMotorcycle,
        MaxDeliveryTime = s_dal.Config.MaxDeliveryTime,
        RiskRange = s_dal.Config.RiskRange,
        MaxTimeInactivity = s_dal.Config.MaxTimeInactivity,
        GoogleApiKey = s_dal.Config.GoogleApiKey,
        EmailAddress = s_dal.Config.EmailAddress,
        ScriptUrl = s_dal.Config.ScriptUrl,
        ScriptPass = s_dal.Config.ScriptPass,
        TokenCallSms = s_dal.Config.TokenCallSms


    };

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        bool configChanged = false; // stage 5

                                    //if (s_dal.Config.MaxRange != configuration.MaxRange) //stage 4
                                    //{
                                    //    s_dal.Config.MaxRange = configuration.MaxRange;
                                    //    configChanged = true;
                                    //}
        //TO_DO: //stage 4//i did, yuda
        //add a condition+assignment for each configuration property
        //...
        if (s_dal.Config.ManagerId != configuration.ManagerId)
        {
            if(!Tools.IsValidId(configuration.ManagerId)) 
                throw new BO.BlInvalidValueException("Invalid Manager ID.");
            s_dal.Config.ManagerId = configuration.ManagerId;
            configChanged = true;
        }
        if (s_dal.Config.PasswordManager != configuration.PasswordManager)
        {
            s_dal.Config.PasswordManager = configuration.PasswordManager;
            configChanged = true;
        }
        if (s_dal.Config.StoreAddress != configuration.StoreAddress)
        {
            string address = configuration.StoreAddress ?? throw new BO.BlInvalidValueException("Store address cannot be null.");
            (double Lat, double Lon)? adressCoordinates = GoogleMapsService.GetGeocodingSync(address) ??
                throw new BO.BlInvalidValueException("Geocoding failed.");
            configuration.Latitude = adressCoordinates?.Lat;
            configuration.Longitude = adressCoordinates?.Lon;
            s_dal.Config.StoreAddress = configuration.StoreAddress;
            s_dal.Config.Latitude = adressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            s_dal.Config.Longitude = adressCoordinates?.Lon ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            configChanged = true;
        }
        if (s_dal.Config.Latitude != configuration.Latitude)
        {
            s_dal.Config.Latitude = configuration.Latitude;
            configChanged = true;
        }
        if (s_dal.Config.Longitude != configuration.Longitude)
        {
            s_dal.Config.Longitude = configuration.Longitude;
            configChanged = true;
        }
        if (s_dal.Config.MaxDeliveryRange != configuration.MaxDeliveryRange)
        {
            s_dal.Config.MaxDeliveryRange = configuration.MaxDeliveryRange;
            configChanged = true;
        }
        if (s_dal.Config.AvgSpeedCar != configuration.AvgSpeedCar)
        {
            s_dal.Config.AvgSpeedCar = configuration.AvgSpeedCar;
            configChanged = true;
        }
        if (s_dal.Config.AvgSpeedMotorcycle != configuration.AvgSpeedMotorcycle)
        {
            s_dal.Config.AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            configChanged = true;
        }
        if (s_dal.Config.AvgSpeedBike != configuration.AvgSpeedBike)
        {
            s_dal.Config.AvgSpeedBike = configuration.AvgSpeedBike;
            configChanged = true;
        }
        if (s_dal.Config.AvgSpeedFoot != configuration.AvgSpeedFoot)
        {
            s_dal.Config.AvgSpeedFoot = configuration.AvgSpeedFoot;
            configChanged = true;
        }
        if (s_dal.Config.MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }
        if (s_dal.Config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (s_dal.Config.MaxTimeInactivity != configuration.MaxTimeInactivity)
        {
            s_dal.Config.MaxTimeInactivity = configuration.MaxTimeInactivity;
            configChanged = true;
        }
        if (s_dal.Config.GoogleApiKey != configuration.GoogleApiKey)
        {
            s_dal.Config.GoogleApiKey = configuration.GoogleApiKey;
            configChanged = true;
        }
        if (s_dal.Config.EmailAddress != configuration.EmailAddress)
        {
            s_dal.Config.EmailAddress = configuration.EmailAddress;
            configChanged = true;
        }
        if (s_dal.Config.ScriptUrl != configuration.ScriptUrl)
        {
            s_dal.Config.ScriptUrl = configuration.ScriptUrl;
            configChanged = true;
        }
        if (s_dal.Config.ScriptPass != configuration.ScriptPass)
        {
            s_dal.Config.ScriptPass = configuration.ScriptPass;
            configChanged = true;
        }
        if (s_dal.Config.TokenCallSms != configuration.TokenCallSms)
        {
            s_dal.Config.TokenCallSms = configuration.TokenCallSms;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
      //  if (s_thread is not null)
         //   throw new BO.BLTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation


           // if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
             //   _simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}
