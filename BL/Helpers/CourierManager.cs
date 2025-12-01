

using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static readonly IDal s_dal = Factory.Get; //stage 4

    internal static string? Login(int id, string password)
    {
        if (id == AdminManager.GetConfig().ManagerId)
        {

            if (password == AdminManager.GetConfig().PasswordManager)
                return "Manager";
            else
                throw new BO.BlIncorrectPasswordException();
        }

        DO.Courier? doCourier = s_dal.Courier.Read(id) ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist"); ;

        if (doCourier.Password == password)
            return "Courier";
        else
            throw new BO.BlIncorrectPasswordException();

    }
    internal static void Create(BO.Courier boCourier)//יצירת שליח בדאטה בייס בסגנון ישות DO
    {
        if (!Tools.IsValidId(boCourier.Id))
            throw new BO.BlInvalidValueException(boCourier.Id);
        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");
        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.BlInvalidValueException("Invalid email address.");
        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.BlInvalidValueException("Password is not strong enough.");


        DO.Courier doCourier = new DO.Courier
        {
            Id = boCourier.Id,
            Name = boCourier.Name,
            Phone = boCourier.Phone,
            Email = boCourier.Email,
            Password = boCourier.Password,
            Active = boCourier.Active,
            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,//חישוב אווירי כלשהוא
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = boCourier.WorkingSince
        };
        try
        {
            s_dal.Courier.Create(doCourier); // שולחים ל-DAL
        }
        catch (Exception ex)
        {
            throw new BO.BlAlreadyExistsException($"courier with id {boCourier.Id} is alredy exists", ex);
        }


    }
    internal static BO.Courier? Read(int id)
    {
        DO.Courier doCourier = s_dal.Courier.Read(id)
            ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");


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
            DeliveryOnTime = s_getDeliveryOnTime(doCourier),
            DeliveryLate = s_getDeliveryLate(doCourier),
            OrderInProgress = s_getOrderInProgres(doCourier.Id)



        };
        return boCourier;//מחזירים שליח מומר
    }

    internal static void Update(int requesterId, BO.Courier boCourier)
    {
        bool manager = requesterId == AdminManager.GetConfig().ManagerId;

        // שליח קיים?
        DO.Courier courier = s_dal.Courier.Read(boCourier.Id)
            ?? throw new BO.BlDoesNotExistException(
                $"Courier with ID={boCourier.Id} does not exist, you can't update");

        // בדיקות תקינות
        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.BlInvalidValueException("Invalid phone number.");

        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.BlInvalidValueException("Invalid email address.");

        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.BlInvalidValueException("Password is not strong enough.");

        // המרה ל-DO
        DO.Courier doCourier = new DO.Courier
        {
            Id = courier.Id,
            Name = boCourier.Name,
            Phone = boCourier.Phone,
            Email = boCourier.Email,
            Password = boCourier.Password,

            // רק מנהל יכול לשנות Active
            Active = manager ? boCourier.Active : courier.Active,

            MaxDistanceDelivery = boCourier.MaxDistanceDelivery,
            TypeShipment = (DO.TheTypeShipment)boCourier.TypeShipment,
            WorkingSince = courier.WorkingSince

        };
        try
        {
            s_dal.Courier.Update(doCourier); // שולחים ל-DAL
        }
        catch (Exception ex)
        {
            throw new BO.BlDoesNotExistException($"courier with id {boCourier.Id} is not found", ex);
        }

    }


    internal static void Delete(int id)
    {
                
            // בדיקה אם השליח קיים
            DO.Courier? courier = s_dal.Courier.Read(id);
            if (courier == null)
                throw new BO.BlDoesNotExistException($"Courier with ID={id} does not exist, you can't delete");
            // בדיקה אם יש משלוחים פעילים
            IEnumerable<DO.Delivery> activeDeliveries = s_dal.Delivery.ReadAll(d =>
                d.CourierId == id &&
                (d.EndDelivery == null || d.EndDelivery != DO.EndDelivery.DELIVERED)
            );
            if (activeDeliveries.Any())
                throw new BO.BlInvalidOperationException("Cannot delete courier with active deliveries.");

            s_dal.Courier.Delete(id); // שולחים ל-DAL אני לא מפחד מחריגה מלמטה כי כבר ווידאתי שהוא קיים
       
       
    }

    internal static IEnumerable<BO.CourierInList> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        if (requesterId != AdminManager.GetConfig().ManagerId)
            throw new BO.BlNoAccessException("Only manager can access the list of couriers.");

        return s_dal.Courier.ReadAll(c => isActive == null || c.Active == isActive)
            .OrderBy(c => sort switch
            {
                BO.CourierFieldSort.Id => (IComparable)c.Id,
                BO.CourierFieldSort.Name => (IComparable)c.Name,
                BO.CourierFieldSort.Phone => (IComparable)c.Phone,
                BO.CourierFieldSort.TypeShipment => (IComparable)c.TypeShipment,
                _ => c.Id
            }).Select(c => new BO.CourierInList
            {
                Id = c.Id,
                Name = c.Name,
                Active = c.Active,
                TypeShipment = (BO.TheTypeShipment)c.TypeShipment,
                WorkingSince = c.WorkingSince,
                DeliveryOnTime = s_getDeliveryOnTime(c),
                DeliveryLate = s_getDeliveryLate(c),
                DeliveryId = s_getOrderInProgres(c.Id)?.DeliveryId
            });
    }



    //פונקציה לחישוב  משלוחים בזמן
    private static int s_getDeliveryOnTime(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&
               d.TimeEndDelivery - d.OrderDate <= AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }
    //פונקציה לחישוב משלוחים באיחור
    private static int s_getDeliveryLate(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&

               d.TimeEndDelivery - d.OrderDate > AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }
    private static BO.OrderInProgress s_createOrderInProgress(DO.Delivery delivery)
    {
        DO.Order? order = s_dal.Order.Read(delivery.OrderId)
         ?? throw new Exception("Order not found");

        var estimatedDeliveryTime = s_getEstimatedDeliveryTime(delivery); // משתנה עזר לחישוב זמן משוער
        var maxDeliveryTime = delivery.OrderDate.Add(s_dal.Config.MaxDeliveryTime);

        BO.OrderInProgress orderInProgress = new BO.OrderInProgress
        {
            DeliveryId = delivery.Id,
            OrderId = delivery.OrderId,
            TypeOfOrder = (BO.TypeOfOrder)order.TypeOfOrder,
            Details = order.Details,
            Address = order.Addres,
            Distance = Tools.GetDistance(order),
            ActualDistance = delivery.ActualDistance,
            CustomerName = order.Name,
            CustomerPhone = order.Phone,
            OrderTime = order.OrderDate,
            StartDeliveryTime = delivery.OrderDate,
            EstimatedDeliveryTime = estimatedDeliveryTime,
            MaxDeliveryTime = maxDeliveryTime,//חישוב זמן מקסימלי 
            OrderStatus = BO.OrderStatus.DELIVERING,
            ScheduleStatus = s_getScheduleStatus((BO.OrderStatus)order.OrderStatus, estimatedDeliveryTime, maxDeliveryTime),//מצב לוח זמנים to do
            TimeRemaining = (delivery.OrderDate.Add(s_dal.Config.MaxDeliveryTime) - DateTime.Now)//זמן שנותר to do
        };
        return orderInProgress;
    }
    private static BO.OrderInProgress? s_getOrderInProgres(int courierId)
    {

        // מקבל אוסף של כל המשלוחים של השליח שעדיין לא הסתיימו
        IEnumerable<DO.Delivery> allDeliveries = s_dal.Delivery.ReadAll(d =>
            d.CourierId == courierId &&
            d.EndDelivery == null &&
            s_dal.Order.Read(d.OrderId)?.OrderStatus == DO.OrderStatus.DELIVERING
        );

        if (!allDeliveries.Any())
            return null;

        return s_createOrderInProgress(allDeliveries.First());
    }


    private static BO.ScheduleStatus s_getScheduleStatus(BO.OrderStatus orderStatus, DateTime estimatedDeliveryTime, DateTime maxDeliveryTime)
    {
        TimeSpan riskRange = AdminManager.GetConfig()?.RiskRange ?? throw new Exception("Risk range not configured");
        // המשלוח וודאי עדיין בתהליך למקרה שנרצה לבדוק משלוח סגור נצטרך להוציא את זה ל TOLLS ולבדוק עוד תנאים

        var timeBuffer = maxDeliveryTime - estimatedDeliveryTime;

        if (timeBuffer > riskRange)
            return BO.ScheduleStatus.ONTYME;

        if (timeBuffer >= TimeSpan.Zero) // כלומר: בין 0 ל-riskRange
            return BO.ScheduleStatus.INRISK;

        return BO.ScheduleStatus.LATE; // הזמן המשוער הוא אחרי זמן המקסימום (שלילי)

    }
    private static DateTime s_getEstimatedDeliveryTime(DO.Delivery delivery)
    {
        DateTime estimatedDeliveryTime;
        if (delivery.ActualDistance.HasValue)
        {
            // שליפת המהירות הממוצעת לפי סוג הרכב
            DO.Courier courier = s_dal.Courier.Read(delivery.CourierId)
                ?? throw new BO.BlDoesNotExistException($"Courier with ID={delivery.CourierId} does Not exist");
            double avgSpeed = courier.TypeShipment switch
            {
                DO.TheTypeShipment.CAR => AdminManager.GetConfig().AvgSpeedCar,
                DO.TheTypeShipment.MOTORCYCLE => AdminManager.GetConfig().AvgSpeedMotorcycle,
                DO.TheTypeShipment.BIKE => AdminManager.GetConfig().AvgSpeedBike,
                DO.TheTypeShipment.FOOT => AdminManager.GetConfig().AvgSpeedFoot,
                _ => 1.0
            };

            // חישוב משך הזמן בשעות והוספה לזמן ההזמנה
            double estimatedHours = delivery.ActualDistance.Value / avgSpeed;
            estimatedDeliveryTime = delivery.OrderDate.AddHours(estimatedHours);
        }
        else
        {
        // אם אין מרחק בפועל, משתמשים בזמן המקסימלי המוגדר
        estimatedDeliveryTime = delivery.OrderDate + AdminManager.GetConfig().MaxDeliveryTime;
        }
        return estimatedDeliveryTime;

    }

}