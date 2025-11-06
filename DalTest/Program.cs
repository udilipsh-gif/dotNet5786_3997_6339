using Dal;
using DalApi;

namespace DalTest
{
    internal class Program
    {
        private static ICourier? s_dalCourier;
        private static IOrder? s_dalOrder;
        private static IDelivery? s_dalDelivery;
        private static IConfig? s_dalConfig;

        // static ctor runs once before any static member access / before Main
        static Program()
        {
            try
            {
                s_dalCourier = new CourierImplementation();
                s_dalOrder = new OrderImplementation();
                s_dalDelivery = new DeliveryImplementation();
                s_dalConfig = new ConfigImplementation();
            }
            catch (Exception ex)
            {
                // Log and rethrow (or set a fallback / mark failure)
                Console.Error.WriteLine($"DAL initialization failed: {ex}");
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            try
            {
                Initialization.Do(s_dalCourier, s_dalOrder, s_dalDelivery, s_dalConfig);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
            }

        }
    }
}
