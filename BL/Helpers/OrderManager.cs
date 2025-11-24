
using DalApi;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    public static void Create( BO.Order boOrder)
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
           
            //OrderStatus=(DO.OrderStatus)boOrder.OrderStatus,
            //DistanceKm=boOrder.DistanceKm,
            // DistanceKmRoad=boOrder.DistanceKmdRoad,
            //DistanceKmWalk=boOrder.DistanceKmWalk,



        };
        s_dal.Order.Create(doOrder);
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
            Distance=4,//חיושב אווירי
            EstimatedDeliveryTime=doOrder.OrderDate.AddHours(1),//חישוב משוער
            MaxDeliveryTime=doOrder.OrderDate.AddHours(2),//חישוב מקסימלי
            OrderStatus=(BO.OrderStatus)doOrder.OrderStatus,//ברירת מחדל
            ScheduleStatus=BO.ScheduleStatus.LATE,//ברירת מחדל
            TimeLeftForDelivery=TimeSpan.FromHours(2),//חישוב
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
