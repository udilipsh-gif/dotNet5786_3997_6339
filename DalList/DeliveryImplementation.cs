

namespace Dal;
using DalApi;
using DO;

internal class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        int newId = Config.NextDeliveryId;
        var newItem = item with { Id = newId };
        DataSource.Deliveries.Add(newItem);
    }

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

    public void DeleteAll() => DataSource.Deliveries.Clear();

    public Delivery? Read(int id)
    {
        foreach (var delivery in DataSource.Deliveries)
        {
            if (delivery.Id == id)
                return delivery;
        }

        return null;
    }

    public List<Delivery> ReadAll() => new List<Delivery>(DataSource.Deliveries);


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
