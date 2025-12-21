namespace BO;
public enum CourierFieldSort
{
    Name,
    Id,
    Phone,
    TypeShipment,
    AvailableDeliveries
}
public enum CourierFieldFilter
{
    ById,
    ByName,
    ByAvailable,
    ByActiveStatus
}
public enum OrderInListField
{
    OrderId,
    TypeOfOrder,
    DistanceKm,
    OrderStatus,
    ScheduleStatus,
    TimeLeftForDelivery,
    TotalTimeOfDelivery,
    DeliveryAttempts
}

public enum OpenOrderInListField
{
    CourierId,
    OrderId,
    TypeOfOrder,
    Weight,
    Address,
    DistanceKm,
    ActualDistance,
    EstimatedDeliveryTime,
    ScheduleStatus,
    TimeLeftForDelivery,
    MaxDeliveryTime
}

public enum ClosedDeliveryInListField
{
    DeliveryId,
    OrderId,
    TypeOfOrder,
    Address,
    ShipmentType,
    AqualDistens,
    DelyveryTime,
    EndDelivery
}

public enum TimeUnit
{
    /// <summary>
    /// Time unit in minutes.
    /// </summary>
    MINUTE,
    /// <summary>
    /// Time unit in hours.
    /// </summary>
    HOUR,
    /// <summary>
    /// Time unit in days.
    /// </summary>
    DAY,
    /// <summary>
    ///     Time unit in weeks.
    /// </summary>
    WEEK,
    /// <summary>
    ///   Time unit in months.  
    /// </summary>
    MONTH,
    /// <summary>
    /// Time unit in years.
    /// </summary>
    YEAR

}
public enum TheTypeShipment
{
    /// <summary>
    /// Delivery by car.
    /// </summary>
    CAR,

    /// <summary>
    /// Delivery by motorcycle.
    /// </summary>
    MOTORCYCLE,

    /// <summary>
    /// Delivery by bike.
    /// </summary>
    BIKE,

    /// <summary>
    /// Delivery on foot.
    /// </summary>
    FOOT
}

/// <summary>
/// Represents the type of order based on delivery speed and requirements.
/// </summary>
public enum TypeOfOrder
{
    /// <summary>
    /// Standard delivery - all shipment types are available.
    /// </summary>
    STANDART,

    /// <summary>
    /// Fast delivery - only motorcycle or car available.
    /// </summary>
    FAST_DELIVERY,

    /// <summary>
    /// Immediate delivery - only motorcycle available.
    /// </summary>
    DELIVER_IMMEDIATELY
}

/// <summary>
/// Represents the current status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order is open and waiting to be processed.
    /// </summary>
    OPEN,

    /// <summary>
    /// The order is currently being delivered.
    /// </summary>
    DELIVERING,

    /// <summary>
    /// The order has been completed successfully.
    /// </summary>
    COMPLETED,

    /// <summary>
    /// The order was refused.
    /// </summary>
    REFUSED,

    /// <summary>
    /// The order was cancelled.
    /// </summary>
    CANCELLED
}

/// <summary>
/// Represents the schedule status of a delivery relative to its expected timeline.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>
    /// The delivery is on time.
    /// </summary>
    ONTYME,

    /// <summary>
    /// The delivery is at risk of being late.
    /// </summary>
    INRISK,

    /// <summary>
    /// The delivery is late.
    /// </summary>
    LATE,

    /// <summary>
    /// The order is canceled.
    /// </summary>
    CANCELLED
}
public enum EndDelivery
{
    /// <summary>
    /// The delivery was successfully completed.
    /// </summary>
    DELIVERED,

    /// <summary>
    /// The delivery was refused by the recipient.
    /// </summary>
    REFUSED,

    /// <summary>
    /// The delivery was cancelled.
    /// </summary>
    CANCELLED,

    /// <summary>
    /// The delivery address or recipient was not found.
    /// </summary>
    NOTFOUND,

    /// <summary>
    /// The delivery failed for other reasons.
    /// </summary>
    FAILED
}
