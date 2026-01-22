namespace BlApi;

/// <summary>
/// Defines administrative operations available in the business logic layer.
/// </summary>
/// <remarks>
/// Administrative operations include initializing/resetting the system state, manipulating the business clock,
/// and reading/updating system configuration. This interface provides the contract for all administrative
/// functionality including:
/// <list type="bullet">
/// <item><description>Database initialization and reset operations</description></item>
/// <item><description>System clock manipulation for testing and simulation</description></item>
/// <item><description>Configuration management (read and update)</description></item>
/// <item><description>Observer pattern support for clock and configuration changes</description></item>
/// </list>
/// </remarks>
public interface IAdmin 
{
    /// <summary>
    /// Resets the database and system state to defaults.
    /// </summary>
    /// <remarks>
    /// This operation clears all data from the system including:
    /// <list type="bullet">
    /// <item><description>All couriers</description></item>
    /// <item><description>All orders</description></item>
    /// <item><description>All deliveries</description></item>
    /// <item><description>Configuration settings (reset to defaults)</description></item>
    /// <item><description>Auto-increment ID counters</description></item>
    /// </list>
    /// This is a destructive operation and should be used with caution, typically only in development or testing scenarios.
    /// </remarks>
    void ResetDB();

    /// <summary>
    /// Initializes the database with initial/sample data.
    /// </summary>
    /// <remarks>
    /// Implementations typically call <see cref="ResetDB"/> before initializing to ensure a clean state.
    /// This method populates the database with:
    /// <list type="bullet">
    /// <item><description>Sample couriers with various vehicle types and statuses</description></item>
    /// <item><description>Sample orders with different types and addresses</description></item>
    /// <item><description>Sample deliveries linking couriers to orders</description></item>
    /// <item><description>Default configuration values</description></item>
    /// </list>
    /// Useful for development, testing, and demonstration purposes.
    /// </remarks>
    void InitializeDB();

    /// <summary>
    /// Gets the current business clock time.
    /// </summary>
    /// <returns>The current clock value used by the business logic layer.</returns>
    /// <remarks>
    /// The business clock may differ from the actual system time, allowing for time simulation
    /// and testing scenarios. All time-dependent business logic should use this clock value
    /// rather than DateTime.Now to ensure consistent behavior in testing environments.
    /// </remarks>
    DateTime GetClock();

    /// <summary>
    /// Advances the business clock by one unit of the specified time type.
    /// </summary>
    /// <param name="unit">The time unit to advance by (minute, hour, day, week, month, or year).</param>
    /// <remarks>
    /// This method allows simulation of time passage for testing delivery deadlines, courier availability,
    /// and other time-dependent features. After advancing the clock:
    /// <list type="bullet">
    /// <item><description>All registered clock observers are notified of the change</description></item>
    /// <item><description>Time-dependent calculations (delivery status, risk assessment) will reflect the new time</description></item>
    /// <item><description>The UI and business logic will use the updated clock value</description></item>
    /// </list>
    /// </remarks>
    void ForwardClock(BO.TimeUnit unit);

    /// <summary>
    /// Retrieves the current system configuration.
    /// </summary>
    /// <returns>The current configuration containing all system settings.</returns>
    /// <remarks>
    /// The configuration includes:
    /// <list type="bullet">
    /// <item><description>Manager credentials (ID and password)</description></item>
    /// <item><description>Store location (address, latitude, longitude)</description></item>
    /// <item><description>Delivery settings (max range, time limits, risk thresholds)</description></item>
    /// <item><description>Vehicle speeds for delivery time calculations</description></item>
    /// <item><description>System clock value</description></item>
    /// <item><description>External API keys (e.g., Google API)</description></item>
    /// </list>
    /// </remarks>
    BO.Config GetConfig();

    /// <summary>
    /// Updates the system configuration with new values.
    /// </summary>
    /// <param name="config">The configuration object containing the updated settings.</param>
    /// <remarks>
    /// This method updates all modifiable configuration settings and notifies all registered
    /// configuration observers of the changes. Changes take effect immediately and affect:
    /// <list type="bullet">
    /// <item><description>Delivery time calculations (if speed or time limits changed)</description></item>
    /// <item><description>Order acceptance (if max delivery range changed)</description></item>
    /// <item><description>Risk assessment (if risk range changed)</description></item>
    /// <item><description>Authentication (if manager credentials changed)</description></item>
    /// </list>
    /// Note: Some values like auto-increment IDs cannot be modified through this method.
    /// </remarks>
    Task SetConfig(BO.Config config);

    /// <summary>
    /// Registers an observer to be notified when the system configuration changes.
    /// </summary>
    /// <param name="configObserver">The action to invoke when configuration changes occur.</param>
    /// <remarks>
    /// The observer will be called whenever <see cref="SetConfig"/> is invoked, allowing UI components
    /// and other interested parties to react to configuration changes in real-time.
    /// Typical use cases include:
    /// <list type="bullet">
    /// <item><description>Updating UI displays showing configuration values</description></item>
    /// <item><description>Recalculating delivery estimates based on new speed settings</description></item>
    /// <item><description>Refreshing distance-based filters when max range changes</description></item>
    /// </list>
    /// </remarks>
    void AddConfigObserver(Action configObserver);

    /// <summary>
    /// Unregisters a previously registered configuration observer.
    /// </summary>
    /// <param name="configObserver">The observer action to remove.</param>
    /// <remarks>
    /// This method should be called when the observer is no longer needed (e.g., when a window closes)
    /// to prevent memory leaks and unnecessary notifications. The observer will no longer receive
    /// configuration change notifications after being removed.
    /// </remarks>
    void RemoveConfigObserver(Action configObserver);

    /// <summary>
    /// Registers an observer to be notified when the system clock advances.
    /// </summary>
    /// <param name="clockObserver">The action to invoke when the clock changes.</param>
    /// <remarks>
    /// The observer will be called whenever <see cref="ForwardClock"/> is invoked, allowing UI components
    /// to update time displays and business logic to react to time changes. Common use cases include:
    /// <list type="bullet">
    /// <item><description>Updating clock displays in the UI</description></item>
    /// <item><description>Recalculating delivery deadlines and time remaining</description></item>
    /// <item><description>Updating schedule status (on-time, at-risk, late)</description></item>
    /// <item><description>Triggering time-based business rules</description></item>
    /// </list>
    /// </remarks>
    void AddClockObserver(Action clockObserver);

    /// <summary>
    /// Unregisters a previously registered clock observer.
    /// </summary>
    /// <param name="clockObserver">The observer action to remove.</param>
    /// <remarks>
    /// This method should be called when the observer is no longer needed (e.g., when a window closes)
    /// to prevent memory leaks and unnecessary notifications. The observer will no longer receive
    /// clock change notifications after being removed.
    /// </remarks>
    void RemoveClockObserver(Action clockObserver);

    void StartSimulator(int interval); //stage 7

    void StopSimulator(); //stage 7
}
