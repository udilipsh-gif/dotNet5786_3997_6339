namespace Dal;
using DalApi;
using System.Data.SqlTypes;

/// <summary>
/// Sealed singleton class implementing the IDal interface.
/// Provides a unified access point to all data access layer operations.
/// </summary>
sealed internal class DalList : IDal
{
    private static readonly Lazy<DalList> lazyInstance =
        new Lazy<DalList>(() => new DalList());
    public static IDal Instance { get; } = lazyInstance.Value;
    private DalList() { }

    /// <summary>
    /// Gets the data access interface for courier operations.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the data access interface for order operations.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the data access interface for delivery operations.
    /// </summary>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Gets the data access interface for system configuration operations.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the entire database to its initial state.
    /// Clears all deliveries, couriers, orders, and resets configuration settings.
    /// </summary>
    /// <remarks>
    /// Warning: This operation is destructive and cannot be undone.
    /// All existing data will be permanently deleted.
    /// </remarks>
    public void ResetDB()
    {
        Delivery.DeleteAll();
        Courier.DeleteAll();
        Order.DeleteAll();
        Config.Reset();
    }
}
