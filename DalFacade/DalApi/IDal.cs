namespace DalApi;
using DO;
/// <summary>
/// Main Data Access Layer interface that aggregates all entity-specific DAL interfaces.
/// Provides a unified entry point for accessing all data operations in the system.
/// </summary>
public interface IDal
{
    /// <summary>
    /// Gets the data access interface for courier operations.
    /// Provides CRUD operations and queries for courier entities.
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Gets the data access interface for order operations.
    /// Provides CRUD operations and queries for order entities.
    /// </summary>
    IOrder Order { get; }

    /// <summary>
    /// Gets the data access interface for delivery operations.
    /// Provides CRUD operations and queries for delivery entities.
    /// </summary>
    IDelivery Delivery { get; }

    /// <summary>
    /// Gets the data access interface for system configuration operations.
    /// Provides access to system settings, clock, and operational parameters.
    /// </summary>
    IConfig Config { get; }

    /// <summary>
    /// Resets the entire database to its initial state.
    /// Clears all data including couriers, orders, deliveries, and resets configuration settings.
    /// </summary>
    /// <remarks>
    /// Warning: This operation is destructive and cannot be undone.
    /// All existing data will be permanently deleted.
    /// </remarks>
    void ResetDB();
}