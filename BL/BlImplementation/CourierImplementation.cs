
namespace BlImplementation;
using BlApi;

using Helpers;

internal class CourierImplementation : ICourier
{
    public void Create(int requesterId, BO.Courier boCourier)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException();
        CourierManager.Create(boCourier);
    }

    public BO.Courier? Read(int requesterId, int courierId)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException();

        return CourierManager.Read(courierId);
    }

    
    public void Update(int requesterId, BO.Courier boCourier)
    {
        if (!Tools.CheckManger(requesterId) && requesterId != boCourier.Id)
            throw new BO.BlNoAccessException();
        // Remove duplicate update call
        CourierManager.Update(requesterId, boCourier);
    }

    public void Delete(int requesterId, int courierId)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException();
        CourierManager.Delete(courierId);
    }

    public string? Login(int id, string password)
    {
        return CourierManager.Login(id, password);
    }
    

    public IEnumerable<BO.CourierInList> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort)
    {
        if (!Tools.CheckManger(requesterId))
            throw new BO.BlNoAccessException("Only manager can access the list of couriers.");
        return CourierManager.ReadAll(requesterId, isActive, sort);
    }




}
