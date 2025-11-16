using DalApi;
namespace Dal;

sealed public class DalXml : IDal
{
    public ICourier Courier => new CourierImplementation();

    public IOrder Order => new OrderImplementation();

    public IDelivery Delivery => new DeliveryImplementation();

    public IConfig Config => new ConfigImplementation();

    public void ResetDB()
    {
        Delivery.DeleteAll();
        Courier.DeleteAll();
        Order.DeleteAll();
        Config.Reset();
    }
}
