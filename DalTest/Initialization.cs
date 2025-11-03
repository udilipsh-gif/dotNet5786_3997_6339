namespace DalTest;
using DalApi;
using DO;
using System.Runtime.CompilerServices;

public static class Initialization
{
    const int MIN_ID = 200000000;
    const int MAX_ID = 400000000;

    private static ICourier? s_dalCourier; //stage 1
    private static IOrder? s_dalOrder; //stage 1
    private static IDelivery? s_dalDelivery; //stage 1
    private static IConfig? s_dalConfig; //stage 1

    private static readonly Random s_rand = new();

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

            double? distens = shipment switch
            {
                TheTypeShipment.CAR => s_rand.Next(50, 701), // 50 to 700 km
                TheTypeShipment.MOTORCYCLE => s_rand.Next(20, 101), // 20 to 100 km
                TheTypeShipment.BIKE => s_rand.Next(5, 51), // 5 to 50 km
                TheTypeShipment.FOOT => s_rand.NextDouble() * 5, // up to 5 km
                _ => null
            };
            return distens > 500 ? null : distens;
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
                WorkingSince = s_dalConfig!.Clock.AddDays(-s_rand.Next(0, 366)), //up to 1 year ago
                MaxDistanceDelivery = getMaxDistanceDelivery(typeShipment)

            });
        }
    }

    private static void CreateOrders()
    {

    // רדיוס כדור הארץ בקילומטרים
    const double EarthRadiusKm = 6371.0;

    // פונקציות עזר להמרת מעלות לרדיאנים
    static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    // פונקציות עזר להמרת רדיאנים למעלות
    static double ToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    (double, double, double) getRandomLocation()
        {
            var rand = new Random();

            double baseLatRad = ToRadians(32.092194);
            double baseLonRad = ToRadians(34.821748);

            // 1. בחירת מרחק אקראי בין 0.5 ל-100 ק"מ
            double randomDistanceKm = rand.NextDouble() * (100.0 - 0.5) + 0.5;

            // 2. בחירת כיוון (Bearing) אקראי ב-360 מעלות
            double randomBearingRad = ToRadians(rand.NextDouble() * 360.0);

            // 3. חישוב הנקודה החדשה (נוסחת יעד גיאוגרפית)
            double angularDistance = randomDistanceKm / EarthRadiusKm;

            double newLatRad = Math.Asin(
                Math.Sin(baseLatRad) * Math.Cos(angularDistance) +
                Math.Cos(baseLatRad) * Math.Sin(angularDistance) * Math.Cos(randomBearingRad)
            );

            double newLonRad = baseLonRad + Math.Atan2(
                Math.Sin(randomBearingRad) * Math.Sin(angularDistance) * Math.Cos(baseLatRad),
                Math.Cos(angularDistance) - Math.Sin(baseLatRad) * Math.Sin(newLatRad)
            );

            // 4. המרה חזרה למעלות והוספה לרשימה
            double newLatDeg = ToDegrees(newLatRad);
            double newLonDeg = ToDegrees(newLonRad);

             return (newLatDeg, newLonDeg, randomDistanceKm);
        }

        for (int i = 0; i < 200; i++)
        {   
            var (latitude, longitude, distance) = getRandomLocation();
            s_dalOrder!.Create(new()
            {
                Id = i,
                TypeOfOrder = (TypeOfOrder)s_rand.Next(0, 3),
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Addres = s_rand.Next(1, 200).ToString() + " Main St, City",
                Latitude = longitude, // Random latitude between -90 and 90
                Longitude = longitude, // Random longitude between -180 and 180
                Name = "Customer" + i,
                Weight = s_rand.Next(1, 21), // Weight between 1 and 20
                Details = "Order details for order " + i,
                OrderData = s_dalConfig!.Clock.AddDays(-s_rand.Next(0, 3)) // Orders within the last month
            });
        }
    }

    private static void CreateDelivery() 
    {
        var list_order = s_dalOrder?.ReadAll() ?? 
            throw new Exception("No orders available");
        var Courior_order = s_dalCourier?.ReadAll() ??
            throw new Exception("No couriers available");

        Random rnd = new Random();

        for (int i = 0; i < 20; i++) //יצירת 20 משלוחים
        {
            int index = 0;
            do
                index = rnd.Next(list_order.Count);
            while (list_order[index].OrderStatus == OrderStatus.OPEN);//בודק שההזמנה לא סופקה כבר
            list_order[index].OrderStatus = OrderStatus.DELIVERING;//עדכון סטטוס ההזמנה לסופקה
            var randomOrder = list_order[index];//הגרלת הזמנה 


            var TypeOfOrder = randomOrder.TypeOfOrder;//שליפה של הסוג שלה
                                                      //צריך כאן לשלוח לפונקציה שתחשב מרחק -
                                                      //אם זה ברגל או באוטו וכו, רגל או אוטו וכו'
                                                      //נקבע על פי סןג השילוח, כרגע נשים נול



            s_dalDelivery!.Create(new()
            {
                Id = 0,
                OrderId = randomOrder.Id,
                TypeOfOrder = randomOrder.TypeOfOrder,
                ActualDistance = null,

                CourierId = s_rand.Next(MIN_ID, MAX_ID),
                AssignedTime = s_dalConfig!.Clock.AddHours(-s_rand.Next(0, 72)), // within last 3 days
                PickupTime = null,
                DeliveryTime = null,
                Status = DeliveryStatus.Pending
            });
        }

        
    }
}
