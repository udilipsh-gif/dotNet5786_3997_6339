

namespace Dal;
using DalApi;
using DO;
using System.Reflection.Metadata.Ecma335;

//using System.Collections.Generic;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if(Read(item.Id)is not null)
            throw new Exception($"Courier with ID={item.Id} already exists");
        else
            DataSource.Couriers.Add(item);
    }

    public void Delete(int id)
    {
        var courier = Read(id);
        if(courier is not null)
            DataSource.Couriers.Remove(courier);
        else
            throw new Exception($"Courier with ID={id} does not exists");
    }

    public void DeleteAll()  => DataSource.Couriers.Clear();
  
   
    public Courier? Read(int id)
    {
        foreach (var courier in DataSource.Couriers)
        {
            if (courier.Id == id)
                return courier; 
        }

       return null;
    }
    public List<Courier> ReadAll() => new List<Courier>(DataSource.Couriers);


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
