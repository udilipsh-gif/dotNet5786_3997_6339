namespace BlImplementation;
using BlApi;

using Helpers;
using System.Threading.Tasks;

/// <summary>
/// Implements the <see cref="IAdmin"/> interface and provides administrative operations for the system.
/// </summary>
/// <remarks>
/// This class serves as the business-logic entry point for administrative actions such as
/// database initialization/reset, clock manipulation, and configuration management.
/// </remarks>
internal class AdminImplementation : IAdmin
{
    /// <summary>
    /// Resets the database and system state to defaults.
    /// </summary>
    /// <remarks>
    /// This operation typically deletes or clears existing data and restores default configuration.
    /// </remarks>
    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    /// <summary>
    /// Initializes the database with initial/sample data.
    /// </summary>
    /// <remarks>
    /// This method first calls <see cref="ResetDB"/> and then populates the system with initial data.
    /// </remarks>
    public void InitializeDB()
    {
        ResetDB();
        AdminManager.InitializeDB();
    }

    /// <summary>
    /// Retrieves the current system clock time.
    /// </summary>
    /// <returns>The current business-logic clock time.</returns>
    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    /// <summary>
    /// Advances the system clock by a specified number of time units.
    /// </summary>
    /// <param name="unit">The time unit to advance by (minute, hour, day, week, month, or year).</param>
    /// <param name="num">The number of units to advance. Defaults to 1 if not specified.</param>
    /// <exception cref="BO.BlInvalidValueException">Thrown when <paramref name="unit"/> is not a recognized value.</exception>
    /// <remarks>
    /// This method allows advancing the clock by multiple units at once (e.g., 5 minutes, 2 hours).
    /// The default value for <paramref name="num"/> is 1, maintaining backward compatibility with code that omits this parameter.
    /// </remarks>
    public void ForwardClock(BO.TimeUnit unit)
    {

        switch (unit)
        {
            case BO.TimeUnit.MINUTE:
                AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
                break;

            case BO.TimeUnit.HOUR:
                AdminManager.UpdateClock(AdminManager.Now.AddHours(1));
                break;

            case BO.TimeUnit.DAY:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(1));

                break;

            case BO.TimeUnit.MONTH:
                AdminManager.UpdateClock(AdminManager.Now.AddMonths(1));
                break;
            case BO.TimeUnit.WEEK:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(7));
                break;

            case BO.TimeUnit.YEAR:
                AdminManager.UpdateClock(AdminManager.Now.AddYears(1));
                break;

            default:
                throw new BO.BlInvalidValueException(nameof(unit));
        }


    }

    /// <summary>
    /// Retrieves the current system configuration.
    /// </summary>
    /// <returns>The current configuration object.</returns>
    public BO.Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    /// <summary>
    /// Updates the system configuration.
    /// </summary>
    /// <param name="config">The configuration to set.</param>
    public void SetConfig(BO.Config config)
    {
        AdminManager.SetConfig(config).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Registers an observer to be notified when the system clock is updated.
    /// </summary>
    /// <param name="clockObserver">The action to invoke when the clock changes.</param>
    /// <remarks>
    /// The observer will be called each time <see cref="ForwardClock(BO.TimeUnit, int)"/> is invoked
    /// or whenever the clock is programmatically updated.
    /// </remarks>
    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    /// <summary>
    /// Unregisters a previously registered clock observer.
    /// </summary>
    /// <param name="clockObserver">The action to remove from the observer list.</param>
    /// <remarks>
    /// It is important to remove observers when they are no longer needed to prevent memory leaks.
    /// </remarks>
    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    /// <summary>
    /// Registers an observer to be notified when the system configuration is updated.
    /// </summary>
    /// <param name="configObserver">The action to invoke when the configuration changes.</param>
    /// <remarks>
    /// The observer will be called each time <see cref="SetConfig(BO.Config)"/> is invoked.
    /// </remarks>
    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    /// <summary>
    /// Unregisters a previously registered configuration observer.
    /// </summary>
    /// <param name="configObserver">The action to remove from the observer list.</param>
    /// <remarks>
    /// It is important to remove observers when they are no longer needed to prevent memory leaks.
    /// </remarks>
    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;

    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }

    public void StopSimulator()
        => AdminManager.Stop(); //stage 7
}
