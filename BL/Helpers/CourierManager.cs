
using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static readonly IDal s_dal = Factory.Get; //stage 4

    public static void Create(BO.Courier boCourier)//יצירת שליח בדאטה בייס בסגנון ישות DO
    {
        DO.Courier doCourier = new DO.Courier
        {
            Id = boCourier.Id,//בדיקת תקינות תז
            Name = boCourier.Name,
            Phone = boCourier.Phone,//בדיקת תקינות טלפון
            Email = boCourier.Email,//בדיקת תקינות אימייל
            Password = boCourier.Password,//בדיקת תקינות סיסמה חזקה וכו
            Active = boCourier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,//חישוב אווירי כלשהוא
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = boCourier.WorkingSince
        };
        s_dal.Courier.Create(doCourier); // שולחים ל-DAL

    }
    public static BO.Courier? Read(int id)
    {
        var doCourier = s_dal.Courier.Read(id);
        if (doCourier is null)
            return null;
        BO.Courier boCourier = new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
            Active = doCourier.Active,
            MaxDistanceDelivery = doCourier.MaxDistanceDelivery,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment,
            WorkingSince = doCourier.WorkingSince,
            DeliveryOnTime = 0, // יש למלא בהתאם ללוגיקה העסקית
            DeliveryLate = 0  // יש למלא בהתאם ללוגיקה העסקית


        };
        return boCourier;//מחזירים שליח מומר
    }
    public static IEnumerable<BO.CourierInList> ReadAll(
        BO.CourierFieldSort? sort = null,
        BO.CourierFieldFilter? filter = null,
        object? value = null)
    {
        var doCouriers = s_dal.Courier.ReadAll();
        // Mapping DO.Courier to BO.CourierInList
        var boCouriers = doCouriers.Select(doCourier => new BO.CourierInList
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Active = doCourier.Active,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment
        });
        // Apply filtering and sorting here based on 'filter', 'value', and 'sort' parameters
        return boCouriers;
    }
    public static void Update(BO.Courier boCourier)
    {
        DO.Courier doCourier = new DO.Courier
        {
            Id = boCourier.Id,//בדיקת תקינות תז
            Name = boCourier.Name,
            Phone = boCourier.Phone,//בדיקת תקינות טלפון
            Email = boCourier.Email,//בדיקת תקינות אימייל
            Password = boCourier.Password,//בדיקת תקינות סיסמה חזקה וכו
            Active = boCourier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = boCourier.WorkingSince
        };
        s_dal.Courier.Update(doCourier); // שולחים ל-DAL
    }
    public static void Delete(int id)
    {
        s_dal.Courier.Delete(id); // שולחים ל-DAL
    }




}