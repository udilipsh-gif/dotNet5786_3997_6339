namespace BlApi;

/// <summary>
/// Defines administrative operations available in the business logic layer.
/// </summary>
/// <remarks>
/// Administrative operations include initializing/resetting the system state, manipulating the business clock,
/// and reading/updating system configuration.
/// </remarks>
public interface IAdmin 
{
    /// <summary>
    /// Resets the database and system state to defaults.
    /// </summary>
    void ResetDB();

    /// <summary>
    /// Initializes the database with initial/sample data.
    /// </summary>
    /// <remarks>
    /// Implementations typically call <see cref="ResetDB"/> before initializing.
    /// </remarks>
    void InitializeDB();

    /// <summary>
    /// Gets the current business clock time.
    /// </summary>
    /// <returns>The current clock value used by the business logic layer.</returns>
    DateTime GetClock();

    /// <summary>
    /// Advances the business clock by one unit.
    /// </summary>
    /// <param name="unit">The unit to advance by (minute, hour, day, week, month, or year).</param>
    void ForwardClock(BO.TimeUnit unit, int num = 1);

    /// <summary>
    /// Retrieves the current system configuration.
    /// </summary>
    /// <returns>The current configuration.</returns>
    BO.Config GetConfig();

    /// <summary>
    /// Updates the system configuration.
    /// </summary>
    /// <param name="config">The configuration to set.</param>
    void SetConfig(BO.Config config);

    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
}
