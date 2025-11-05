namespace DO;

public enum TheTypeShipment
{
    CAR,
    MOTORCYCLE,
    BIKE,
    FOOT
}
public enum TypeOfOrder
{
    STANDART,//all types
    FAST_DELIVERY,//only motorosycle / car
    DELIVER_IMMEDIATELY//only motorosycle
}
public enum EndDelivery
{
    DELIVERED,
    REFUSED,
    CONCELLED,
    NOTFOUND,
    FAILED
}

public enum OrderStatus
{
    OPEN,
    DELIVERING,
    COMPLETED,
    REFUSED,
    CONCELLED
}

public enum ScheduleStatus
{
    ONTYME,
    INRISK,
    LATE
}