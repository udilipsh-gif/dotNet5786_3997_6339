
namespace BlImplementation;
using BlApi;

using Helpers;


internal class OrderImplementation : IOrder
{
    public void Create(int id, BO.Order boOrder)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Create(boOrder);
    }
    public BO.Order? Read(int id, int orderId)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        return OrderManager.Read(id);
    }
    public void Update(int id, BO.Order boOrder)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Update(boOrder);
    }
    public void Delete(int id, int orderId)
    {
        if (Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        OrderManager.Delete(id);
    }
    public void StartDelivery(int id, int courierId, int orderId)
    {
        if (!Tools.CheckManger(id) && id != courierId) //העברה ל DELYVERY MANGER
            throw new BO.BlNoAccessException();
        OrderManager.StartDelivery(courierId, orderId);
    }
    public IEnumerable<BO.OrderInList> ReadAll(
        int id,
        BO.OrderInListField? filter,
        object? value,
        BO.OrderInListField? sort)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();
        return OrderManager.ReadAll(filter, value, sort);
    }

    public void Deliver(int id, int courierId, int orderId)
    {
        if (!Tools.CheckManger(id) && id != courierId) //העברה ל DELYVERY MANGER
            throw new BO.BlNoAccessException();
        DeliveryManager.Deliver(courierId, orderId);
    }
    public void Cancel(int id, int orderId)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        OrderManager.Cancel(orderId);
    }
    public int[] GetAllOrderStatistic(int id)
    {
        if (!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return OrderManager.GetAllOrderStatistic();
    }
    public IEnumerable<BO.ClosedDeliveryInList> GetClosed(int id, int courierId, BO.TypeOfOrder? filter, BO.ClosedDeliveryInListField? sort)
    {
        if(!Tools.CheckManger(id))
            throw new BO.BlNoAccessException();

        return OrderManager.GetClosed(courierId, filter, sort);
    }
    public IEnumerable<BO.OpenOrderInList> GetOpen(int id, int courierId, BO.TypeOfOrder? filter, BO.OpenOrderInListField? sort)
    {
        if(!Tools.CheckManger(id) && id != courierId)
            throw new BO.BlNoAccessException();

        return OrderManager.GetOpen(courierId, filter, sort);
    }


}
