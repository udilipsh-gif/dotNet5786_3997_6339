
namespace BlImplementation;
using BlApi;
using Helpers;

internal class CourierImplementation : ICourier
{
    public void Create(BO.Courier boCourier)
    {
        CourierManager.Create(boCourier);
    }
    
    public BO.Courier? Read(int id)
    {
        return CourierManager.Read(id);
        
    }
   public IEnumerable<BO.CourierInList> ReadAll(
        BO.CourierFieldSort? sort = null,
        BO.CourierFieldFilter? filter = null,
        object? value = null)
    {
        return CourierManager.ReadAll(sort, filter, value);
        //throw new NotImplementedException();
    }
    public void Update(BO.Courier boCourier)
    {
        CourierManager.Update(boCourier);
        //throw new NotImplementedException();
    }
   public void Delete(int id)
    {
        CourierManager.Delete(id);

    }

}
