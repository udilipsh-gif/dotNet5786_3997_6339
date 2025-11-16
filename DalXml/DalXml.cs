using DalApi;
using System.Diagnostics;

namespace Dal;

/// <summary>
/// XML-based implementation of the Data Access Layer (DAL).
/// Provides access to all data operations using XML file storage.
/// </summary>
/// <remarks>
/// This class implements the Singleton pattern (sealed) and provides access to
/// courier, order, delivery, and configuration data stored in XML files.
/// All data operations are performed through XML serialization/deserialization.
/// </remarks>
sealed internal class DalXml : IDal
{
    public static IDal Instance { get; } = new DalXml();
    private DalXml() { }
    /// <summary>
    /// Gets the data access interface for courier operations.
    /// </summary>
    /// <value>
    /// A new instance of CourierImplementation for each access.
    /// </value>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the data access interface for order operations.
    /// </summary>
    /// <value>
    /// A new instance of OrderImplementation for each access.
    /// </value>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Gets the data access interface for delivery operations.
    /// </summary>
    /// <value>
    /// A new instance of DeliveryImplementation for each access.
    /// </value>
    public IDelivery Delivery { get; } = new DeliveryImplementation();

    /// <summary>
    /// Gets the data access interface for system configuration operations.
    /// </summary>
    /// <value>
    /// A new instance of ConfigImplementation for each access.
    /// </value>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the entire database to its initial state.
    /// Deletes all deliveries, couriers, and orders, then resets configuration settings.
    /// </summary>
    /// <remarks>
    /// Warning: This operation is destructive and cannot be undone.
    /// All existing data in XML files will be permanently deleted.
    /// The deletion order is important: Deliveries first, then Couriers, then Orders,
    /// to maintain referential integrity during the reset process.
    /// </remarks>
    public void ResetDB()
    {
        Delivery.DeleteAll();
        Courier.DeleteAll();
        Order.DeleteAll();
        Config.Reset();
    }
}
