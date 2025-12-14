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

    internal static DO.Delivery? Read(int id)
    {
        return s_dal.Delivery.Read(id);
    }

    public static void Deliver(int courierId, int deliveryId)
    {
        DO.Delivery delivery = s_dal.Delivery.Read(deliveryId)
            ?? throw new BO.BlDoesNotExistException($"Delivery with ID {deliveryId} not found");
        if (delivery.CourierId != courierId)
            throw new BO.BlInvalidValueException($"Courier with ID {courierId} is not assigned to this delivery  ");
        delivery = delivery with
        {
            EndDelivery = DO.EndDelivery.DELIVERED,
            TimeEndDelivery = AdminManager.Now
        };
        s_dal.Delivery.Update(delivery);
    }

    public static void Create(DO.Order order, DO.Courier courier)
    {
        if(order.DistanceKm > courier.MaxDistanceDelivery)
            throw new BO.BlInvalidValueException($"Courier with ID {courier.Id} cannot deliver to distance {order.DistanceKm} km");

        DO.Delivery delivery = new DO.Delivery
        {
            Id = 0,
            OrderId = order.Id,
            CourierId = courier.Id,
            TypeShipment = (DO.TheTypeShipment)order.TypeOfOrder,
            OrderDate = AdminManager.Now,
            ActualDistance = Tools.GetActualDistance(order.Addres, (BO.TheTypeShipment)order.TypeOfOrder), // updated method call
            EndDelivery = null,
            TimeEndDelivery = null
        };
        s_dal.Delivery.Create(delivery);
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
