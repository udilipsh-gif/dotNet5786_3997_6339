using DO;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Manages administrative tasks, global configuration, system clock simulation, and periodic maintenance.
/// </summary>
/// <remarks>
/// This static class serves as the central control hub for the application. It handles:
/// <list type="bullet">
///   <item><description>System Clock: Simulates time progression and notifies observers.</description></item>
///   <item><description>Configuration: Manages global settings (Store address, speeds, passwords).</description></item>
///   <item><description>Periodic Tasks: Automatically checks for courier inactivity and order status changes.</description></item>
///   <item><description>Database Reset: Provides functionality to clear or initialize the database.</description></item>
/// </list>
/// </remarks>
internal static class AdminManager
{
    #region Services & Fields

    /// <summary>
    /// Data access layer instance for database operations.
    /// </summary>
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Mutex to prevent overlapping executions of periodic system updates.
    /// </summary>
    private static readonly AsyncMutex s_periodicMutex = new();

    /// <summary>
    /// Gets the current simulated system time.
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; }

    /// <summary>
    /// Event triggered when the system configuration is updated.
    /// </summary>
    internal static event Action? ConfigUpdatedObservers;

    /// <summary>
    /// Event triggered when the simulated system clock is updated.
    /// </summary>
    internal static event Action? ClockUpdatedObservers;

    /// <summary>
    /// Task handle for the background periodic update process.
    /// </summary>
    private static Task? _periodicTask = null;

    #endregion

    #region Clock & Time Management

    /// <summary>
    /// Updates the system clock to a new time and triggers related events.
    /// </summary>
    /// <param name="newClock">The new date and time to set.</param>
    /// <remarks>
    /// This method updates the clock in the DAL, notifies observers, and initiates
    /// a background task to perform periodic system updates (like checking courier inactivity).
    /// </remarks>
    internal static void UpdateClock(DateTime newClock)
    {
        var oldClock = s_dal.Config.Clock;
        s_dal.Config.Clock = newClock;

        // Ensure previous task is completed before starting a new one to prevent overload
        if (_periodicTask is null || _periodicTask.IsCompleted)
            _periodicTask = Task.Run(() => PeriodicSystemUpdates(oldClock, newClock));

        ClockUpdatedObservers?.Invoke();
    }

    #endregion

    #region Configuration Management

    /// <summary>
    /// Retrieves the current system configuration.
    /// </summary>
    /// <returns>A <see cref="BO.Config"/> object containing all system settings.</returns>
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

    /// <summary>
    /// Updates the system configuration with new values.
    /// </summary>
    /// <param name="configuration">The new configuration object.</param>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when validation fails (e.g., weak password, invalid ID, geocoding failure).
    /// </exception>
    /// <remarks>
    /// <para>Performs validation on critical fields (ID, Password).</para>
    /// <para>If the Store Address changes, it automatically triggers Google Maps Geocoding
    /// to update coordinates and recalculates distances for all open orders.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void SetConfig(BO.Config configuration)
    {
        bool configChanged = false;

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
            configChanged = true;
        }

        // Handle Store Address Change & Geocoding
        if (config.StoreAddress != configuration.StoreAddress)
        {
            string address = configuration.StoreAddress ?? throw new BO.BlInvalidValueException("Store address cannot be null.");
            string api = configuration.GoogleApiKey ?? throw new BO.BlInvalidValueException("Google API key cannot be null.");

            // Synchronous call to async geocoding
            (double Lat, double Lon)? adressCoordinates = Task.Run(() => GoogleMapsService.GetGeocodingAsync(address, api)).Result ??
                throw new BO.BlInvalidValueException("Geocoding failed.");

            configuration.Latitude = adressCoordinates?.Lat;
            configuration.Longitude = adressCoordinates?.Lon;

            s_dal.Config.StoreAddress = configuration.StoreAddress;
            s_dal.Config.Latitude = adressCoordinates?.Lat ?? throw new BO.BlInvalidValueException("Failed to geocode address.");
            s_dal.Config.Longitude = adressCoordinates?.Lon ?? throw new BO.BlInvalidValueException("Failed to geocode address.");

            configChanged = true;

            // Recalculate distances for existing orders
            Task.Run(OrderManager.UpdateDistanceForOrders);
        }

        // Update simple properties
        if (config.MaxDeliveryRange != configuration.MaxDeliveryRange)
        {
            s_dal.Config.MaxDeliveryRange = configuration.MaxDeliveryRange;
            configChanged = true;
        }
        if (config.AvgSpeedCar != configuration.AvgSpeedCar)
        {
            s_dal.Config.AvgSpeedCar = configuration.AvgSpeedCar;
            configChanged = true;
        }
        if (config.AvgSpeedMotorcycle != configuration.AvgSpeedMotorcycle)
        {
            s_dal.Config.AvgSpeedMotorcycle = configuration.AvgSpeedMotorcycle;
            configChanged = true;
        }
        if (config.AvgSpeedBike != configuration.AvgSpeedBike)
        {
            s_dal.Config.AvgSpeedBike = configuration.AvgSpeedBike;
            configChanged = true;
        }
        if (config.AvgSpeedFoot != configuration.AvgSpeedFoot)
        {
            s_dal.Config.AvgSpeedFoot = configuration.AvgSpeedFoot;
            configChanged = true;
        }
        if (config.MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }
        if (config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (config.MaxTimeInactivity != configuration.MaxTimeInactivity)
        {
            s_dal.Config.MaxTimeInactivity = configuration.MaxTimeInactivity;
            configChanged = true;
        }
        if (config.GoogleApiKey != configuration.GoogleApiKey)
        {
            s_dal.Config.GoogleApiKey = configuration.GoogleApiKey;
            configChanged = true;
        }
        if (config.EmailAddress != configuration.EmailAddress)
        {
            s_dal.Config.EmailAddress = configuration.EmailAddress ?? string.Empty;
            configChanged = true;
        }
        if (config.ScriptUrl != configuration.ScriptUrl)
        {
            s_dal.Config.ScriptUrl = configuration.ScriptUrl;
            configChanged = true;
        }
        if (config.ScriptPass != configuration.ScriptPass)
        {
            s_dal.Config.ScriptPass = configuration.ScriptPass;
            configChanged = true;
        }
        if (config.TokenCallSms != configuration.TokenCallSms)
        {
            s_dal.Config.TokenCallSms = configuration.TokenCallSms;
            configChanged = true;
        }

        if (configChanged)
        {
            ConfigUpdatedObservers?.Invoke();
            OrderManager.ResetCache();
            OrderManager.Observer.NotifyListUpdated();
        }
    }

    #endregion

    #region Database Operations

    /// <summary>
    /// Resets the database to an empty state.
    /// </summary>
    /// <remarks>
    /// Clears all data, resets caches, updates the clock, and notifies all observers to refresh the UI.
    /// </remarks>
    internal static async Task ResetDB()
    {
        // 1. Reset Cache in OrderManager (Mandatory!)
        OrderManager.ResetCache();

        await Task.Run(() =>
        {
            lock (BlMutex)
            {
                s_dal.ResetDB();
            }
        });

        // Update clock and display
        AdminManager.UpdateClock(AdminManager.Now);
        ConfigUpdatedObservers?.Invoke();

        // Update lists to clear screens
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    /// <summary>
    /// Initializes the database with dummy test data.
    /// </summary>
    /// <remarks>
    /// Populates the DB, resets caches, and notifies observers to display the new data.
    /// </remarks>
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

        // Update lists so screens fill with new data
        OrderManager.Observer.NotifyListUpdated();
        CourierManager.Observer.NotifyListUpdated();
    }

    #endregion

    #region Simulation & Periodic Updates

    /// <summary>
    /// Global lock object for thread synchronization during simulation.
    /// </summary>
    internal static readonly object BlMutex = new();

    private static volatile Thread? s_thread;
    private static int s_interval = 1;
    private static volatile bool s_stop = false;

    /// <summary>
    /// Throws an exception if the simulator is currently running.
    /// </summary>
    /// <exception cref="BO.BLTemporaryNotAvailableException">Thrown if the simulator is active.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("לא ניתן לעדכן נתונים כאשר הסימולטור פעיל");
    }

    /// <summary>
    /// Starts the simulator clock thread.
    /// </summary>
    /// <param name="interval">The time interval (in minutes) to advance the clock per tick.</param>
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

    /// <summary>
    /// Stops the simulator clock thread.
    /// </summary>
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

    /// <summary>
    /// The main loop for the simulator thread.
    /// </summary>
    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            // Run courier simulation logic
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

    /// <summary>
    /// Performs periodic system checks triggered by clock updates.
    /// </summary>
    /// <param name="oldClock">The time before the update.</param>
    /// <param name="newClock">The time after the update.</param>
    /// <remarks>
    /// This method checks for:
    /// <list type="bullet">
    ///   <item><description>Courier Inactivity: Sets couriers to inactive if they haven't worked for a long time.</description></item>
    ///   <item><description>Order Status: Checks if orders have become "Late" or "At Risk".</description></item>
    /// </list>
    /// It uses a mutex to ensure only one update runs at a time.
    /// </remarks>
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
            // 1. Check Courier Inactivity
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
                // Skip if currently delivering
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

            // 2. Check Order Status (Late/Risk)
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

    /// <summary>
    /// Checks if the simulator is currently stopped.
    /// </summary>
    /// <returns>True if the simulator thread is null (stopped); otherwise, false.</returns>
    public static bool IsSimulatorStop()
        => s_thread == null;

    #endregion
}