

namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        var newId = Config.NextOrderId;
        var newItem = item with { Id = newId };
        DataSource.Orders.Add(newItem);
    }

    public void Delete(int id)
    {
        var order = Read(id);
        if (order is null)
            throw new Exception($"Order with ID={id} does not exists");
        else
            DataSource.Orders.Remove(order!);
    }

    public void DeleteAll() => DataSource.Orders.Clear();


    public Order? Read(int id)
    {
        foreach (var order in DataSource.Orders)
        {
            if (order.Id == id)
                return order;
        }

        return null;
    }

    public List<Order> ReadAll() => new List<Order>(DataSource.Orders);

    public void Update(Order item)
    {
        var order = Read(item.Id);
        if (order is null)
            throw new Exception($"Order with ID={item.Id} does not exists");
        else
        {
            DataSource.Orders.Remove(order);
            DataSource.Orders.Add(item);
        }
    }
}
