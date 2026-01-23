//using BO;
using DO;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    private static readonly AsyncMutex s_periodicMutex = new();

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
    internal static void UpdateClock(DateTime newClock) //stage 4-7 **********************************************************************
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        Task.Run(() => PeriodicSystemUpdates(oldClock, newClock));

        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //{
        //    _periodicTask = Task.Run(() =>
        //    {
        //        // קריאה לפונקציה החדשה שיצרנו ב-Tools
        //        _ = Task.Run(() => Tools.PeriodicSystemUpdates());

        //        // אם בוצעו שינויים בנתונים (למשל שליח הפך ללא פעיל), נרצה להודיע על כך
        //        //if (dataChanged)
        //        //{
        //        //    // אופציונלי: קריאה לאירוע עדכון קונפיגורציה או אירוע ייעודי אחר לריענון רשימות
        //        //    //ConfigUpdatedObservers?.Invoke();
        //        //    //CourierManager.Observer.NotifyListUpdated();
        //        //}
        //        //OrderManager.Observer.NotifyListUpdated();
        //    });
        //}

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
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7 ******************************************************************************************
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
       
        ////////////////////////////////////////////////////////////////////////////////////////לא ברור לי מה היתה ההוא אמינא פה , כמו כן בדוגמא של דן הבדיקה כן מול s_dal ולא מול אדמין מנג'ר
        bool configChanged = false; // stage 5

                                    //if (s_dal.Config.MaxRange != configuration.MaxRange) //stage 4
                                    //{
                                    //    s_dal.Config.MaxRange = configuration.MaxRange;
                                    //    configChanged = true;
                                    //}
        //TO_DO: //stage 4//i did, yuda
        //add a condition+assignment for each configuration property
        //...
        if (AdminManager.GetConfig().ManagerId != configuration.ManagerId)
        {
            if(!Tools.IsValidId(configuration.ManagerId)) 
                throw new BO.BlInvalidValueException("Invalid Manager ID.");
            s_dal.Config.ManagerId = configuration.ManagerId;
            //AdminManager.GetConfig().ManagerId = configuration.ManagerId;
            configChanged = true;
        }
        if (AdminManager.GetConfig().PasswordManager != configuration.PasswordManager)
        {
            if (!Tools.IsStrongPassword(configuration.PasswordManager))
                throw new BO.BlInvalidValueException("Weak password for Manager.");
            s_dal.Config.PasswordManager = configuration.PasswordManager;
            //AdminManager.GetConfig().PasswordManager = configuration.PasswordManager;
            configChanged = true;
        }
        if (AdminManager.GetConfig().StoreAddress != configuration.StoreAddress)
        {
            string address = configuration.StoreAddress ?? throw new BO.BlInvalidValueException("Store address cannot be null.");

            string api = configuration.GoogleApiKey ?? throw new BO.BlInvalidValueException("Google API key cannot be null.");
            (double Lat, double Lon)? adressCoordinates =Task.Run(()=> GoogleMapsService.GetGeocodingAsync(address,api)).Result ??
                throw new BO.BlInvalidValueException("Geocoding failed.");
            configuration.Latitude = adressCoordinates?.Lat;
            configuration.Longitude = adressCoordinates?.Lon;

            s_dal.Config.StoreAddress = configuration.StoreAddress;
            //AdminManager.GetConfig().StoreAddress = configuration.StoreAddress;

            s_dal.Config.Latitude = adressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            //AdminManager.GetConfig().Latitude = adressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address.");

            s_dal.Config.Longitude = adressCoordinates?.Lon ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            //AdminManager.GetConfig().Longitude = adressCoordinates?.Lon ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            configChanged = true;
        }
        if (AdminManager.GetConfig().Latitude != configuration.Latitude)
        {
            s_dal.Config.Latitude = configuration.Latitude;
            //AdminManager.GetConfig().Latitude = configuration.Latitude;
            configChanged = true;
        }
        if (AdminManager.GetConfig().Longitude != configuration.Longitude)
        {
            s_dal.Config.Longitude = configuration.Longitude;
            //AdminManager.GetConfig().Longitude = configuration.Longitude;
            configChanged = true;
        }
        if (AdminManager.GetConfig().MaxDeliveryRange != configuration.MaxDeliveryRange)
        {
            s_dal.Config.MaxDeliveryRange = configuration.MaxDeliveryRange;
            //  AdminManager.GetConfig().MaxDeliveryRange = configuration.MaxDeliveryRange;
            configChanged = true;
        }
        if (AdminManager.GetConfig().AvgSpeedCar != configuration.AvgSpeedCar)
        {
            s_dal .Config.AvgSpeedCar = configuration.AvgSpeedCar;
            //AdminManager.GetConfig().AvgSpeedCar = configuration.AvgSpeedCar;
            configChanged = true;
        }
        if (AdminManager.GetConfig().AvgSpeedMotorcycle != configuration.AvgSpeedMotorcycle)
        {
            s_dal.Config.AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            //AdminManager.GetConfig().AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            configChanged = true;
        }
        if (AdminManager.GetConfig().AvgSpeedBike != configuration.AvgSpeedBike)
        {
            s_dal.Config.AvgSpeedBike = configuration.AvgSpeedBike;
            //AdminManager.GetConfig().AvgSpeedBike = configuration.AvgSpeedBike;
            configChanged = true;
        }
        if (AdminManager.GetConfig().AvgSpeedFoot != configuration.AvgSpeedFoot)
        {
            s_dal.Config.AvgSpeedFoot = configuration.AvgSpeedFoot;
            //AdminManager.GetConfig().AvgSpeedFoot = configuration.AvgSpeedFoot;
            configChanged = true;
        }
        if (AdminManager.GetConfig().MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            //AdminManager.GetConfig().MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }
        if (AdminManager.GetConfig().RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            //AdminManager.GetConfig().RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (AdminManager.GetConfig().MaxTimeInactivity != configuration.MaxTimeInactivity)
        {
            s_dal.Config.MaxTimeInactivity = configuration.MaxTimeInactivity;
            //AdminManager.GetConfig().MaxTimeInactivity = configuration.MaxTimeInactivity;
            configChanged = true;
        }
        if (AdminManager.GetConfig().GoogleApiKey != configuration.GoogleApiKey)
        {
            s_dal.Config.GoogleApiKey = configuration.GoogleApiKey;
            //AdminManager.GetConfig().GoogleApiKey = configuration.GoogleApiKey;
            configChanged = true;
        }
        if (AdminManager.GetConfig().EmailAddress != configuration.EmailAddress)
        {
            s_dal.Config.EmailAddress = configuration.EmailAddress ?? string.Empty;
            //AdminManager.GetConfig().EmailAddress = configuration.EmailAddress ?? string.Empty;
            configChanged = true;
        }
        if (AdminManager.GetConfig().ScriptUrl != configuration.ScriptUrl)
        {
            s_dal.Config.ScriptUrl = configuration.ScriptUrl;
            //AdminManager.GetConfig().ScriptUrl = configuration.ScriptUrl;
            configChanged = true;
        }
        if (AdminManager.GetConfig().ScriptPass != configuration.ScriptPass)
        {
            s_dal.Config.ScriptPass = configuration.ScriptPass;
            //AdminManager.GetConfig().ScriptPass = configuration.ScriptPass;
            configChanged = true;
        }
        if (AdminManager.GetConfig().TokenCallSms != configuration.TokenCallSms)
        {
            s_dal.Config.TokenCallSms = configuration.TokenCallSms;
            //AdminManager.GetConfig().TokenCallSms = configuration.TokenCallSms;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static async Task ResetDB() //stage 4-7
    {
        await Task.Run(() =>
        {
            lock (BlMutex) //stage 7
            {
                s_dal.ResetDB(); //stage 4
            }
        });
        AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
        ConfigUpdatedObservers?.Invoke(); //stage 5 - needed to update PL 
    }

    internal static async Task InitializeDB() //stage 4-7
    {
        await Task.Run(() =>
        {
            lock (BlMutex) //stage 7
            {
                DalTest.Initialization.Do(); //stage 4
            }
        });
        AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
        ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL
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
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("לא ניתן לעדכן נתונים כאשר הסימולטור פעיל");
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

    private static void clockRunner()  //**********************************************************************************************************stage 7
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation


           if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
            {
                _simulateTask = Task.Run(() => CourierManager.CourierSimulation());
            }

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    /// <summary>
    /// Performs periodic checks on system entities (Couriers, Orders) triggered by clock updates.
    /// Checks for courier inactivity and updates their status if necessary.
    /// </summary>
    /// <returns>True if any changes were made to the database/lists, requiring a UI refresh.</returns>
    public static void PeriodicSystemUpdates(DateTime oldClock, DateTime newClock)
    {
        if (oldClock + TimeSpan.FromDays(7) >= newClock)
            return;

        if (s_periodicMutex.CheckAndSetInProgress())
            return;
        try
        {

            TimeSpan maxInactivity = AdminManager.GetConfig().MaxTimeInactivity;
            bool anyListChange = false;

            IEnumerable<DO.Courier> couriers;
            ILookup<int, DO.Delivery> deliveriesByCourier;


            lock (AdminManager.BlMutex)
            {
                couriers = s_dal.Courier.ReadAll(c => c.Active);
                deliveriesByCourier = s_dal.Delivery.ReadAll()
                                                    .ToLookup(d => d.CourierId);
            }

            foreach (var courier in couriers)
            {
                var courierDeliveries = deliveriesByCourier[courier.Id];

                bool isCurrentlyDelivering = courierDeliveries.Any(d => d.EndDelivery == null);
                if (isCurrentlyDelivering)
                    continue;

                DateTime lastActivityTime;

                var lastCompletedDelivery = courierDeliveries
                                            .Where(d => d.EndDelivery != null)
                                            .OrderByDescending(d => d.TimeEndDelivery)
                                            .FirstOrDefault();

                if (lastCompletedDelivery != null && lastCompletedDelivery.TimeEndDelivery.HasValue)
                {
                    lastActivityTime = lastCompletedDelivery.TimeEndDelivery.Value;
                }
                else
                {
                    // מקרה קצה: שליח שמעולם לא ביצע משלוח
                    lastActivityTime = courier.WorkingSince;
                }

                // חישוב הזמן שעבר
                TimeSpan timeSinceActivity = newClock - lastActivityTime;

                if (timeSinceActivity > maxInactivity)
                {
                    var updatedCourier = courier with { Active = false };

                    lock (AdminManager.BlMutex)
                        s_dal.Courier.Update(updatedCourier);

                    // עדכון משקיפים
                    anyListChange = true;
                    CourierManager.Observer.NotifyItemUpdated(updatedCourier.Id);
                }
            }

            if (anyListChange)
            {
                CourierManager.Observer.NotifyListUpdated();
            }
        }
        finally
        {

            s_periodicMutex.UnsetInProgress();
        }
    }

    #endregion Stage 7 base
}
