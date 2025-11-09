using DalApi;

namespace Dal;

internal class DalList : IDal
{
    public ICourier Courier => throw new NotImplementedException();

    public IOrder Order => throw new NotImplementedException();

    public IDelivery Delivery => throw new NotImplementedException();

    public IConfig Config => throw new NotImplementedException();

    public void ResetDB()
    {
        throw new NotImplementedException();
    }
}
