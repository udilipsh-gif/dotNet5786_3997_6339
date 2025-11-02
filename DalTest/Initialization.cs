
namespace DalTest;
using DalApi;
using DO;

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

        foreach (var name in courierNames)
        {
            int id;
            do
                id = s_rand.Next(MIN_ID, MAX_ID);
            while (s_dalCourier!.Read(id) != null);

            string phone = "+972" + s_rand.Next(500000000, 599999999).ToString();
            string email = name.Replace(" ", ".").ToLower() + "@courier.com";
            string password = "Pass#" + s_rand.Next(100000, 500000).ToString();
            bool active = s_rand.Next(0, 5) == 0 ? false : true;
            TheTypeShipment typeShipment = (TheTypeShipment)s_rand.Next(0, 3);

            Func<double?> getMaxDistanceDelivery = () =>
            {
                double? maxDistanceDelivery = s_dalConfig!.maxDeliveryRange != null ? s_dalConfig!.maxDeliveryRange : 100000000;
                switch (typeShipment)
                {
                    case TheTypeShipment.CAR:
                        {
                            int maxRenage = Math.Min(700, (int)maxDistanceDelivery);
                            double distance = s_rand.Next(5, maxRenage);
                            return distance > 700 ? null : distance;
                        }
                    case TheTypeShipment.MOTORCYCLE:
                        {
                            double distance = s_rand.Next(1, 50);
                            return Math.Min(distance, (double)maxDistanceDelivery);
                        }
                    case TheTypeShipment.BIKE:
                        {
                            double distance = s_rand.Next(1, 10);
                            return Math.Min(distance, (double)maxDistanceDelivery);
                        }
                    case TheTypeShipment.FOOT:
                        {
                            double distance = s_rand.NextDouble() * 5 / 2; // up to 2.5
                            return Math.Min(distance, (double)maxDistanceDelivery);
                        }
                    default:
                        return null;
                }

            };
            DateTime workingSince = s_dalConfig!.Clock.AddDays(-s_rand.Next(0, 366)); //up to 1 year ago

            s_dalCourier!.Create(new() { Id = id,
                Name = name,
                Email = email,
                Phone = phone,
                Password = password,
                Active = active,
                MaxDistanceDelivery = getMaxDistanceDelivery(),
                TheTypeShipment = typeShipment,
                WorkingSince = workingSince
            });
        }
    }

    private static void CreateOrders()
    {
        for (int i = 0; i < 200; i++)
        {
            s_dalOrder!.Create(new() { 
                Id = i,
                TypeOfOrder = (TypeOfOrder)s_rand.Next(0, 3),
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Addres = s_rand.Next(1, 200).ToString() + " Main St, City",
                Latitude = s_rand.NextDouble() * 180 - 90, // Random latitude between -90 and 90
                Longitude = s_rand.NextDouble() * 360 - 180, // Random longitude between -180 and 180
                Name = "Customer" + i,
                Weight = s_rand.Next(1, 21), // Weight between 1 and 20
                Details = "Order details for order " + i,
                OrderData = s_dalConfig!.Clock.AddDays(-s_rand.Next(0, 3)) // Orders within the last month
            });
        }
    }

    private static void CreateDelivery() 
    {
        var list_order = s_dalOrder?.ReadAll();
        var Courior_order = s_dalCourier?.ReadAll();

        Random rnd = new Random();
        int index = rnd.Next(list_order.Count);
        var randomOrder = list_order[index];//הגרלת הזמנה 
        var TypeOfOrder=randomOrder.TypeOfOrder;//שליפה של הסוג שלה
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
