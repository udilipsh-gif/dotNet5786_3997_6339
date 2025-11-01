

namespace DalTest;
using DalApi;
using DO;

public static class Initialization
{
    private static ICourier? s_dalStudent; //stage 1
    private static IOrder? s_dalCourse; //stage 1
    private static IDelivery? s_dalLink; //stage 1
    private static IConfig? s_dalConfig; //stage 1

    private static readonly Random s_rand = new();

    private static createCourier = () =>
    {
        return new Courier
        {
            Id = s_rand.Next(1, 1000),
            Name = "Test Courier",
            Vehicle = Vehicle.Bike,
            IsAvailable = true
        };
    };

}
