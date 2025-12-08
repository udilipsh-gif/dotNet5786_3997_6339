namespace BlImplementation;
using BlApi;
internal class Bl : IBl
{
    public IAdmin Admin { get; } = new AdminImplementation();
    public ICourier Courier { get; } = new CourierImplementation();
    public IOrder Order { get; } = new OrderImplementation();

    //public IClosedDeliveryInList ClosedDeliveryInList { get; } = new ClosedDeliveryInListImplementation();
    //public ICourierInList CourierInList { get; } = new CourierInListImplementation();
    //public IDelivery Delivery { get; } = new DeliveryImplementation();
    //public IDeliveryPerOrderInList DeliveryPerOrderInList { get; } = new DeliveryPerOrderInListImplementation();
    //public IOpenOrderInList OpenOrderInList { get; } = new OpenOrderInListImplementation();
    //public IOrderInProgress OrderInProgress { get; } = new OrderInProgressImplementation();
    //public IOrderInList OrderInList { get; } = new OrderInListImplementation();


}
