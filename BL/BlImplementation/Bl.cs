namespace BlImplementation;
using BlApi;

/// <summary>
/// Business logic facade that exposes the system's BL services.
/// </summary>
/// <remarks>
/// This class composes the concrete implementations of the BL contracts and provides a single
/// entry point for accessing administrative, courier, and order operations.
/// </remarks>
internal class Bl : IBl
{
    /// <summary>
    /// Gets the administrative service.
    /// </summary>
    public IAdmin Admin { get; } = new AdminImplementation();

    /// <summary>
    /// Gets the courier management service.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Gets the order management service.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();
}
