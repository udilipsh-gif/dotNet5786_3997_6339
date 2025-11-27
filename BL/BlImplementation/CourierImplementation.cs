
namespace BlImplementation;
using BlApi;
using Helpers;
using System.Reflection.Metadata.Ecma335;

internal class CourierImplementation : ICourier
{

    public void Create(int id, BO.Courier boCourier) => CourierManager.Create(boCourier);

    public BO.Courier? Read(int id, int OrderId) => CourierManager.Read(OrderId);
    
    //public IEnumerable<BO.CourierInList> ReadAll(
    //     BO.CourierFieldSort? sort = null,
    //     BO.CourierFieldFilter? filter = null,
    //     object? value = null)
    // {
    //     return CourierManager.ReadAll(sort, filter, value);
    //     //throw new NotImplementedException();
    // }
    public void Update(BO.Courier boCourier) => CourierManager.Update(boCourier);
    
    public void Delete(int id, int orderId) => CourierManager.Delete(id);
    
    public string? Login(int id, string password)
    {
        return CourierManager.Login(id, password);
    }
    public IEnumerable<BO.CourierInList> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort)
    {
        return CourierManager.ReadAll(requesterId, isActive, sort);
    }



}
