namespace DO;

/// <summary>
/// Represents the type of shipment method used for delivery.
/// </summary>
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
/// Represents the final status of a delivery attempt.
/// </summary>
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
    CONCELLED,
    
    /// <summary>
    /// The delivery address or recipient was not found.
    /// </summary>
    NOTFOUND,
    
    /// <summary>
    /// The delivery failed for other reasons.
    /// </summary>
    FAILED
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
    CONCELLED
}
