
namespace BlImplementation;
using BlApi;

using Helpers;


internal class OrderImplementation : IOrder
{
     public void Create (int id, BO.Order boOrder)
    {
        OrderManager.Create(boOrder);
    }
    public BO.Order? Read (int id, int orderId)
    {
        return OrderManager.Read(id);
    }
    public void Update(int id, BO.Order boOrder)
    {
        OrderManager.Update(boOrder);
    }
    public void Delete (int id, int orderId)
    {
        OrderManager.Delete(id);
    }
    public void StartDelivery(int id, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }
    public BO.OrderInList ReadAll(int id, BO.OrderInListField? filter, object? value, BO.OrderInListField? sort)
    {
        throw new NotImplementedException();
    }

    public void Deliver(int id, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }
    public void Cancel(int id, int orderId)
    {
        throw new NotImplementedException();
    }
    public int[] GetAllOrderStatistic(int id)
    {
        throw new NotImplementedException();
    }
    public BO.ClosedDeliveryInList GetClosed(int id, int courierId, BO.ClosedDeliveryInListField? filter, BO.ClosedDeliveryInListField sort)
    {
        throw new NotImplementedException();
    }
    public BO.OpenOrderInList GetOpen(int id, int courierId, BO.OpenOrderInListField? filter, BO.OpenOrderInListField sort)
    {
        throw new NotImplementedException();
    }


}
