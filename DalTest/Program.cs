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

        private static void SetCourier()
        {
            Console.WriteLine("SetCourier() called.");
            int choiche;
            do
            {
                Console.WriteLine(
                "to exit press 0\n" +
                "to create courier press 1\n" +
                "to read courier press 2\n" +
                "to read all couriers press 3\n" +
                "to update press 4\nto delete press 5\n" +
                "to delete courier press 5\n" +
                "to delete all couriers press 6\n");


                choiche = int.Parse(Console.ReadLine());
                switch (choiche)
                {
                    case 0:
                        break;
                    case 1:
                        Console.WriteLine("Create Courier selected.");

                        try
                        {
                            Console.Write("Enter ID: ");
                            int id = int.Parse(Console.ReadLine());

                            Console.Write("Enter Name: ");
                            string name = Console.ReadLine();

                            Console.Write("Enter Phone: ");
                            string phone = Console.ReadLine();

                            Console.Write("Enter Email: ");
                            string email = Console.ReadLine();

                            Console.Write("Enter Password: ");
                            string password = Console.ReadLine();

                            Console.Write("Enter Max Distance Delivery (in km): ");
                            double maxDistanceDelivery = double.Parse(Console.ReadLine());

                            Console.Write("Enter Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                            DO.TheTypeShipment typeShipment = (DO.TheTypeShipment)int.Parse(Console.ReadLine());


                            var newCourier = new DO.Courier
                            {
                                Id = id,
                                Name = name,
                                Phone = phone,
                                Email = email,
                                Password = password,
                                Active = true,
                                MaxDistanceDelivery = maxDistanceDelivery,
                                TypeShipment = typeShipment,
                                WorkingSince = DateTime.Now
                            };

                            s_dalCourier.Create(newCourier);

                            Console.WriteLine("Courier created successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter numbers where required.");
                        }

                        break;

                    case 2:
                        Console.WriteLine("Read Courier selected.\n enter id of courior");
                        int idRead = int.Parse(Console.ReadLine());
                        var courier = s_dalCourier.Read(idRead);
                        if (courier != null)
                        {
                            Console.WriteLine($"Courier Details:\nID: {courier.Id}\nName: {courier.Name}\nPhone: {courier.Phone}\nEmail: {courier.Email}\nActive: {courier.Active}\nMax Distance Delivery: {courier.MaxDistanceDelivery}\nType Shipment: {courier.TypeShipment}\nWorking Since: {courier.WorkingSince}");
                        }
                        else
                        {
                            Console.WriteLine("Courier not found.");
                        }

                        break;
                    case 3:
                        Console.WriteLine("Read All Couriers selected.");
                        break;
                    case 4:
                        Console.WriteLine("Update Courier selected.");
                        break;
                    case 5:
                        Console.WriteLine("Delete Courier selected.");
                        break;
                    case 6:
                        Console.WriteLine("Delete All Couriers selected.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            } while (choiche != 0);


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

                    Console.WriteLine(" Main Menu \nto exit press 0\nto set courier press 1\nto set order press 2\nto set delivery press 3\nenter your choice: ");

                    //string? input = Console.ReadLine();
                    choice = int.Parse(Console.ReadLine());

                    Console.Write("Enter your choice: ");

                    // מנסה להמיר את הקלט למספר
                    //if (!int.TryParse(input, out choice))
                    //{
                    //    Console.WriteLine("Invalid input! Please enter a number.");
                    //}
                    if (choice < 0 || choice > 3)
                    {
                        Console.WriteLine("Invalid choice! Please enter number beetwin 0-3.");
                    }
                    //else
                    //{
                    //    Console.WriteLine($"You chose option {choice}");
                    //}


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
