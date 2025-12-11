
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
            Id = boOrder.Id,//צריך לקחת מהקונפיג של השכבה התחתונה מס רץ
            TypeOfOrder = (DO.TypeOfOrder)boOrder.TypeOfOrder,
            Details = boOrder.Details,
            Addres = boOrder.Addres,//חישוב תקינות כתובת
            Latitude = boOrder.Latitude,
            Longitude = boOrder.Longitude,
            Name = boOrder.Name,
            Phone = boOrder.Phone,//חישוב תקינות טלפון
            Weight = boOrder.Weight,
            OrderDate = boOrder.OrderDate,

            //OrderStatus=(DO.OrderStatus)boOrder.OrderStatus,
            //DistanceKm=boOrder.DistanceKm,
            // DistanceKmRoad=boOrder.DistanceKmdRoad,
            //DistanceKmWalk=boOrder.DistanceKmWalk,



        };
        s_dal.Order.Create(doOrder);
    }

    public static List<BO.OrderInList> ReadAll(BO.OrderInListField? filter,
        Object? filterValue,
        BO.OrderInListField orderBy = BO.OrderInListField.OrderStatus)
    {
        if (filter is not null && filterValue is null)
            throw new Exception("not send value for filter");
        Func<DO.Order, bool>? filterFunc = filter switch
        {
            BO.OrderInListField.OrderId => ((o) => o.Id == (int)filterValue!),
            BO.OrderInListField.TypeOfOrder => (o) => o.TypeOfOrder == (DO.TypeOfOrder)filterValue!,
            BO.OrderInListField.OrderStatus => (o) => o.OrderStatus == (DO.OrderStatus)filterValue!,
            BO.OrderInListField.DistanceKm => (o) => Tools.GetDistance(o) == (double)filterValue!,
            BO.OrderInListField.ScheduleStatus => (o) => Tools.GetScheduleStatus(o) == (BO.ScheduleStatus)filterValue!,
            BO.OrderInListField.TimeLeftForDelivery => (o) =>
            {
                TimeSpan timeLeft = TimeSpan.Zero;
                if (o.OrderStatus is not DO.OrderStatus.COMPLETED)
                {
                    DateTime maxDeliveryTime = o.OrderDate + AdminManager.GetConfig().MaxDeliveryTime;
                    timeLeft = maxDeliveryTime - AdminManager.Now;
                }
                return timeLeft <= (TimeSpan)filterValue!;
            }
            ,
            BO.OrderInListField.TotalTimeOfDelivery => (DO.Order order) =>
            {
                TimeSpan TotalTime = TimeSpan.Zero;
                if (order.OrderStatus is DO.OrderStatus.COMPLETED)
                {
                    DateTime endDelivery = (from dlivery in s_dal.Delivery
                                      .ReadAll(d => d.OrderId == order.Id)
                                            orderby dlivery.Id descending
                                            select dlivery.TimeEndDelivery)
                                      .FirstOrDefault() ??
                                      throw new NotImplementedException("is complet but do not ave date to complet");
                    TotalTime = endDelivery - order.OrderDate;
                }

                return TotalTime <= (TimeSpan)filterValue!;
            }
            ,
            _ => null
        };

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
            EstimatedDeliveryTime = doOrder.OrderDate.AddHours(1),//חישוב משוער
            MaxDeliveryTime = doOrder.OrderDate.AddHours(2),//חישוב מקסימלי
            OrderStatus = (BO.OrderStatus)doOrder.OrderStatus,//ברירת מחדל
            ScheduleStatus = BO.ScheduleStatus.LATE,//ברירת מחדל
            TimeLeftForDelivery = TimeSpan.FromHours(2),//חישוב
                                                        // DeliveryPerOrderInLists= null//למלא ברשימות


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
            Addres = boOrder.Addres,//חישוב תקינות כתובת
            Latitude = boOrder.Latitude,
            Longitude = boOrder.Longitude,
            Name = boOrder.Name,
            Phone = boOrder.Phone,//חישוב תקינות טלפון
            Weight = boOrder.Weight,
            OrderDate = boOrder.OrderDate,

        };
        s_dal.Order.Update(doOrder);
    }
    public static void Delete(int id)
    {
        s_dal.Order.Delete(id);
    }

}
