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
    PENDING,
    INPROGRESS,
    DELIVERED
}
