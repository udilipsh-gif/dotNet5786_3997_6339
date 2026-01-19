namespace BlApi;

/// <summary>
/// Defines the main business logic facade.
/// </summary>
/// <remarks>
/// This interface exposes the available BL services (admin, courier, order) through a single entry point.
/// </remarks>
public interface IBl 
{
    /// <summary>
    /// Gets the administrative service.
    /// </summary>
    IAdmin Admin { get; }

    /// <summary>
    /// Gets the courier management service.
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Gets the order management service.
    /// </summary>
    IOrder Order { get; }

    /// <summary>
    /// Gets the order management service.
    /// </summary>
    IDelivery Delivery { get; }
}
