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
    BOXIT,//only car
    STANDART,//motorocycle or car
    FAST_DELIVERY,//only motorosycle
    DELIVER_IMMEDIATELY//only foot
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