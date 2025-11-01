

namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public void DeleteAll()
    {
        throw new NotImplementedException();
    }

    public Order? Read(int id)
    {
        foreach (var order in DataSource.Orders)
        {
            if (order.Id == id)
                return order;
        }

        return null;
    }

    public List<Order> ReadAll()
    {
        throw new NotImplementedException();
    }

    public void Update(Order item)
    {
        throw new NotImplementedException();
    }
}
