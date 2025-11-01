

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
            Id = s_rand.Next(1, 1000),
            Name = "Test Courier",
            Vehicle = Vehicle.Bike,
            IsAvailable = true
        };
    };

}
