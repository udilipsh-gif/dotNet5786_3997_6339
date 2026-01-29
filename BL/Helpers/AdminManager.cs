using DO;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Helpers;

internal static class AdminManager
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get;
    private static readonly AsyncMutex s_periodicMutex = new();

    internal static DateTime Now { get => s_dal.Config.Clock; }

    internal static event Action? ConfigUpdatedObservers;
    internal static event Action? ClockUpdatedObservers;

    private static Task? _periodicTask = null;

    internal static void UpdateClock(DateTime newClock)
    {
        var oldClock = s_dal.Config.Clock;
        s_dal.Config.Clock = newClock;

        // בדיקה שהמשימה הקודמת הסתיימה לפני שמתחילים חדשה למניעת עומס
        if (_periodicTask is null || _periodicTask.IsCompleted)
            _periodicTask = Task.Run(() => PeriodicSystemUpdates(oldClock, newClock));

        ClockUpdatedObservers?.Invoke();
    }

    // ... (GetConfig ו-SetConfig נשארים ללא שינוי - הם תקינים) ...
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static BO.Config GetConfig()
    {
        return new BO.Config()
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
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void SetConfig(BO.Config configuration)
    {
        bool configChanged = false; // stage 5

        var config = AdminManager.GetConfig();

        if (config.ManagerId != configuration.ManagerId)
        {
            if (!Tools.IsValidId(configuration.ManagerId))
                throw new BO.BlInvalidValueException("Invalid Manager ID.");
            s_dal.Config.ManagerId = configuration.ManagerId;
            configChanged = true;
        }
        if (config.PasswordManager != configuration.PasswordManager)
        {
            if (!Tools.IsStrongPassword(configuration.PasswordManager))
                throw new BO.BlInvalidValueException("Weak password for Manager.");
            s_dal.Config.PasswordManager = configuration.PasswordManager;
            //AdminManager.GetConfig().PasswordManager = configuration.PasswordManager;
            configChanged = true;
        }

        if (config.StoreAddress != configuration.StoreAddress)
        {
            
            string address = configuration.StoreAddress ?? throw new BO.BlInvalidValueException("Store address cannot be null.");

            string api = configuration.GoogleApiKey ?? throw new BO.BlInvalidValueException("Google API key cannot be null.");
            (double Lat, double Lon)? adressCoordinates = Task.Run(() => GoogleMapsService.GetGeocodingAsync(address, api)).Result ??
                throw new BO.BlInvalidValueException("Geocoding failed.");
            configuration.Latitude = adressCoordinates?.Lat;
            configuration.Longitude = adressCoordinates?.Lon;

            s_dal.Config.StoreAddress = configuration.StoreAddress;

            s_dal.Config.Latitude = adressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address.");

            s_dal.Config.Longitude = adressCoordinates?.Lon ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            configChanged = true;
            Task.Run(OrderManager.UpdateDistanceForOrders);
        }

        if (config.MaxDeliveryRange != configuration.MaxDeliveryRange)
        {
            s_dal.Config.MaxDeliveryRange = configuration.MaxDeliveryRange;
            //  AdminManager.GetConfig().MaxDeliveryRange = configuration.MaxDeliveryRange;
            configChanged = true;
        }
        if (config.AvgSpeedCar != configuration.AvgSpeedCar)
        {
            s_dal.Config.AvgSpeedCar = configuration.AvgSpeedCar;
            //AdminManager.GetConfig().AvgSpeedCar = configuration.AvgSpeedCar;
            configChanged = true;
        }
        if (config.AvgSpeedMotorcycle != configuration.AvgSpeedMotorcycle)
        {
            s_dal.Config.AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            //AdminManager.GetConfig().AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            configChanged = true;
        }
        if (config.AvgSpeedBike != configuration.AvgSpeedBike)
        {
            s_dal.Config.AvgSpeedBike = configuration.AvgSpeedBike;
            //AdminManager.GetConfig().AvgSpeedBike = configuration.AvgSpeedBike;
            configChanged = true;
        }
        if (config.AvgSpeedFoot != configuration.AvgSpeedFoot)
        {
            s_dal.Config.AvgSpeedFoot = configuration.AvgSpeedFoot;
            //AdminManager.GetConfig().AvgSpeedFoot = configuration.AvgSpeedFoot;
            configChanged = true;
        }
        if (config.MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            //AdminManager.GetConfig().MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }
        if (config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            //AdminManager.GetConfig().RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (config.MaxTimeInactivity != configuration.MaxTimeInactivity)
        {
            s_dal.Config.MaxTimeInactivity = configuration.MaxTimeInactivity;
            //AdminManager.GetConfig().MaxTimeInactivity = configuration.MaxTimeInactivity;
            configChanged = true;
        }
        if (config.GoogleApiKey != configuration.GoogleApiKey)
        {
            s_dal.Config.GoogleApiKey = configuration.GoogleApiKey;
            //AdminManager.GetConfig().GoogleApiKey = configuration.GoogleApiKey;
            configChanged = true;
        }
        if (config.EmailAddress != configuration.EmailAddress)
        {
            s_dal.Config.EmailAddress = configuration.EmailAddress ?? string.Empty;
            //AdminManager.GetConfig().EmailAddress = configuration.EmailAddress ?? string.Empty;
            configChanged = true;
        }
        if (config.ScriptUrl != configuration.ScriptUrl)
        {
            s_dal.Config.ScriptUrl = configuration.ScriptUrl;
            //AdminManager.GetConfig().ScriptUrl = configuration.ScriptUrl;
            configChanged = true;
        }
        if (config.ScriptPass != configuration.ScriptPass)
        {
            s_dal.Config.ScriptPass = configuration.ScriptPass;
            //AdminManager.GetConfig().ScriptPass = configuration.ScriptPass;
            configChanged = true;
        }
        if (config.TokenCallSms != configuration.TokenCallSms)
        {
            s_dal.Config.TokenCallSms = configuration.TokenCallSms;
            //AdminManager.GetConfig().TokenCallSms = configuration.TokenCallSms;
            configChanged = true;
        }

        if (configChanged)
            ConfigUpdatedObservers?.Invoke();
    }

    internal static async Task ResetDB()
    {
        // 1. איפוס ה-Cache ב-OrderManager (חובה!)
        OrderManager.ResetCache();

        await Task.Run(() =>
        {
            lock (BlMutex)
            {
                s_dal.ResetDB();
            }
        });

        // עדכון שעון ותצוגה
        AdminManager.UpdateClock(AdminManager.Now);
        ConfigUpdatedObservers?.Invoke();

        // עדכון רשימות כדי שהמסכים יתנקו
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    internal static async Task InitializeDB()
    {
        OrderManager.ResetCache();

        await Task.Run(() =>
        {
            lock (BlMutex)
            {
                DalTest.Initialization.Do();
            }
        });

        AdminManager.UpdateClock(AdminManager.Now);
        ConfigUpdatedObservers?.Invoke();

        // עדכון רשימות כדי שהמסכים יתמלאו בנתונים החדשים
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    #endregion Stage 4-7

    #region Stage 7 base

    internal static readonly object BlMutex = new();
    private static volatile Thread? s_thread;
    private static int s_interval = 1;
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("לא ניתן לעדכן נתונים כאשר הסימולטור פעיל");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt();
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

            // כאן מתבצעת הסימולציה של השליחים
            if (_simulateTask is null || _simulateTask.IsCompleted)
            {
                _simulateTask = Task.Run(() => CourierManager.CourierSimulation());
            }

            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException) { }
        }
    }

    public static void PeriodicSystemUpdates(DateTime oldClock, DateTime newClock)
    {
        if (newClock <= oldClock)
            return;

        var config = AdminManager.GetConfig();

        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        bool anyListChange = false;

        bool ordersChanged = false;

        try
        {
            TimeSpan maxInactivity = config.MaxTimeInactivity;
            IEnumerable<DO.Courier> couriers;
            ILookup<int, DO.Delivery> deliveriesByCourier;

            lock (AdminManager.BlMutex)
                couriers = s_dal.Courier.ReadAll(c => c.Active).ToList();
            lock (AdminManager.BlMutex)
                deliveriesByCourier = s_dal.Delivery.ReadAll().ToLookup(d => d.CourierId);

            foreach (var courier in couriers)
            {
                var courierDeliveries = deliveriesByCourier[courier.Id];
                if (courierDeliveries.Any(d => d.EndDelivery == null)) continue;

                DateTime lastActivityTime;
                var lastCompleted = courierDeliveries.Where(d => d.EndDelivery != null).MaxBy(d => d.TimeEndDelivery);

                lastActivityTime = lastCompleted?.TimeEndDelivery ?? courier.WorkingSince;

                if (newClock - lastActivityTime > maxInactivity)
                {
                    var updated = courier with { Active = false };
                    lock (AdminManager.BlMutex) s_dal.Courier.Update(updated);
                    anyListChange = true;
                    CourierManager.Observer.NotifyItemUpdated(updated.Id);
                }
            }

            ordersChanged = OrderManager.CheckStatusChanges(oldClock, newClock);

        }
        finally
        {
            if (anyListChange)
                CourierManager.Observer.NotifyListUpdated();

            if (ordersChanged)
            {
                OrderManager.Observer.NotifyListUpdated();
            }
            
            s_periodicMutex.UnsetInProgress();
        }
    }

    public static bool IsSimulatorStop()
        => s_thread == null;

    #endregion Stage 7 base
}