namespace BlImplementation;
using BlApi;

using Helpers;

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
    /// Advances the system clock by one unit of time.
    /// </summary>
    /// <param name="unit">The time unit to advance by (minute, hour, day, week, month, or year).</param>
    /// <exception cref="BO.BlInvalidValueException">Thrown when <paramref name="unit"/> is not a recognized value.</exception>
    /// <remarks>
    /// Each call advances the clock by exactly one unit (e.g., 1 hour, 1 day).
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
        AdminManager.SetConfig(config);
    }
}
