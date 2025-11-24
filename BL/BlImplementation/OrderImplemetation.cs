
namespace BlImplementation;
using BlApi;
using BO;
using Helpers;


internal class OrderImplemetation: IOrder
{
     public void Create (BO.Order boOrder)
    {
        OrderManager.Create(boOrder);
    }
    public BO.Order? Read (int id)
    {
        return OrderManager.Read(id);
    }
    public void Update(BO.Order boOrder)
    {
        OrderManager.Update(boOrder);
    }
    public void Delete (int id)
    {
        OrderManager.Delete(id);
    }


}
