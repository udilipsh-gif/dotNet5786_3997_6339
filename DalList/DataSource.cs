namespace Dal;
using DO;

/// <summary>
/// Internal static class that serves as an in-memory data source for the application.
/// Contains collections for storing deliveries, orders, and couriers.
/// </summary>
internal static class DataSource
{
    /// <summary>
    /// Gets the collection of all deliveries in the system.
    /// </summary>
    internal static List<DO.Delivery> Deliveries { get; } = new();
    
    /// <summary>
    /// Gets the collection of all orders in the system.
    /// </summary>
    internal static List<DO.Order> Orders { get; } = new();
    
    /// <summary>
    /// Gets the collection of all couriers in the system.
    /// </summary>
    internal static List<DO.Courier> Couriers { get; } = new();
}
