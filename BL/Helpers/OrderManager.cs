using DalApi;


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
            Addres = boOrder.Addres,//חישוב
            Latitude = boOrder.Latitude,
            Longitude = boOrder.Longitude,
            Name = boOrder.Name,
            Phone = boOrder.Phone,//חישוב
            Weight = boOrder.Weight,
            OrderDate = AdminManager.Now,
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
        if (!Tools.IsValidPhone(boOrder.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");

        var adressCoordinates = Tools.GetGeocodingSync(boOrder.Addres);

        DO.Order doOrder = new DO.Order
        {
            Id = boOrder.Id,
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,//חישוב
            Latitude = adressCoordinates?.Lat ?? boOrder.Latitude,
            Longitude = adressCoordinates?.Lng ?? boOrder.Longitude,
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
                    TypeShipment = DO.TheTypeShipment.FOOT,
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

        DO.Courier? doCourier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        BO.OrderInList boOrderInList = s_convertToBoOrderInList(doOrder);
        if (boOrderInList.OrderStatus is not (BO.OrderStatus.OPEN or BO.OrderStatus.REFUSED))
            throw new BO.BlInvalidOperationException("Order is not open for selection");

        s_dal.Delivery.Create(new DO.Delivery
        {
            Id = 0,
            OrderId = orderId,
            CourierId = courierId,
            TypeShipment = (DO.TheTypeShipment)doCourier.TypeShipment,
            OrderDate = AdminManager.Now,
        });
        s_dal.Order.Update(doOrder with
        {
            OrderStatus = DO.OrderStatus.DELIVERING
        });
    }

    public static List<BO.ClosedDeliveryInList> GetClosedOrderInLists(int courierId, BO.TypeOfOrder? filter, BO.ClosedDeliveryInListField sort)
    {
        var query = from doDelivery in s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.EndDelivery != null)
                    let order = s_dal.Order.Read(doDelivery.OrderId)
                    where order != null && (filter == null || (BO.TypeOfOrder)order.TypeOfOrder == filter)
                    select new BO.ClosedDeliveryInList
                    {
                        DeliveryId = doDelivery.Id,
                        OrderId = order.Id,
                        OrderType = (BO.TypeOfOrder)order.TypeOfOrder,
                        Address = order.Addres,
                        ShipmentType = (BO.TheTypeShipment)doDelivery.TypeShipment,
                        AqualDistens = doDelivery.ActualDistance,
                        DelyveryTime = (TimeSpan)(doDelivery.TimeEndDelivery! - doDelivery.OrderDate),
                        EndDelivery = (BO.EndDelivery)doDelivery.EndDelivery!
                    };
        var uniqueQuery = query.DistinctBy(x => x.OrderId);

        IEnumerable<BO.ClosedDeliveryInList> sortedQuery = sort switch
        {
            BO.ClosedDeliveryInListField.DeliveryId => uniqueQuery.OrderBy(x => x.DeliveryId),
            BO.ClosedDeliveryInListField.OrderId => uniqueQuery.OrderBy(x => x.OrderId),
            BO.ClosedDeliveryInListField.TypeOfOrder => uniqueQuery.OrderBy(x => x.OrderType),
            BO.ClosedDeliveryInListField.Address => uniqueQuery.OrderBy(x => x.Address),
            BO.ClosedDeliveryInListField.ShipmentType => uniqueQuery.OrderBy(x => x.ShipmentType),
            BO.ClosedDeliveryInListField.AqualDistens => uniqueQuery.OrderBy(x => x.AqualDistens),
            BO.ClosedDeliveryInListField.DelyveryTime => uniqueQuery.OrderBy(x => x.DelyveryTime),
            BO.ClosedDeliveryInListField.EndDelivery => uniqueQuery.OrderBy(x => x.EndDelivery),
            _ => uniqueQuery.OrderBy(x => x.OrderType) // ברירת מחדל
        };

        return [.. sortedQuery];
    }

    public static List<BO.OpenOrderInList> GetOrdersForDelivery(int courierId, BO.TypeOfOrder? filter, BO.OpenOrderInListField sort)
    {
        DO.Courier doCourier = s_dal.Courier.Read(courierId)
            ?? throw new BO.BlDoesNotExistException("Courier not found");

        var query = from doOrder in s_dal.Order.ReadAll(o => o.OrderStatus == DO.OrderStatus.OPEN || o.OrderStatus == DO.OrderStatus.REFUSED)
                    let distense = Tools.GetDistance(doOrder)
                    where ((filter == null || (BO.TypeOfOrder)doOrder.TypeOfOrder == filter) && (distense <= doCourier.MaxDistanceDelivery))
                    select new BO.OpenOrderInList
                    {
                        OrderId = doOrder.Id,
                        TypeOfOrder = (BO.TypeOfOrder)doOrder.TypeOfOrder,
                        Weight = doOrder.Weight,
                        Address = doOrder.Addres,
                        ActualDistance = null,//לא מחושב עדיין
                        DistanceKm = distense,
                        EstimatedDeliveryTime = null,
                        ScheduleStatus = Tools.GetScheduleStatus(doOrder),
                        TimeLeftForDelivery = Tools.GetTimeLeftForDelivery(doOrder),
                        MaxDeliveryTime = doOrder.OrderDate + AdminManager.GetConfig().MaxDeliveryTime
                    };

        IEnumerable<BO.OpenOrderInList> sortedQuery = sort switch
        {
            BO.OpenOrderInListField.OrderId => query.OrderBy(x => x.OrderId),
            BO.OpenOrderInListField.TypeOfOrder => query.OrderBy(x => x.TypeOfOrder),
            BO.OpenOrderInListField.Weight => query.OrderBy(x => x.Weight),
            BO.OpenOrderInListField.Address => query.OrderBy(x => x.Address),
            BO.OpenOrderInListField.DistanceKm => query.OrderBy(x => x.DistanceKm),
            BO.OpenOrderInListField.ActualDistance => query.OrderBy(x => x.ActualDistance),
            BO.OpenOrderInListField.EstimatedDeliveryTime => query.OrderBy(x => x.EstimatedDeliveryTime),
            BO.OpenOrderInListField.ScheduleStatus => query.OrderBy(x => x.ScheduleStatus),
            BO.OpenOrderInListField.TimeLeftForDelivery => query.OrderBy(x => x.TimeLeftForDelivery),
            BO.OpenOrderInListField.MaxDeliveryTime => query.OrderBy(x => x.MaxDeliveryTime),
            _ => query.OrderBy(x => x.ScheduleStatus) // ברירת מחדל
        };

        return [.. sortedQuery];
    }

    private static BO.OrderInList s_convertToBoOrderInList(DO.Order doOrder)
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

