namespace DalTest;
using DalApi;
using DO;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// static class for initializing the data store
/// </summary>
public static class Initialization
{
    const int MIN_ID = 200000000;
    const int MAX_ID = 400000000;

    private static ICourier? s_dalCourier; //stage 1
    private static IOrder? s_dalOrder; //stage 1
    private static IDelivery? s_dalDelivery; //stage 1
    private static IConfig? s_dalConfig; //stage 1

    // Random generator
    private static readonly Random s_rand = new();

    /// <summary>
    /// Represents a collection of predefined addresses with associated geographic coordinates and a distance metric.
    /// </summary>
    /// <remarks>Each entry in the collection contains the following data: <list type="bullet"> <item>
    /// <description>A string representing the address.</description> </item> <item> <description>A latitude value as a
    /// <see cref="double"/>.</description> </item> <item> <description>A longitude value as a <see
    /// cref="double"/>.</description> </item> <item> <description>A distance metric as a <see
    /// cref="double"/>.</description> </item> </list> This data can be used for geographic calculations, such as
    /// finding nearby locations or mapping.</remarks>
    private static object[][] s_addresses =
    {

      ["הירקון 50 תל אביב", 32.09487, 34.825308, 0.45, 0.5625, 0.7425],
      ["שד' התמרים 12 רמת גן", 32.088052, 34.820129, 0.49, 0.6125, 0.8085],
      ["המסגר 20 רמת גן", 32.086789, 34.819234, 0.65, 0.8125, 1.0725],
      ["ראשון לציון 35 רמת גן", 32.085432, 34.818901, 0.8, 1.0, 1.32],
      ["שד' בן גוריון 55 רמת גן", 32.084567, 34.820123, 0.86, 1.075, 1.419],
      ["הירקון 75 תל אביב", 32.098123, 34.828765, 0.93, 1.1625, 1.5345],
      ["הרצל 40 רמת גן", 32.083456, 34.819012, 1.01, 1.2625, 1.6665],
      ["השלום 50 רמת גן", 32.082345, 34.819876, 1.11, 1.3875, 1.8315],
      ["שד' יצחק רבין 25 רמת גן", 32.082345, 34.818345, 1.14, 1.425, 1.881],
      ["הרצל 15 רמת גן", 32.0810786, 34.8180973, 1.28, 1.6, 2.112],
      ["שד' בן גוריון 40 רמת גן", 32.080456, 34.821233, 1.31, 1.6375, 2.1615],
      ["ראשון לציון 50 בני ברק", 32.080234, 34.824345, 1.35, 1.6875, 2.2275],
      ["שד' ההסתדרות 14 בני ברק", 32.080123, 34.825678, 1.39, 1.7375, 2.2935],
      ["שד' רוקח 5 רמת גן", 32.079876, 34.817654, 1.42, 1.775, 2.343],
      ["שד' גולדה מאיר 50 תל אביב", 32.101234, 34.832345, 1.42, 1.775, 2.343],
      ["העצמאות 45 בני ברק", 32.078901, 34.823456, 1.49, 1.8625, 2.4585],
      ["שד' התמרים 20 בני ברק", 32.079012, 34.824567, 1.49, 1.8625, 2.4585],
      ["הבנים 20 בני ברק", 32.077654, 34.823789, 1.63, 2.0375, 2.6895],
      ["שד' בן צבי 18 בני ברק", 32.076543, 34.821234, 1.74, 2.175, 2.871],
      ["השלום 80 בני ברק", 32.075123, 34.825678, 1.93, 2.4125, 3.1845],
      ["שד' ירושלים 12 בני ברק", 32.07491, 34.824194, 1.94, 2.425, 3.201],
      ["שד' ירושלים 45 רמת גן", 32.073393, 34.822611, 2.09, 2.6125, 3.4485],
      ["הירקון 85 בני ברק", 32.073456, 34.823901, 2.09, 2.6125, 3.4485],
      ["הירקון 100 תל אביב", 32.109456, 34.831234, 2.12, 2.65, 3.498],
      ["הירקון 30 בני ברק", 32.071234, 34.822345, 2.33, 2.9125, 3.8445],
      ["ויצמן 5 גבעתיים", 32.074703, 34.807631, 2.36, 2.95, 3.894],
      ["הבנים 9 רמת גן", 32.065432, 34.812345, 3.1, 3.875, 5.115],
      ["ויצמן 22 תל אביב", 32.08197, 34.787879, 3.39, 4.2375, 5.5935],
      ["שד' יצחק רבין 7 פתח תקווה", 32.076664, 34.859017, 3.91, 4.8875, 6.4515],
      ["רמת חן 30 רמת גן", 32.054005, 34.815657, 4.29, 5.3625, 7.0785],
      ["סוקלוב 30 תל אביב", 32.087445, 34.776397, 4.3, 5.375, 7.095],
      ["ויצמן 15 תל אביב", 32.075678, 34.77789, 4.52, 5.65, 7.458],
      ["שד' ירושלים 30 תל אביב", 32.072345, 34.779012, 4.59, 5.7375, 7.5735],
      ["הבנים 35 תל אביב", 32.073567, 34.778123, 4.6, 5.75, 7.59],
      ["שד' התמרים 35 תל אביב", 32.069234, 34.780123, 4.68, 5.85, 7.722],
      ["הרצל 25 תל אביב", 32.068901, 34.779345, 4.76, 5.95, 7.854],
      ["השלום 18 תל אביב", 32.070789, 34.777345, 4.81, 6.0125, 7.9365],
      ["ריינס 5 תל אביב", 32.077475, 34.773417, 4.84, 6.05, 7.986],
      ["המסגר 8 תל אביב", 32.071567, 34.776234, 4.86, 6.075, 8.019],
      ["שד' יצחק רבין 10 תל אביב", 32.067345, 34.778901, 4.89, 6.1125, 8.0685],
      ["הרצל 15 תל אביב", 32.06682, 34.777819, 5.01, 6.2625, 8.2665],
      ["ראשון לציון 20 פתח תקווה", 32.104233, 34.874897, 5.18, 6.475, 8.547],
      ["העצמאות 60 פתח תקווה", 32.073523, 34.872251, 5.19, 6.4875, 8.5635],
      ["העצמאות 22 תל אביב", 32.065789, 34.775432, 5.26, 6.575, 8.679],
      ["בני ברק 8 תל אביב-יפו", 32.057181, 34.778104, 5.66, 7.075, 9.339],
      ["ויצמן 40 פתח תקווה", 32.090123, 34.885678, 6.03, 7.5375, 9.9495],
      ["שד' גולדה מאיר 25 פתח תקווה", 32.098765, 34.885432, 6.04, 7.55, 9.966],
      ["בני ברק 15 פתח תקווה", 32.092345, 34.887654, 6.21, 7.7625, 10.2465],
      ["המסגר 12 פתח תקווה", 32.085678, 34.890123, 6.48, 8.1, 10.692],
      ["השלום 25 פתח תקווה", 32.088765, 34.892345, 6.66, 8.325, 10.989],
      ["השלום 60 פתח תקווה", 32.092456, 34.893456, 6.76, 8.45, 11.154],
      ["שד' ההסתדרות 30 פתח תקווה", 32.095678, 34.895678, 6.98, 8.725, 11.517],
      ["הרצל 55 פתח תקווה", 32.094123, 34.896789, 7.07, 8.8375, 11.6655],
      ["שד' רוקח 10 פתח תקווה", 32.089923, 34.899951, 7.37, 9.2125, 12.1605]


    };

    //בדיקה האם סוג ההזמנה מתאים לסוג השליח
    public static bool MatchTypeShipmentAndOrder(TheTypeShipment courier, TypeOfOrder order)
    {
        return order switch
        {
            TypeOfOrder.STANDART => courier == TheTypeShipment.CAR || courier == TheTypeShipment.MOTORCYCLE,
            TypeOfOrder.FAST_DELIVERY => courier == TheTypeShipment.MOTORCYCLE,
            TypeOfOrder.DELIVER_IMMEDIATELY => courier == TheTypeShipment.FOOT,
            _ => false
        };
    }

    /// <summary>
    /// Initializes the configuration settings for the system with default values.
    /// </summary>
    /// <remarks>This method sets up the initial configuration for the system, including the starting clock
    /// time,  manager credentials, store location, delivery parameters, and other operational settings.  It is
    /// intended to be called during the system's initialization phase.</remarks>
    private static void CreateConfig()//אתחול ראשוני של הקונפיג
    {


        s_dalConfig!.Clock = DateTime.Now;//התחלת פעילות המערכת תחילת 24
        s_dalConfig.ManagerId = 203383997;
        s_dalConfig.PasswordManager = "Admin1234$";
        s_dalConfig.storeAddress = "bar cochva, 21, Bney Braq";//כתובת המכללה
        s_dalConfig.Latitude = 32.0936195;
        s_dalConfig.Longitude = 34.8229463;
        s_dalConfig.MaxDeliveryRange = 50.0; // in km
        s_dalConfig.AvgSpeedCar = 60.0; // in km/h
        s_dalConfig.AvgSpeedMotorcycle = 40.0; // in km/h
        s_dalConfig.AvgSpeedBike = 15.0; // in km/h
        s_dalConfig.AvgSpeedFoot = 5.0; // in km/h
        s_dalConfig.MaxDeliveryTime = TimeSpan.FromDays(5);//זמן אספקה מירבי
        s_dalConfig.RiskRange = TimeSpan.FromDays(4);
        s_dalConfig.MaxTimeInactivity = TimeSpan.FromDays(14);
    }

    private static void CreateCourier()
    {
        string[] courierNames =
        {
                "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof",
                "Nadav Katz", "Rina Cohen", "Moshe Bar", "Rachel Adler", "Itay Mizrahi", "Noa Ben-David",
                "Yaniv Shapiro", "Maya Rosen", "Omer Azulay", "Lior Kaplan", "Tamar Weiss", "Ariel Gold",
                "Galit Peretz", "Eden Harari"
            };

        static int getUniqueId()
        {
            int id;
            do
                id = s_rand.Next(MIN_ID, MAX_ID);
            while (s_dalCourier!.Read(id) != null);
            return id;
        }

        static double? getMaxDistanceDelivery(TheTypeShipment shipment)
        {

            double? distans = shipment switch
            {
                TheTypeShipment.CAR => s_rand.Next(10, 100), // 10 to 100 km
                TheTypeShipment.MOTORCYCLE => s_rand.Next(2, 25), // 2 to 25 km
                TheTypeShipment.BIKE => s_rand.Next(1, 5), // 1 to 5 km
                TheTypeShipment.FOOT => s_rand.NextDouble() * 2, // up to 2 km
                _ => null
            };
            return distans > 100 ? null : distans;
        }
        ;

        var typeShipment = (TheTypeShipment)s_rand.Next(0, 4); // 0..3 עבור 4 ערכים

        foreach (var name in courierNames)
        {
            s_dalCourier!.Create(new()
            {
                Id = getUniqueId(),
                Name = name,
                Email = name.Replace(" ", ".").ToLower() + "@courier.com",
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Password = "Pass#" + s_rand.Next(100000, 500000).ToString(),
                Active = s_rand.Next(0, 5) != 0 ? true : false,
                TypeShipment = typeShipment,
                WorkingSince = s_dalConfig!.Clock.AddDays(-s_rand.Next(5, 366)), //up to 1 year ago
                MaxDistanceDelivery = getMaxDistanceDelivery(typeShipment)

            });
        }
    }

    /// <summary>
    /// Creates and initializes a collection of 50 orders with randomized data.
    /// </summary>
    /// <remarks>This method generates 50 orders with randomized properties such as order type, phone number, 
    /// address, geographic coordinates, customer name, weight, and order details. The orders are  created using the
    /// <see cref="s_dalOrder"/> data access layer and are assigned timestamps  within the last three days.</remarks>
    private static void CreateOrders()
    {
        int num_of_order = 0;
        static OrderStatus getRandomOrderStatus(int num_of_order)
        {

            return num_of_order++ switch
            {
                < 20 => OrderStatus.OPEN,
                < 30 => OrderStatus.DELIVERING,
                _ => (OrderStatus)s_rand.Next(2, 5)
            };

        }

        for (int i = 0; i < 50; i++)
        {
            var adressIndex = s_rand.Next(s_addresses.Length);
            s_dalOrder!.Create(new()
            {
                Id = 0,
                TypeOfOrder = (TypeOfOrder)s_rand.Next(0, 2),
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Addres = (string)s_addresses[adressIndex][0],
                Latitude = (double)s_addresses[adressIndex][2],
                Longitude = (double)s_addresses[adressIndex][2],
                Name = "Customer" + i,
                Weight = s_rand.Next(1, 21), // Weight between 1 and 20
                Details = "Order details for order " + i,
                OrderData = s_dalConfig!.Clock.AddDays(-s_rand.Next(0, 366)), // זמן פתיחת הזמנה רנדומלי
                DistanceKm = (double)s_addresses[adressIndex][3],
                DistanceKmWalk = (double)s_addresses[adressIndex][4],
                DistanceKmRoad = (double)s_addresses[adressIndex][5],
                OrderStatus = getRandomOrderStatus(num_of_order),
            });
        }
    }

    /// <summary>
    /// Creates and assigns a batch of delivery records based on available orders and couriers.
    /// </summary>
    /// <remarks>This method generates 50 delivery records by randomly selecting orders and matching them with
    /// couriers.  Orders that are already in the "Open" status are excluded from selection. The method ensures that the
    /// courier's shipment type matches the order's requirements before assigning the delivery.  The deliveries are
    /// initialized with default values, including a pending status and a random assignment  time within the last 72
    /// hours.</remarks>
    /// <exception cref="Exception">Thrown if no orders or no couriers are available.</exception>
    private static void CreateDelivery()
    {
        //פונקציה שבודקת אם סוג ההזמנה מתאים לסוג השליח 
        static bool MatchTypeShipmentAndOrder(TheTypeShipment courierType, TypeOfOrder order)
        {
            return order switch
            {
                TypeOfOrder.STANDART => true,
                TypeOfOrder.FAST_DELIVERY => courierType == TheTypeShipment.MOTORCYCLE || courierType == TheTypeShipment.CAR,
                TypeOfOrder.DELIVER_IMMEDIATELY => courierType == TheTypeShipment.MOTORCYCLE,
                _ => false
            };
        }
        //פונקציה לחישוב זמן סיום המשלוח בהתאם לסוג הסיום
        static DateTime? getTimeEndDelivery(DateTime orderDate, TimeSpan duration, EndDelivery endDelivery)
        {
            return endDelivery switch
            {
                EndDelivery.DELIVERED => orderDate.Add(duration),
                EndDelivery.REFUSED => orderDate.Add(duration).AddMinutes(s_rand.Next(5, 31)), // 5 to 30 minutes after expected delivery time
                EndDelivery.CONCELLED => null, // No end time for cancelled deliveries
                EndDelivery.NOTFOUND => orderDate.Add(duration).AddMinutes(s_rand.Next(10, 61)), // 10 to 60 minutes after expected delivery time
                EndDelivery.FAILED => orderDate.Add(duration).AddMinutes(s_rand.Next(15, 91)), // 15 to 90 minutes after expected delivery time
                _ => null,
            };
        }

        var list_order = s_dalOrder?.ReadAll() ?? //רשימת ההזמנות
            throw new Exception("No orders available");
        foreach (var order in list_order.ToList()) // הסרת הזמנות שלא במצב פתוח
        {
            if (order.OrderStatus != OrderStatus.OPEN)
                list_order.Remove(order);
        }

        for (int i = 0; i < 50; i++) //יצירת 50 משלוחים
        {
            var randomOrder = list_order[s_rand.Next(list_order.Count)];//משיכת הזמנה אקראית

            var matchedCouriers = s_dalCourier?.ReadAll() ??//רשימת השליחים
                    throw new Exception("No couriers available");

            foreach (var courier in matchedCouriers.ToList()) //בדיקת התאמה בין סוג ההזמנה לסוג השליח
            {
                if (courier.Active == false)//אם השליח לא פעיל הסרתו מהרשימה
                    matchedCouriers.Remove(courier);

                if (!MatchTypeShipmentAndOrder(courier.TypeShipment, randomOrder.TypeOfOrder))
                    matchedCouriers.Remove(courier);

                if (courier.MaxDistanceDelivery < randomOrder.DistanceKm)
                    matchedCouriers.Remove(courier);
            }
            if (matchedCouriers.Count == 0)
                throw new Exception("No matched couriers available for the order");

            var selectedCourier = matchedCouriers[s_rand.Next(matchedCouriers.Count)]; //הגרלת שליח מתאים
            //randomOrder = randomOrder with { OrderStatus = OrderStatus.DELIVERING };//עדכון סטטוס ההזמנה
            //s_dalOrder.Update(randomOrder);//עדכון ההזמנה במסד הנתונים
            //list_order.Remove(randomOrder); //הסרת ההזמנה מהרשימה כדי לא ליצור לה שוב משלוח

            double? getActualDistance = //שמירת המרחק האמיתי בהתאם לסוג השליח
                (selectedCourier.TypeShipment is TheTypeShipment.CAR or TheTypeShipment.MOTORCYCLE)//אם השליח הוא ברכב או אופנוע
                ? randomOrder.DistanceKmRoad
                : randomOrder.DistanceKmWalk;

            TimeSpan duration = getActualDistance.HasValue//חישוב משך זמן המשלוח בהתאם לסוג השליח
                ? TimeSpan.FromHours(getActualDistance.Value /
                    (selectedCourier.TypeShipment switch
                    {
                        TheTypeShipment.CAR => s_dalConfig!.AvgSpeedCar,
                        TheTypeShipment.MOTORCYCLE => s_dalConfig!.AvgSpeedMotorcycle,
                        TheTypeShipment.BIKE => s_dalConfig!.AvgSpeedBike,
                        TheTypeShipment.FOOT => s_dalConfig!.AvgSpeedFoot,
                        _ => 1.0
                    }))
                : TimeSpan.FromHours(1); // ברירת מחדל של שעה אם המרחק לא ידוע

            DateTime orderData = s_dalConfig!.Clock.AddHours(-s_rand.Next(0, duration.Hours)); // בתוך 3 הימים האחרונים

            EndDelivery getEndDelivery = (EndDelivery)s_rand.Next(0, 4);//הגרלת סוג סיום המשלוח
            
            if (getEndDelivery==EndDelivery.DELIVERED||getEndDelivery==EndDelivery.REFUSED||getEndDelivery==EndDelivery.CONCELLED)
                s_dalOrder.Delete(randomOrder.Id);//מחיקת ההזמנה אם המשלוח הסתיים בהצלחה או בסירוב או בביטול


            //יצירת משלוח חדש

            s_dalDelivery!.Create(new()
            {
                Id = 0,
                OrderId = randomOrder.Id,
                TypeOfOrder = randomOrder.TypeOfOrder,
                ActualDistance = getActualDistance,//עדכון צערך*************
                CourierId = selectedCourier.Id,
                OrderData = orderData,
                EndDelivery = getEndDelivery,
                TimeEndDelivery= getTimeEndDelivery(orderData, duration, getEndDelivery) ?? default

            });
        }


    }
}
