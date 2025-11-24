
namespace BlApi;

public interface IBI
{
    IAdmin Admin { get; }
    ICourier Courier { get; }
    IOrder Order { get; }
    IClosedDeliveryInList ClosedDeliveryInList { get; }
    ICourierInList CourierInList { get; }
  //  IDelivery Delivery { get; }
    IDeliveryPerOrderInList DeliveryPerOrderInList { get; }
    IOpenOrderInList OpenOrderInList { get; }
    IOrderInProgress OrderInProgress { get; }
    IOrderInList OrderInList { get; }

}
