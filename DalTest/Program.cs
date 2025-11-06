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
                Console.Error.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
            static void MainMenu()
            {
                int choice;
                do
                {

                    Console.WriteLine(" Main Menu \nto exit press\nto set courier press 1\nto set order press 2\nto set delivery press 3\nenter your choice: ");

                    string? input = Console.ReadLine();


                    Console.Write("Enter your choice: ");

                    // מנסה להמיר את הקלט למספר
                    if (!int.TryParse(input, out choice))
                    {
                        Console.WriteLine("Invalid input! Please enter a number.");
                    }
                    else if (choice < 0 || choice > 3)
                    {
                        Console.WriteLine("Invalid choice! Please enter 0, 1, 2, or 3.");
                    }
                    else
                    {
                        Console.WriteLine($"You chose option {choice}");
                    }


                    // פעולה לפי הבחירה
                    switch (choice)
                    {
                        case 1:
                            SetCourier();
                            break;
                        case 2:
                            SetOrder();
                            break;
                        case 3:
                            SetDelivery();
                            break;
                        case 0:
                            Console.WriteLine("good bay");
                            break;
                        default:
                            Console.WriteLine("error");
                            break;
                    }

                } while (choice != 0);
            }

        }
    }
}
