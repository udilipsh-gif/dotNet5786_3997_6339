

namespace Dal;
using DalApi;
using DO;
//using System.Collections.Generic;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if(Read(item.Id)is not null)
            throw new Exception($"Courier with ID={item.Id} already exists");

        DataSource.Couriers.Add(item);
    }

    public void Delete(int id)
    {
        if (Read(id) is not not null)
    }

    public void DeleteAll()
    {
        throw new NotImplementedException();
    }

    
    public Courier? Read(int id)
    {
        foreach (var courier in DataSource.Couriers)
        {
            if (courier.Id == id)
                return courier; 
        }

       return null;
    }


    public List<Courier> ReadAll()
    {
        return new List<Courier>(DataSource.Couriers);
    }


    public void Update(Courier item)
    {
        var returnCourier = Read(item.Id);
        if (returnCourier is null)
            throw new Exception($"Courier with ID={item.Id} does not exists");
        int index = DataSource.Couriers.IndexOf(returnCourier);
        DataSource.Couriers[index] = item;
    }
}
