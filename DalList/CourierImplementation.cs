namespace Dal;
using DalApi;
using DO;
using System.Reflection.Metadata.Ecma335;

//using System.Collections.Generic;

/// <summary>
/// Implementation of the ICourier interface for managing courier data in the data access layer.
/// Provides CRUD operations for courier entities stored in memory.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier in the data source.
    /// </summary>
    /// <param name="item">The courier object to create.</param>
    /// <exception cref="Exception">Thrown when a courier with the same ID already exists.</exception>
    public void Create(Courier item)
    {
        if(Read(item.Id)is not null)
            throw new Exception($"Courier with ID={item.Id} already exists");
        else
            DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Deletes a courier from the data source by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to delete.</param>
    /// <exception cref="Exception">Thrown when a courier with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        var courier = Read(id);
        if(courier is not null)
            DataSource.Couriers.Remove(courier);
        else
            throw new Exception($"Courier with ID={id} does not exists");
    }

    /// <summary>
    /// Deletes all couriers from the data source.
    /// </summary>
    public void DeleteAll()  => DataSource.Couriers.Clear();
  
    /// <summary>
    /// Reads and retrieves a courier by their unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the courier to retrieve.</param>
    /// <returns>The courier object if found; otherwise, null.</returns>
    public Courier? Read(int id)
    {
        foreach (var courier in DataSource.Couriers)
        {
            if (courier.Id == id)
                return courier; 
        }

       return null;
    }
    
    /// <summary>
    /// Reads and retrieves all couriers from the data source.
    /// </summary>
    /// <returns>A list containing all courier objects.</returns>
    public List<Courier> ReadAll() => new List<Courier>(DataSource.Couriers);

    /// <summary>
    /// Updates an existing courier in the data source.
    /// </summary>
    /// <param name="item">The courier object with updated information.</param>
    /// <exception cref="Exception">Thrown when a courier with the specified ID does not exist.</exception>
    public void Update(Courier item)
    {
        var courier = Read(item.Id);
        if (courier is not null)
        {
            DataSource.Couriers.Remove(courier);
            DataSource.Couriers.Add(item);
        }
        else
            throw new Exception($"Courier with ID={item.Id} does not exists");
    }
}
