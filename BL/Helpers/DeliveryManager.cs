
using DalApi;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    internal static IEnumerable<DO.Delivery> ReadAll(

    BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        return s_dal.Delivery.ReadAll();
    }

}
