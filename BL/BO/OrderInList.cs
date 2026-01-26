using Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BO;

/// <summary>
/// Represents an order item in a list view with summary information.
/// </summary>
/// <remarks>
/// This class provides a lightweight representation of an order used in list or summary views,
/// including delivery tracking, status information, and timing metrics. It is optimized for
/// displaying multiple orders efficiently without loading full order details.
/// </remarks>
public class OrderInList : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. If omitted, uses the caller member name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Gets the unique identifier of the delivery associated with this order.
    /// </summary>
    /// <value>The delivery ID if a delivery has been assigned, or null if no delivery has been created yet.</value>
    public int? DeliveryId { get; init; } = null;

    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    /// <value>A unique integer ID that is assigned once and cannot be changed.</value>
    public required int OrderId { get; init; }

    /// <summary>
    /// Gets the type of order based on delivery speed requirements.
    /// </summary>
    /// <value>A <see cref="TypeOfOrder"/> value indicating whether this is a standard, fast, or immediate delivery.</value>
    public required TypeOfOrder TypeOfOrder { get; init; }

    /// <summary>
    /// Gets the distance from the store to the delivery location.
    /// </summary>
    /// <value>The delivery distance in kilometers.</value>
    public required double DistanceKm { get; init; }

    /// <summary>
    /// Gets the current status of the order in the delivery process.
    /// </summary>
    /// <value>An <see cref="OrderStatus"/> value indicating whether the order is open, delivering, completed, refused, or cancelled.</value>
    public required OrderStatus OrderStatus { get; init; }

    /// <summary>
    /// Backing field for ScheduleStatus property.
    /// </summary>
    private ScheduleStatus _ScheduleStatus;

    /// <summary>
    /// Gets the schedule status indicating if the delivery is on time, at risk, or late.
    /// </summary>
    /// <value>A <see cref="ScheduleStatus"/> value representing the delivery timeline status relative to expected delivery time.</value>
    public ScheduleStatus ScheduleStatus
    {
        get => _ScheduleStatus;
        set
        {
            if (_ScheduleStatus != value)
            {
                _ScheduleStatus = value;
                OnPropertyChanged();
}
        }
    }

    /// <summary>
    /// Backing field for TimeRemaining property.
    /// </summary>
    private TimeSpan _TimeLeftForDelivery;


    /// <summary>
    /// Gets the time remaining until the maximum delivery deadline.
    /// </summary>
    /// <value>A TimeSpan representing how much time is left before the order becomes late.</value>
    public TimeSpan TimeLeftForDelivery
    {
        get => _TimeLeftForDelivery;
        set
        {
            if (_TimeLeftForDelivery != value)
            {
                _TimeLeftForDelivery = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the total time allocated for delivering this order.
    /// </summary>
    /// <value>A TimeSpan representing the complete duration from order placement to the maximum delivery deadline.</value>
    public required TimeSpan TotalTimeOfDelivery { get; init; }

    /// <summary>
    /// Gets the number of delivery attempts made for this order.
    /// </summary>
    /// <value>An integer count of how many times delivery has been attempted for this order.</value>
    /// <remarks>
    /// This counter increments with each delivery assignment, including failed or refused deliveries.
    /// Multiple attempts may indicate delivery challenges or customer unavailability.
    /// </remarks>
    public required int NumberOfDeliveryAttempts { get; init; }

    public override string ToString() => this.ToStringProperty();
}
