

using DalApi;



namespace Helpers;

internal static class CourierManager
{
    private static readonly IDal s_dal = Factory.Get; //stage 4

    internal static string? Login(int id, string password)
    {
        if (id == s_dal.Config.ManagerId)
        {

            if (password == s_dal.Config.PasswordManager)
                return "Manager";
            else
                throw new BO.InvalidLoginException();
        }

        DO.Courier? doCourier = s_dal.Courier.Read(id) ?? throw new BO.InvalidLoginException(); ;

        if (doCourier.Password == password)
            return "Courier";
        else
            throw new BO.InvalidLoginException();

    }
    internal static void Create(BO.Courier boCourier)//יצירת שליח בדאטה בייס בסגנון ישות DO
    {
        if (!Tools.IsValidId(boCourier.Id))
            throw new BO.InvalidIdException("Invalid ID.");
        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.InvalidPhoneException("Invalid phone number.");
        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.InvalidEmailException("Invalid email address.");
        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.WeakPasswordException("Password is not strong enough.");


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
        s_dal.Courier.Create(doCourier); // שולחים ל-DAL

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
            DeliveryOnTime = GetDeliveryOnTime(doCourier),
            DeliveryLate = GetDeliveryLate(doCourier),
            OrderInProgress = GetOrderInProgres(doCourier.Id)



        };
        return boCourier;//מחזירים שליח מומר
    }
    private static BO.OrderInProgress? GetOrderInProgres(int courierId)
    {

        // מקבל אוסף של כל המשלוחים של השליח שעדיין לא הסתיימו
        IEnumerable<DO.Delivery> allDeliveries = s_dal.Delivery.ReadAll(d =>
            d.CourierId == courierId &&
            d.EndDelivery == null &&
            s_dal.Order.Read(d.OrderId)?.OrderStatus == DO.OrderStatus.DELIVERING
        );

        if (!allDeliveries.Any())
            return null;

        return CreateOrderInProgress(allDeliveries.First());
    }
    private static BO.OrderInProgress CreateOrderInProgress(DO.Delivery delivery)
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
            OrderStatus = (BO.OrderStatus)order.OrderStatus,
            ScheduleStatus = s_getScheduleStatus((BO.OrderStatus)order.OrderStatus, estimatedDeliveryTime, maxDeliveryTime),//מצב לוח זמנים to do
            TimeRemaining = (delivery.OrderDate.Add(s_dal.Config.MaxDeliveryTime) - DateTime.Now)//זמן שנותר to do
        };
        return orderInProgress;
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
                ?? throw new Exception("Courier not found");
            double avgSpeed = courier.TypeShipment switch
            {
                DO.TheTypeShipment.CAR => s_dal.Config.AvgSpeedCar,
                DO.TheTypeShipment.MOTORCYCLE => s_dal.Config.AvgSpeedMotorcycle,
                DO.TheTypeShipment.BIKE => s_dal.Config.AvgSpeedBike,
                DO.TheTypeShipment.FOOT => s_dal.Config.AvgSpeedFoot,
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

    internal static void Update(int requesterId, BO.Courier boCourier)
    {
        bool manager = requesterId == AdminManager.GetConfig().ManagerId;

        // שליח קיים?
        DO.Courier courier = s_dal.Courier.Read(boCourier.Id)
            ?? throw new BO.BlDoesNotExistException(
                $"Courier with ID={boCourier.Id} does not exist, you can't update");

        // בדיקות תקינות
        if (!Tools.IsValidPhone(boCourier.Phone))
            throw new BO.InvalidPhoneException("Invalid phone number.");

        if (!Tools.IsValidEmail(boCourier.Email))
            throw new BO.InvalidEmailException("Invalid email address.");

        if (!Tools.IsStrongPassword(boCourier.Password))
            throw new BO.WeakPasswordException("Password is not strong enough.");

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

        // עדכון ב-DAL
        s_dal.Courier.Update(doCourier);
    }


    internal static void Delete(int id)
    {
        s_dal.Courier.Delete(id); // שולחים ל-DAL
    }

    internal static IEnumerable<BO.CourierInList> ReadAll(
        int requesterId,
        bool? isActive,
        BO.CourierFieldSort? sort = BO.CourierFieldSort.Id)
    {
        if (requesterId != AdminManager.GetConfig().ManagerId)
            throw new BO.UnauthorizedAccessException("Only manager can access the list of couriers.");

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
                DeliveryOnTime = GetDeliveryOnTime(c),
                DeliveryLate = GetDeliveryLate(c),
                DeliveryId = GetOrderInProgres(c.Id)?.DeliveryId
            });
    }



    //פונקציה לחישוב  משלוחים בזמן
    private static int GetDeliveryOnTime(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&
               d.TimeEndDelivery - d.OrderDate <= AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }
    //פונקציה לחישוב משלוחים באיחור
    private static int GetDeliveryLate(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == DO.EndDelivery.DELIVERED &&

               d.TimeEndDelivery - d.OrderDate > AdminManager.GetConfig().MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }
}