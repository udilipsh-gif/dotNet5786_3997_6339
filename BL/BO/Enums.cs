using System.ComponentModel;

namespace BO;

/// <summary>
/// Defines the fields available for sorting courier list results.
/// </summary>
public enum CourierFieldSort
{
    /// <summary>Sort by courier name.</summary>
    Name,

    /// <summary>Sort by courier ID.</summary>
    Id,

    /// <summary>Sort by courier phone number.</summary>
    Phone,

    /// <summary>Sort by the courier shipment/vehicle type.</summary>
    TypeShipment,

    /// <summary>Sort by the number of deliveries currently available for the courier.</summary>
    AvailableDeliveries
}

/// <summary>
/// Defines the fields available for filtering courier list results.
/// </summary>
public enum CourierFieldFilter
{
    [Description("הכול")]
    All,

    [Description("פעילים בלבד")]
    IsActive,

    [Description("לא פעילים בלבד")]
    InActive,
}

/// <summary>
/// Defines the fields available for filtering and sorting the orders list (summary view).
/// </summary>
public enum OrderInListField
{
    /// <summary>Order identifier.</summary>
    OrderId,

    /// <summary>Order type (standard/fast/immediate).</summary>
    TypeOfOrder,

    /// <summary>Distance from store to customer (km).</summary>
    DistanceKm,

    /// <summary>Order current status.</summary>
    OrderStatus,

    /// <summary>Schedule/on-time status.</summary>
    ScheduleStatus,

    /// <summary>Time remaining until delivery deadline.</summary>
    TimeLeftForDelivery,

    /// <summary>Total allocated time window for delivery.</summary>
    TotalTimeOfDelivery,

    /// <summary>Number of delivery attempts.</summary>
    DeliveryAttempts
}

/// <summary>
/// Defines the fields available for filtering and sorting open orders list results.
/// </summary>
public enum OpenOrderInListField
{
    /// <summary>Courier identifier (if applicable to the view/model).</summary>
    CourierId,

    /// <summary>Order identifier.</summary>
    OrderId,

    /// <summary>Order type (standard/fast/immediate).</summary>
    TypeOfOrder,

    /// <summary>Order weight.</summary>
    Weight,

    /// <summary>Delivery address.</summary>
    Address,

    /// <summary>Distance from store to customer (km).</summary>
    DistanceKm,

    /// <summary>Actual travel distance (km), if available.</summary>
    ActualDistance,

    /// <summary>Estimated delivery duration/time, if available.</summary>
    EstimatedDeliveryTime,

    /// <summary>Schedule/on-time status.</summary>
    ScheduleStatus,

    /// <summary>Time remaining until delivery deadline.</summary>
    TimeLeftForDelivery,

    /// <summary>Maximum delivery deadline timestamp.</summary>
    MaxDeliveryTime
}

/// <summary>
/// Defines the fields available for filtering and sorting closed deliveries list results.
/// </summary>
public enum ClosedDeliveryInListField
{
    /// <summary>Delivery identifier.</summary>
    DeliveryId,

    /// <summary>Order identifier.</summary>
    OrderId,

    /// <summary>Order type (standard/fast/immediate).</summary>
    TypeOfOrder,

    /// <summary>Delivery address.</summary>
    Address,

    /// <summary>Shipment type used.</summary>
    ShipmentType,

    /// <summary>Actual travel distance (km).</summary>
    ActualDistance,

    /// <summary>Delivery duration/time.</summary>
    DeliveryTime,

    /// <summary>Delivery end status/outcome.</summary>
    EndDelivery
}

/// <summary>
/// Represents the units supported for forwarding the system clock.
/// </summary>
public enum TimeUnit
{
    /// <summary>Time unit in minutes.</summary>
    MINUTE,

    /// <summary>Time unit in hours.</summary>
    HOUR,

    /// <summary>Time unit in days.</summary>
    DAY,

    /// <summary>Time unit in weeks.</summary>
    WEEK,

    /// <summary>Time unit in months.</summary>
    MONTH,

    /// <summary>Time unit in years.</summary>
    YEAR
}

/// <summary>
/// Represents the shipment method/vehicle type used by a courier.
/// </summary>
public enum TheTypeShipment
{
    /// <summary>Delivery by car.</summary>
    [Description("רכב")]
    CAR,

    /// <summary>Delivery by motorcycle.</summary>
    [Description("קטנוע")]
    MOTORCYCLE,

    /// <summary>Delivery by bike.</summary>
    [Description("אופניים")]
    BIKE,

    /// <summary>Delivery on foot.</summary>
    [Description("ברגל")]
    FOOT
}

/// <summary>
/// Represents the type of order based on delivery speed and requirements.
/// </summary>
public enum TypeOfOrder
{
    /// <summary>Standard delivery - all shipment types are available.</summary>
    [Description("רגיל")]
    STANDART,

    /// <summary>Fast delivery - only motorcycle or car available.</summary>
    [Description("מהיר")]
    FAST_DELIVERY,

    /// <summary>Immediate delivery - only motorcycle available.</summary>
    [Description("מיידי")]
    DELIVER_IMMEDIATELY,

}

/// <summary>
/// Represents the current status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>The order is open and waiting to be processed.</summary>
    [Description("פתוח")]
    OPEN,

    /// <summary>The order is currently being delivered.</summary>
    [Description("במשלוח")]
    DELIVERING,

    /// <summary>The order has been completed successfully.</summary>
    [Description("נמסר")]
    COMPLETED,

    /// <summary>The order was refused.</summary>
    [Description("סורב על ידי הלקוח")]
    REFUSED,

    /// <summary>The order was cancelled.</summary>
    [Description("בוטל")]
    CANCELLED
}

/// <summary>
/// Represents the schedule status of a delivery relative to its expected timeline.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>The delivery is on time.</summary>
    [Description("בזמן")]
    ONTYME,

    /// <summary>The delivery is at risk of being late.</summary>
    [Description("בסיכון")]
    INRISK,

    /// <summary>The delivery is late.</summary>
    [Description("באיחור")]
    LATE,

    /// <summary>The order is canceled.</summary>
    [Description("בוטל")]
    CANCELLED,

}

/// <summary>
/// Represents the final outcome status of a delivery attempt.
/// </summary>
public enum EndDelivery
{
    /// <summary>
    /// The delivery was successfully completed.
    /// </summary>
    [Description("נמסר בהצלחה")]
    DELIVERED,

    /// <summary>
    /// The delivery was refused by the recipient.
    /// </summary>
    [Description("סורב על ידי הלקוח")]
    REFUSED,

    /// <summary>
    /// The delivery was cancelled.
    /// </summary>
    [Description("בוטל")]
    CANCELLED,

    /// <summary>
    /// The delivery address or recipient was not found.
    /// </summary>
    [Description("כתובת/לקוח לא נמצא")]
    NOTFOUND,

    /// <summary>
    /// The delivery failed for other reasons.
    /// </summary>
    [Description("נכשל")]
    FAILED
}
