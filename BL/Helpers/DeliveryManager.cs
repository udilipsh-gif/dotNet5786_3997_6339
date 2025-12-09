
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

    //internal static void PeriodicDeliveriesUpdates(DateTime oldClock, DateTime newClock)
    //{
    //    // קריאת המשלוחים שעדיין לא הסתיימו
    //    var deliveries = s_dal.Delivery.ReadAll(d => d.EndDelivery == null);

    //    TimeSpan maxTime = s_dal.Config.MaxDeliveryTime;

    //    foreach (var delivery in deliveries)
    //    {
    //        // אם הזמן החדש עבר את זמן המשלוח המקסימלי
    //        if (newClock >= delivery.OrderDate.Add(maxTime))
    //        {
    //            var updated = delivery with
    //            {
    //                EndDelivery = DO.EndDelivery.DELIVERED,
    //                TimeEndDelivery = newClock
    //            };

    //            s_dal.Delivery.Update(updated);
    //        }
    //    }
    //}


}
