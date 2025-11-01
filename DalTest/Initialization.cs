

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

    private static void createCourier()
        
    {

        string[] courierNames =
            { "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof" };

        foreach (var name in courierNames)
        {
            int id;
            do
                id = s_rand.Next(MIN_ID, MAX_ID);
            while (s_dalCourier!.Read(id) != null);

            string phone = "+972" + s_rand.Next(500000000, 599999999).ToString();
            string email = name.Replace(" ", ".").ToLower() + "@courier.com";
            string password = "Pass#" + s_rand.Next(100000, 500000).ToString();
            bool active = true; //s_rand.Next(0, 2) == 0 ? false : true;

            double maxDistanceDelivery = s_rand.Next(0, 51); //5-50 km
            TheTypeShipment theTypeShipment = (TheTypeShipment)s_rand.Next(0, 4);
            DataTime workingSince=DateTime.Now;

            //bool? even = (id % 2) == 0 ? true : false;
            //string? alias = even ? name + "ALIAS" : null;
            //DateTime start = new DateTime(1995, 1, 1);
           // DateTime bdt = start.AddDays(s_rand.Next((s_dalConfig.Clock - start).Days));

            s_dalCourier!.Create(new(id, name, phone, email, password, active, maxDistanceDelivery, theTypeShipment, workingSince ));
        }
    }


}
