
using DalApi;
using DO;
using System.Text.RegularExpressions;

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
        if (!IsValidId(boCourier.Id))
            throw new BO.InvalidIdException("Invalid ID.");
        if (!IsValidPhone(boCourier.Phone))
            throw new BO.InvalidPhoneException("Invalid phone number.");
        if (!IsValidEmail(boCourier.Email))
            throw new BO.InvalidEmailException("Invalid email address.");
        if (!IsStrongPassword(boCourier.Password))
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
            DeliveryLate = GetDeliveryLate(doCourier) 


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
        if (!IsValidPhone(boCourier.Phone))
            throw new BO.InvalidPhoneException("Invalid phone number.");

        if (!IsValidEmail(boCourier.Email))
            throw new BO.InvalidEmailException("Invalid email address.");

        if (!IsStrongPassword(boCourier.Password))
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
        BO.CourierFieldSort? sort)
    {
        if (requesterId != s_dal.Config.ManagerId)
            throw new BO.UnauthorizedAccessException("Only manager can access the list of couriers.");

        IEnumerable<DO.Courier>doCouriers = s_dal.Courier.ReadAll();
        var boCouriers = doCouriers.Select(doCourier => new BO.CourierInList
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Active = doCourier.Active,
            TypeShipment = (BO.TheTypeShipment)doCourier.TypeShipment
        });
        if (isActive.HasValue)
        {
            boCouriers = boCouriers.Where(courier => courier.Active == isActive.Value);
        }
        if (sort.HasValue)
        {
            boCouriers = sort.Value switch
            {
                BO.CourierFieldSort.Id => boCouriers.OrderBy(courier => courier.Id),
                BO.CourierFieldSort.Name => boCouriers.OrderBy(courier => courier.Name),
                BO.CourierFieldSort.Phone => boCouriers.OrderBy(courier => courier.Phone),
                BO.CourierFieldSort.TypeShipment => boCouriers.OrderBy(courier => courier.TypeShipment),
                _ => boCouriers
            };
        }
        else
        {
            boCouriers = boCouriers.OrderBy(courier => courier.Id);
        }

        return boCouriers;
    }



    //פונקציה לחישוב  משלוחים בזמן
    private static int GetDeliveryOnTime(DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == EndDelivery.DELIVERED &&
               d.TimeEndDelivery != null &&
               d.TimeEndDelivery - d.OrderDate <= s_dal.Config.MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }
    //פונקציה לחישוב משלוחים באיחור
    private static int GetDeliveryLate (DO.Courier doCourier)
    {
        IEnumerable<DO.Delivery> deliveriesOnTime = s_dal.Delivery.ReadAll(d =>
               d.CourierId == doCourier.Id &&
               d.EndDelivery == EndDelivery.DELIVERED &&
               d.TimeEndDelivery != null &&
               d.TimeEndDelivery - d.OrderDate > s_dal.Config.MaxDeliveryTime
        );

        return deliveriesOnTime.Count();
    }


    //בדיקות תקינות של ערכים
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    private static bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^0\d{8,9}$") ||
        Regex.IsMatch(phone, @"^\+?[1-9]\d{1,14}$");

    }
    private static bool IsStrongPassword(string password)
    {
        if (password.Length < 8)
            return false;
        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
    private static bool IsValidId(int id)
    {
        int tempId = id;
        int sum = 0;
        tempId = tempId / 10;
        for (int i = 1; i < 9; i++)
        {
            int temp = tempId % 10;
            if (i % 2 == 0)
            {
                sum = sum + temp;
            }
            else
            {
                temp = temp * 2;
                sum = sum + (temp % 10 + temp / 10);
            }
            tempId = tempId / 10;
        }

        return (id % 10 == (10 - (sum % 10)));
    }



}