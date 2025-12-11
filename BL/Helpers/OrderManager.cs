
using BlApi;
using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    public static int[] GetOrdersStatistics()
    {
        var allOrders = s_dal.Order.ReadAll();
        int maxStatusVal = (int)Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>().Max();
        int maxScheduleVal = (int)Enum.GetValues(typeof(BO.ScheduleStatus)).Cast<BO.ScheduleStatus>().Max();

        int[] results = new int[maxStatusVal + 1 + maxScheduleVal + 1];

        var query = (from order in allOrders
                     group order by order.OrderStatus into g
                     select new
                     {
                         Index = (int)g.Key,
                         Count = g.Count()
                     })
                           .Concat
                           (from order in allOrders
                            let timeStatus = Tools.GetScheduleStatus(order)
                            group order by timeStatus into g
                            select new
                            {
                                Index = (int)g.Key + maxStatusVal + 1,
                                Count = g.Count()
                            });

        foreach (var item in query)
        {
            results[item.Index] = item.Count;
        }
        return results;
    }

    public static void Create(BO.Order boOrder)
    {
        DO.Order doOrder = new DO.Order
        {
            Id = 0,
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,
            Latitude = boOrder.Latitude,
            Longitude = boOrder.Longitude,
            Name = boOrder.Name,
            Phone = boOrder.Phone,
            Weight = boOrder.Weight,
            OrderDate = boOrder.OrderDate,
        };
        s_dal.Order.Create(doOrder);
    }

    public static List<BO.OrderInList> ReadAll(BO.OrderInListField? filter,
        Object? filterValue,
        BO.OrderInListField orderBy = BO.OrderInListField.OrderStatus)
    {
        Func<BO.OrderInList, bool> filterPredicate = s_getFilterFunc(filter, filterValue);

        Func<BO.OrderInList, object> sortSelector = s_getSortFunc(orderBy);

        var Query = from doOrder in s_dal.Order.ReadAll()
                    let boOrder = s_convertToBoOrderInList(doOrder)
                    where filterPredicate(boOrder)
                    orderby sortSelector(boOrder)
                    select boOrder;
        return [.. Query];
    }

    public static BO.Order? Read(int id)
    {
        var doOrder = s_dal.Order.Read(id);
        if (doOrder is null)
            return null;

        BO.Order boOrder = new BO.Order
        {
            Id = doOrder.Id,
            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
            Details = doOrder.Details,
            Latitude = doOrder.Latitude,
            Longitude = doOrder.Longitude,
            Addres = doOrder.Addres,
            Name = doOrder.Name,
            Phone = doOrder.Phone,
            Weight = doOrder.Weight,
            OrderDate = doOrder.OrderDate,
            Distance = Tools.GetDistance(doOrder),
            EstimatedDeliveryTime = Tools.GetEstimatedDeliveryTime(doOrder),
            MaxDeliveryTime = doOrder.OrderDate + AdminManager.GetConfig().MaxDeliveryTime,
            OrderStatus = Tools.GetOrderStatus(doOrder),
            ScheduleStatus = Tools.GetScheduleStatus(doOrder), 
            TimeLeftForDelivery = Tools.GetTimeLeftForDelivery(doOrder), 
            DeliveryPerOrderInLists = s_createDeliveryPerOrderInList(doOrder.Id)
        };
        return boOrder;
    }
    public static void Update(BO.Order boOrder)
    {
        DO.Order doOrder = new DO.Order
        {
            Id = boOrder.Id,
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,
            Latitude = boOrder.Latitude,
            Longitude = boOrder.Longitude,
            Name = boOrder.Name,
            Phone = boOrder.Phone,
            Weight = boOrder.Weight,
            OrderDate = boOrder.OrderDate,

        };
        s_dal.Order.Update(doOrder);
    }


    public static void Delete(int id)
    {
        throw new BO.BlDoesNotExistException("Order cannot be deleted");
    }

    public static void ConcelOrder(int orderId)
    {
        DO.Order doOrder = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException("Order not found");
        Action action = doOrder.OrderStatus switch
        {
            DO.OrderStatus.COMPLETED => throw new BO.BlInvalidOperationException("Cannot cancel a completed order."),
            DO.OrderStatus.CONCELLED => throw new BO.BlInvalidOperationException("Order is already cancelled."),
            DO.OrderStatus.OPEN or DO.OrderStatus.REFUSED => () =>
            {
                doOrder = doOrder with { OrderStatus = DO.OrderStatus.CONCELLED };
                s_dal.Delivery.Create(new DO.Delivery
                {
                    Id = doOrder.Id,
                    OrderId = doOrder.Id,
                    CourierId = 0,
                    TypeOfOrder = doOrder.TypeOfOrder,
                    OrderDate = AdminManager.Now,
                    EndDelivery = DO.EndDelivery.CONCELLED,
                    TimeEndDelivery = AdminManager.Now,
                    ActualDistance = 0
                });
            }
            ,
            DO.OrderStatus.DELIVERING => () =>
            {
                var delivery = (from d in s_dal.Delivery?.ReadAll()
                                where d.OrderId == doOrder.Id
                                orderby d.Id descending
                                select d).FirstOrDefault()
                                ?? throw new BO.BlDoesNotExistException("Delivery not found for the order");
                s_dal.Delivery?.Update(delivery with
                {
                    EndDelivery = DO.EndDelivery.CONCELLED,
                    TimeEndDelivery = AdminManager.Now
                });
            }
            ,
            _ => throw new BO.BlInvalidOperationException("Invalid order status.")
        };
    }

   

    public static void OrderSelection(int courierId, int orderId)
    {
        DO.Order doOrder = s_dal.Order.Read(orderId)
            ?? throw new BO.BlDoesNotExistException("Order not found");
        BO.OrderInList boOrderInList = s_convertToBoOrderInList(doOrder);   
        if (boOrderInList.OrderStatus is BO.OrderStatus.OPEN or is BO.OrderStatus.REFUSED )
            throw new BO.BlInvalidOperationException("Order is not open for selection");
    }

    private static BO.OrderInList s_convertToBoOrderInList (DO.Order doOrder)
    {
        DO.Delivery? delivery = (from d in s_dal.Delivery?.ReadAll()
                           where d.OrderId == doOrder.Id
                           orderby d.Id descending
                           select d).FirstOrDefault();

        var orderStatus = Tools.GetOrderStatus(doOrder, delivery);

        return new BO.OrderInList
        {
            DeliveryId = delivery?.Id,
            OrderId = doOrder.Id,
            TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
            DistanceKm = Tools.GetDistance(doOrder),
            OrderStatus = orderStatus,
            ScheduleStatus = Tools.GetScheduleStatus(doOrder, delivery),
            TimeLeftForDelivery = Tools.GetTimeLeftForDelivery(doOrder, orderStatus),
            TotalTimeOfDelivery = Tools.GetTotalTimeOfDelivery(doOrder, orderStatus, delivery),
            NumberOfDeliveryAttempts = Tools.GetCuntOfDelivery(doOrder.Id)
        };
    }


    private static Func<BO.OrderInList, bool> s_getFilterFunc(BO.OrderInListField? filter, Object? filterValue)
    {
        if (filter is not null && filterValue is null)
            throw new Exception("not send value for filter");
        return filter switch
        {
            BO.OrderInListField.OrderId => (o) => o.OrderId == (int)filterValue!,
            BO.OrderInListField.TypeOfOrder => (o) => o.TypeOfOrder == (BO.TypeOfOrder)filterValue!,
            BO.OrderInListField.OrderStatus => (o) => o.OrderStatus == (BO.OrderStatus)filterValue!,
            BO.OrderInListField.DistanceKm => (o) => o.DistanceKm == (double)filterValue!,
            BO.OrderInListField.ScheduleStatus => (o) => o.ScheduleStatus == (BO.ScheduleStatus)filterValue!,
            BO.OrderInListField.TimeLeftForDelivery => (o) => o.TimeLeftForDelivery <= (TimeSpan)filterValue!,
            BO.OrderInListField.TotalTimeOfDelivery => (o) => o.TotalTimeOfDelivery <= (TimeSpan)filterValue!,
            BO.OrderInListField.DeliveryAttempts => (o) => o.NumberOfDeliveryAttempts == (int)filterValue!,
            _ => (o) => true
        };
    }

    private static Func<BO.OrderInList, Object> s_getSortFunc(BO.OrderInListField? sort)
    {
        return sort switch
        {
            BO.OrderInListField.OrderId => (o) => o.OrderId,
            BO.OrderInListField.TypeOfOrder => (o) => o.TypeOfOrder,
            BO.OrderInListField.OrderStatus => (o) => o.OrderStatus,
            BO.OrderInListField.DistanceKm => (o) => o.DistanceKm,
            BO.OrderInListField.ScheduleStatus => (o) => o.ScheduleStatus,
            BO.OrderInListField.TimeLeftForDelivery => (o) => o.TimeLeftForDelivery,
            BO.OrderInListField.TotalTimeOfDelivery => (o) => o.TotalTimeOfDelivery,
            _ => (o) => o.OrderStatus
        };
    }

    private static List<BO.DeliveryPerOrderInList>? s_createDeliveryPerOrderInList(int orderId)
    {
        var deliveries = from doDelivery in s_dal.Delivery.ReadAll(d => d.OrderId == orderId)
                         let courier = s_dal.Courier.Read(doDelivery.CourierId)
                         select new BO.DeliveryPerOrderInList
                         {
                             DeliveryId = doDelivery.Id,
                             CourierId = courier.Id,
                             CourierName = courier.Name,
                             TypeShipment = (BO.TheTypeShipment)courier.TypeShipment,
                             OrderDate = doDelivery.OrderDate,
                             EndDelivery = doDelivery.EndDelivery.HasValue ? (BO.EndDelivery)doDelivery.EndDelivery.Value : null,
                             TimeEndDelivery = doDelivery.TimeEndDelivery.HasValue ? doDelivery.TimeEndDelivery.Value : null
                         }
        ;
        return deliveries.Any() ? [.. deliveries] : null;
    }

}


//if (filter is not null && filterValue is null)
//    throw new Exception("not send value for filter");
//Func<DO.Order, bool>? filterFunc = filter switch
//{
//    BO.OrderInListField.OrderId => ((o) => o.Id == (int)filterValue!),
//    BO.OrderInListField.TypeOfOrder => (o) => o.TypeOfOrder == (DO.TypeOfOrder)filterValue!,
//    BO.OrderInListField.OrderStatus => (o) => o.OrderStatus == (DO.OrderStatus)filterValue!,
//    BO.OrderInListField.DistanceKm => (o) => Tools.GetDistance(o) == (double)filterValue!,
//    BO.OrderInListField.ScheduleStatus => (o) => Tools.GetScheduleStatus(o) == (BO.ScheduleStatus)filterValue!,
//    BO.OrderInListField.TimeLeftForDelivery => (o) =>
//    {
//        TimeSpan timeLeft = TimeSpan.Zero;
//        if (o.OrderStatus is not DO.OrderStatus.COMPLETED)
//        {
//            DateTime maxDeliveryTime = o.OrderDate + AdminManager.GetConfig().MaxDeliveryTime;
//            timeLeft = maxDeliveryTime - AdminManager.Now;
//        }
//        return timeLeft <= (TimeSpan)filterValue!;
//    }
//    ,
//    BO.OrderInListField.TotalTimeOfDelivery => (DO.Order order) =>
//    {
//        TimeSpan TotalTime = TimeSpan.Zero;
//        if (order.OrderStatus is DO.OrderStatus.COMPLETED)
//        {
//            DateTime endDelivery = (from dlivery in s_dal.Delivery
//                              .ReadAll(d => d.OrderId == order.Id)
//                                    orderby dlivery.Id descending
//                                    select dlivery.TimeEndDelivery)
//                              .FirstOrDefault() ??
//                              throw new NotImplementedException("is complet but do not ave date to complet");
//            TotalTime = endDelivery - order.OrderDate;
//        }

//        return TotalTime <= (TimeSpan)filterValue!;
//    }
//    ,
//    _ => null
//};