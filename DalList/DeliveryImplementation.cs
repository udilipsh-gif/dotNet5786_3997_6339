namespace Dal;
using DalApi;
using DO;

/// <summary>
/// Implementation of the IDelivery interface for managing delivery data in the data access layer.
/// Provides CRUD operations for delivery entities stored in memory.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new delivery in the data source with an auto-generated unique ID.
    /// </summary>
    /// <param name="item">The delivery object to create. The ID will be automatically assigned.</param>
    public void Create(Delivery item)
    {
        int newId = Config.NextDeliveryId;
        var newItem = item with { Id = newId };
        DataSource.Deliveries.Add(newItem);
    }

    /// <summary>
    /// Deletes a delivery from the data source by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to delete.</param>
    /// <exception cref="Exception">Thrown when a delivery with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        var delivery = Read(id);
        if (delivery is null)
            throw new Exception($"Delivery with ID={id} does not exists");
        else
        {
            DataSource.Deliveries.Remove(delivery!);
        }
    }

    /// <summary>
    /// Deletes all deliveries from the data source.
    /// </summary>
    public void DeleteAll() => DataSource.Deliveries.Clear();

    /// <summary>
    /// Reads and retrieves a delivery by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery to retrieve.</param>
    /// <returns>The delivery object if found; otherwise, null.</returns>
    public Delivery? Read(int id)
    {
        //foreach (var delivery in DataSource.Deliveries)
        //{
        //    if (delivery.Id == id)
        //        return delivery;
        //}

        //return null;
        return DataSource.Deliveries.FirstOrDefault(item => item.Id == id);
    }
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        var delivery = (from item in DataSource.Deliveries
                     where filter(item)
                     select item).FirstOrDefault();
        return delivery;
    }

    /// <summary>
    /// Reads and retrieves all deliveries from the data source.
    /// </summary>
    /// <returns>A list containing all delivery objects.</returns>
    // public List<Delivery> ReadAll() => new List<Delivery>(DataSource.Deliveries);//stage 1
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null) //stage 2
         => filter != null
             ? from item in DataSource.Deliveries
               where filter(item)
               select item
             : from item in DataSource.Deliveries
               select item;

    /// <summary>
    /// Updates an existing delivery in the data source.
    /// </summary>
    /// <param name="item">The delivery object with updated information.</param>
    /// <exception cref="Exception">Thrown when a delivery with the specified ID does not exist.</exception>
    public void Update(Delivery item)
    {
        var delivery = Read(item.Id);
        if (delivery is not null)
        {
            DataSource.Deliveries.Remove(delivery);
            DataSource.Deliveries.Add(item);
        }
        else
            throw new Exception($"Delivery with ID={item.Id} does not exists");
    }
}
