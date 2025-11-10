namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Implementation of the IOrder interface for managing order data in the data access layer.
/// Provides CRUD operations for order entities stored in memory.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order in the data source with an auto-generated unique ID.
    /// </summary>
    /// <param name="item">The order object to create. The ID will be automatically assigned.</param>
    public void Create(Order item)
    {
        var newId = Config.NextOrderId;
        var newItem = item with { Id = newId };
        DataSource.Orders.Add(newItem);
    }

    /// <summary>
    /// Deletes an order from the data source by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <exception cref="Exception">Thrown when an order with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        var order = Read(id);
        if (order is null)
            throw new DalDoesNotExistException(id);
        else
            DataSource.Orders.Remove(order!);
    }

    /// <summary>
    /// Deletes all orders from the data source.
    /// </summary>
    public void DeleteAll() => DataSource.Orders.Clear();

    /// <summary>
    /// Reads and retrieves an order by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the order to retrieve.</param>
    /// <returns>The order object if found; otherwise, null.</returns>
    public Order? Read(int id)
    {
        //foreach (var order in DataSource.Orders)
        //{
        //    if (order.Id == id)
        //        return order;
        //}

        //return null;
        return DataSource.Orders.FirstOrDefault(item => item.Id == id);
    }
    public Order? Read(Func<Order, bool> filter)
    {
        var order = (from item in DataSource.Orders
                       where filter(item)
                       select item).FirstOrDefault();
        return order;
    }

    /// <summary>
    /// Reads and retrieves all orders from the data source.
    /// </summary>
    /// <returns>A list containing all order objects.</returns>
   // public List<Order> ReadAll() => new List<Order>(DataSource.Orders);//stage 1
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null) //stage 2
        => filter != null
            ? from item in DataSource.Orders
              where filter(item)
              select item
            : from item in DataSource.Orders
              select item;

    /// <summary>
    /// Updates an existing order in the data source.
    /// </summary>
    /// <param name="item">The order object with updated information.</param>
    /// <exception cref="Exception">Thrown when an order with the specified ID does not exist.</exception>
    public void Update(Order item)
    {
        var order = Read(item.Id);
        if (order is null)
            throw new DalDoesNotExistException(item.Id);
        else
        {
            DataSource.Orders.Remove(order);
            DataSource.Orders.Add(item);
        }
    }
}
