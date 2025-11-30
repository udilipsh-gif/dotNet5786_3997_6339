
namespace BlImplementation;
using BlApi;

using Helpers;

internal class CourierImplementation : ICourier
{

    public void Create(int id, BO.Courier boCourier)
    {
        if (id != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException();

        CourierManager.Create(boCourier);
    }

    public BO.Courier? Read(int id, int courierId)
    {
        if (id != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException();

        return CourierManager.Read(courierId);
    }

    
    public void Update(int requesterId, BO.Courier boCourier)
    {
       if (requesterId != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException();

        CourierManager.Update(requesterId, boCourier);
    }

    public void Delete(int id, int courierId)
    {
        if (id != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException();
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
        if (requesterId != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException();
        return CourierManager.ReadAll(requesterId, isActive, sort);
    }




}
