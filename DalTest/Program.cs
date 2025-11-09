using Dal;
using DalApi;
using DO;
using System.Data;
using System.Diagnostics.Metrics;
using System.Numerics;

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

        private static Courier CreateCourier()
        {
            Console.WriteLine("Creating a new courier...");
            Console.Write("Enter ID: ");
            int id = GetIntInput();

            Console.Write("Enter Name: ");
            string name = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Phone: ");
            string phone = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Email: ");
            string email = Console.ReadLine() ?? string.Empty;

            string password = string.Empty;
            while (password == string.Empty)
            {
                Console.Write("Enter Password: ");
                password = Console.ReadLine() ?? string.Empty;
            }

            Console.Write("Enter Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
            int typeShipmentInput = GetIntInput();

            Console.Write("Enter Max Distance Delivery (in km): ");
            string maxDistanceInput = Console.ReadLine() ?? "0";
            double maxDistanceDelivery = double.Parse(maxDistanceInput);

            return new Courier
            {
                Id = id,
                Name = name,
                Phone = phone,
                Email = email,
                Password = password,
                Active = true,
                MaxDistanceDelivery = maxDistanceDelivery,
                TypeShipment = (TheTypeShipment)typeShipmentInput,
                WorkingSince = s_dalConfig?.Clock ?? DateTime.Now
            };
        }

        private static void UpdateCourier(int id)
        {
            int choiche = 0;
            do
            {
                Courier? courierToUpdate = s_dalCourier?.Read(id);
                if (courierToUpdate == null)
                {
                    Console.WriteLine($"not found courier with ID: {id}");
                    return;
                }
                Console.WriteLine($@"
    Updating courier with ID: {id}
        to exit press 0
        to set Name press 1
        to set Phone press 2
        to set Email press 3
        to set Password press 4
        to set isActive press 5
        to set max Distance press 6
        to set Type Shipment press 7
            ");
                Action action = choiche switch
                {
                    0 => () => Console.WriteLine("good bye"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Name: ");
                        string name = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Name = name;
                        s_dalCourier?.Update(courierToUpdate);

                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Phone = phone;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Email: ");
                        string email = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Email = email;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Password = password;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Active status (true/false): ");
                        bool isActive = bool.Parse(Console.ReadLine() ?? "true");
                        courierToUpdate.Active = isActive;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Max Distance Delivery (in km): ");
                        double maxDistance = double.Parse(Console.ReadLine() ?? "0");
                        courierToUpdate.MaxDistanceDelivery = maxDistance;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                        int typeShipmentInput = GetIntInput();
                        courierToUpdate.TypeShipment = (TheTypeShipment)typeShipmentInput;
                        s_dalCourier?.Update(courierToUpdate);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }
        private static Order CreateOrder()
        {
            Console.WriteLine("Creating a new order...");

            Console.Write("Enter Name: ");
            string name = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Phone: ");
            string phone = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Customer Address: ");
            string address = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter details of your order: ");
            string? details = Console.ReadLine();

            Console.Write("Enter Weight of order: ");
            int weight = GetIntInput();

            Console.Write("Enter Type Shipment (0=STANDARD, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
            var typeOfOrder = (TypeOfOrder)GetIntInput();

            return new Order
            {
                Id = 0, // ID will be set by DAL
                Name = name,
                Phone = phone,
                Addres = address,
                Details = details,
                Weight = weight,
                TypeOfOrder = typeOfOrder,
                OrderStatus = OrderStatus.OPEN,
                Latitude = 0.0, // Placeholder
                Longitude = 0.0, // Placeholder
                OrderDate = s_dalConfig?.Clock ?? DateTime.Now,
            };
        }

        private static void UpdateOrder(int id)
        {
            int choiche = 0;
            do
            {
                Order? orderToUpdate = s_dalOrder?.Read(id);
                if (orderToUpdate == null)
                {
                    Console.WriteLine($"not found order with ID: {id}");
                    return;
                }
                Console.WriteLine($@"
    Updating order with ID: {id}
        to exit press 0
        to set Name press 1
        to set Phone press 2
        to set Address press 3
        to set Details press 4
        to set Weight press 5
        to set Type of Order press 6
        to set Order Status press 7
            ");
                Action action = choiche switch
                {
                    0 => () => Console.WriteLine("good bye"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Name: ");
                        string name = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Name = name;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Phone = phone;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Addres = address;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Details: ");
                        string details = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Details = details;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Weight: ");
                        int weight = GetIntInput();
                        orderToUpdate.Weight = weight;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Type of Order (0=STANDARD, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
                        int typeOfOrderInput = GetIntInput();
                        orderToUpdate.TypeOfOrder = (TypeOfOrder)typeOfOrderInput;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Order Status (0=OPEN, 1=IN PROGRESS, 2=DELIVERED): ");
                        int orderStatusInput = GetIntInput();
                        orderToUpdate.OrderStatus = (OrderStatus)orderStatusInput;
                        s_dalOrder?.Update(orderToUpdate);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }

        private static void UpdateSetting()
        {
            if (s_dalConfig == null)
            {
                Console.WriteLine("Configuration DAL is not initialized.");
                return;
            }
            int choiche = 0;
            do
            {
                Console.WriteLine(@$"
    Updating settings...
        to set manager id press 1
        to set menu password press 2
        to set store address press 3
        to set delivery Latitude press 5
        to set delivery Longitude press 6
        to set Max Delivery Range press 7
        to set avg speed car press 8
        to set avg speed motorcycle press 9
        to set avg speed bike press 10
        to set avg speed foot press 11
        to set max delivery time press 12
        to set risk range press 13
        to set max time inactivity press 14
                ");
                choiche = GetIntInput();
                Action action = choiche switch
                {
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Manager ID: ");
                        int id = GetIntInput();
                        s_dalConfig.ManagerId = id;
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Menu Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        s_dalConfig.PasswordManager = password;
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Store Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        s_dalConfig.storeAddress = address;
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Latitude: ");
                        double latitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.Latitude = latitude;
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Longitude: ");
                        double longitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.Longitude = longitude;
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Range (in km): ");
                        double maxRange = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.MaxDeliveryRange = maxRange;
                    }
                    ,
                    8 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Car (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.AvgSpeedCar = speed;
                    }
                    ,
                    9 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Motorcycle (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.AvgSpeedMotorcycle = speed;
                    }
                    ,
                    10 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Bike (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.AvgSpeedBike = speed;
                    }
                    ,
                    11 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Foot (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dalConfig.AvgSpeedFoot = speed;
                    }
                    ,
                    12 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Time (in minutes): ");
                        int minutes = GetIntInput();
                        s_dalConfig.MaxDeliveryTime = TimeSpan.FromMinutes(minutes);
                    }
                    ,
                    13 => () =>
                    {
                        Console.WriteLine("Enter new Risk Range (in minutes): ");
                        int minutes = GetIntInput();
                        s_dalConfig.RiskRange = TimeSpan.FromMinutes(minutes);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }

        private static void ReadSetting()
        {
            if (s_dalConfig == null)
            {
                Console.WriteLine("Configuration DAL is not initialized.");
                return;
            }
            int choiche = 0;
            do
            {
                Console.WriteLine(@$"
    Reading settings...
        to exit press 0
        to get manager id press 1
        to get manager password press 2
        to get store address press 3
        to get store Latitude press 5
        to get store Longitude press 6
        to get Max Delivery Range press 7
        to get avg speed car press 8
        to get avg speed motorcycle press 9
        to get avg speed bike press 10
        to get avg speed foot press 11
        to get max delivery time press 12
        to get risk range press 13
        to get max time inactivity press 14
                ");
                choiche = GetIntInput();
                Action action = choiche switch
                {
                    1 => () => Console.WriteLine($"Manager ID: {s_dalConfig.ManagerId}"),
                    2 => () => Console.WriteLine($"Menu Password: {s_dalConfig.PasswordManager}"),
                    3 => () => Console.WriteLine($"Store Address: {s_dalConfig.storeAddress}"),
                    5 => () => Console.WriteLine($"Delivery Latitude: {s_dalConfig.Latitude}"),
                    6 => () => Console.WriteLine($"Delivery Longitude: {s_dalConfig.Longitude}"),
                    7 => () => Console.WriteLine($"Max Delivery Range (in km): {s_dalConfig.MaxDeliveryRange}"),
                    8 => () => Console.WriteLine($"Average Speed for Car (in km/h): {s_dalConfig.AvgSpeedCar}"),
                    9 => () => Console.WriteLine($"Average Speed for Motorcycle (in km/h): {s_dalConfig.AvgSpeedMotorcycle}"),
                    10 => () => Console.WriteLine($"Average Speed for Bike (in km/h): {s_dalConfig.AvgSpeedBike}"),
                    11 => () => Console.WriteLine($"Average Speed for Foot (in km/h): {s_dalConfig.AvgSpeedFoot}"),
                    12 => () => Console.WriteLine($"Max Delivery Time (in minutes): {s_dalConfig.MaxDeliveryTime.TotalMinutes}"),
                    13 => () => Console.WriteLine($"Risk Range (in minutes): {s_dalConfig.RiskRange.TotalMinutes}"),
                    14 => () => Console.WriteLine($"Max Time Inactivity (in minutes): {s_dalConfig.MaxTimeInactivity.TotalMinutes}"),
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();

            }
            while (choiche != 0);
        }
        private static Delivery CreateDelivery()
        {
            Console.WriteLine("Creating a new delivery...");

            var matchedOrders = s_dalOrder?.ReadAll() ??
                throw new Exception("No orders available to create a delivery.");

            // סינון הזמנות פתוחות
            matchedOrders = matchedOrders.Where(o => o.OrderStatus == OrderStatus.OPEN).ToList();

            if (matchedOrders.Count == 0)
            {
                throw new Exception("No open orders available.");
            }

            Console.WriteLine($"Available open orders: {string.Join(", ", matchedOrders.Select(o => o.Id))}");

            int orderid;
            Order? selectedOrder = null;
            do
            {
                Console.Write("Enter open Order ID: ");
                orderid = GetIntInput();
                selectedOrder = matchedOrders.FirstOrDefault(o => o.Id == orderid);

                if (selectedOrder == null)
                {
                    Console.WriteLine($"Order ID {orderid} not found or not open. Please try again.");
                }
            }
            while (selectedOrder == null);

            Console.WriteLine($"Selected order: {selectedOrder.Id}");

            // מסננים שליחים מתאימים
            var matchedCouriers = s_dalCourier?.ReadAll() ??
                throw new Exception("No couriers available to create a delivery.");

            matchedCouriers = matchedCouriers.Where(courier =>
                courier.Active &&
                MatchTypeShipmentAndOrder(courier.TypeShipment, selectedOrder.TypeOfOrder) &&
                courier.MaxDistanceDelivery >= selectedOrder.DistanceKm
            ).ToList();

            if (matchedCouriers.Count == 0)
            {
                throw new Exception("No suitable couriers available for this order.");
            }

            Console.WriteLine($"Available couriers: {string.Join(", ", matchedCouriers.Select(c => c.Id))}");

            int courierId;
            Courier? selectedCourier = null;
            do
            {
                Console.Write("Enter Courier ID: ");
                courierId = GetIntInput();
                selectedCourier = matchedCouriers.FirstOrDefault(c => c.Id == courierId);

                if (selectedCourier == null)
                {
                    Console.WriteLine($"Courier ID {courierId} not found or not suitable. Please try again.");
                }
            }
            while (selectedCourier == null);

            Console.Write("Enter Actual Distance: ");
            double actualDistance = double.Parse(Console.ReadLine() ?? "0");

            return new Delivery
            {
                Id = 0, // ID will be set by DAL
                OrderId = orderid,
                CourierId = courierId,
                TypeOfOrder = selectedOrder.TypeOfOrder,
                OrderDate = s_dalConfig?.Clock ?? DateTime.Now,
                ActualDistance = actualDistance,
                TimeEndDelivery = null
            };
        }
        public static int GetIntInput()
        {
            int result;
            while (true)
            {
                string? input = Console.ReadLine();
                if (int.TryParse(input, out result))
                {
                    return result;
                }
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }

        private static void DataMenu<T>(ICrud<T>? dal) where T : class
        {
            string typeName = typeof(T).Name.ToLower();
            Console.WriteLine($"Set {typeName} Menu.");
            int choiche;
            do
            {
                Console.WriteLine(@$"
Set {typeName} method called.
    to exit press 0
    to create {typeName} press 1
    to read {typeName} press 2
    to read all {typeName}s press 3
    to update press 4
    to delete {typeName} press 5
    to delete all {typeName}s press 6
        ");

                choiche = GetIntInput();

                try
                {
                    Action action = choiche switch
                    {
                        0 => () => Console.WriteLine("good bye"),
                        1 => () =>
                        {
                            T? newItem = typeof(T).Name switch
                            {
                                nameof(Courier) => CreateCourier() as T,
                                nameof(Order) => CreateOrder() as T,
                                nameof(Delivery) => CreateDelivery() as T,
                                _ => throw new InvalidOperationException($"Unknown type: {typeof(T).Name}")
                            };

                            if (newItem != null)
                            {
                                dal?.Create(newItem);
                                Console.WriteLine($"{typeName} created successfully!");
                            }
                        }
                        ,
                        2 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id: ");
                            int id = GetIntInput();
                            var result = dal?.Read(id);
                            if (result == null)
                                Console.WriteLine($"No {typeName} found with id {id}");
                            else
                                Console.WriteLine(result);
                        }
                        ,
                        3 => () =>
                        {
                            var items = dal?.ReadAll();
                            if (items?.Count == 0 || items is null)
                                Console.WriteLine($"No {typeName}s found.");
                            else
                                items.ForEach(item => Console.WriteLine(item));
                        }
                        ,
                        4 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id to update: ");
                            int id = GetIntInput();
                            switch (typeof(T).Name)
                            {
                                case nameof(Courier):
                                    UpdateCourier(id);
                                    break;
                                case nameof(Order):
                                    UpdateOrder(id);
                                    break;
                                default:
                                    throw new InvalidOperationException($"Update not supported for type: {typeof(T).Name}");
                            }
                        }
                        ,
                        5 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id: ");
                            int id = GetIntInput();
                            try
                            {
                                dal?.Delete(id);
                                Console.WriteLine($"{typeName} deleted successfully!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error deleting: {ex.Message}");
                            }
                        }
                        ,
                        6 => () =>
                        {
                            dal?.DeleteAll();
                            Console.WriteLine($"All {typeName}s deleted successfully!");
                        }
                        ,
                        _ => () => Console.WriteLine("Invalid choice, please try again.")
                    };

                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (choiche != 0);
        }

        private static void SettingMenu()
        {
            int choiche;
            do
            {
                Console.WriteLine(@$"
    settings Menu.
        to exit press 0
        move forward system clock by minute press 1
        move forward system clock by hour press 2
        move forward system clock by days press 3
        Display current system date and time press 4
        to Change values ​​in settings press 5
        to read settings value press 6
        reset all settings press 7
        ");
                choiche = GetIntInput();
                Action action = choiche switch
                {
                    0 => () => Console.WriteLine("good bye"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter number of minutes to move forward: ");
                        int minutes = GetIntInput();
                        if (s_dalConfig != null)
                        {
                            s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(minutes);
                            Console.WriteLine($"System clock moved forward by {minutes} minutes.");
                        }
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter number of hours to move forward: ");
                        int hours = GetIntInput();
                        if (s_dalConfig != null)
                        {
                            s_dalConfig.Clock = s_dalConfig.Clock.AddHours(hours);
                            Console.WriteLine($"System clock moved forward by {hours} hours.");
                        }
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter number of days to move forward: ");
                        int days = GetIntInput();
                        if (s_dalConfig != null)
                        {
                            s_dalConfig.Clock = s_dalConfig.Clock.AddDays(days);
                            Console.WriteLine($"System clock moved forward by {days} days.");
                        }
                    }
                    ,
                    4 => () =>
                    {
                        if (s_dalConfig != null)
                        {
                            Console.WriteLine($"Current system date and time: {s_dalConfig.Clock}");
                        }
                    }
                    ,
                    5 => () =>
                    {
                        UpdateSetting();
                    }
                    ,
                    6 => () =>
                    {
                        ReadSetting();
                    }
                    ,
                    7 => () => s_dalConfig?.Reset(),
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();

            }
            while (choiche != 0);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Book Soop!");
            try
            {
                int choice;
                do
                {

                    Console.WriteLine("" +
                        "Main Menu \n" +
                        "   to exit press 0\n" +
                        "   to set courier press 1\n" +
                        "   to set order press 2\n" +
                        "   to set delivery press 3\n" +
                        "   to install rendom data press 5\n" +
                        "   to print all data press 6\n" +
                        "   to edit setings press 7\n" +
                        "   to remove all data press 8\n" +
                        "       enter your choice: ");
                    choice = GetIntInput();
                    switch (choice)
                    {
                        case 0:
                            Console.WriteLine("good bye");
                            break;
                        case 1:
                            DataMenu(s_dalCourier!);
                            break;
                        case 2:
                            DataMenu(s_dalOrder!);
                            break;
                        case 3:
                            DataMenu(s_dalDelivery!);
                            break;
                        case 5:
                            Initialization.Do(s_dalCourier, s_dalOrder, s_dalDelivery, s_dalConfig);
                            break;
                        case 6:
                            s_dalCourier?.ReadAll().ForEach(courier => Console.WriteLine(courier));
                            s_dalOrder?.ReadAll().ForEach(order => Console.WriteLine(order));
                            s_dalDelivery?.ReadAll().ForEach(delivery => Console.WriteLine(delivery));
                            break;
                        case 7:
                            SettingMenu();
                            break;
                        case 8:
                            Console.WriteLine("Delete all data.");
                            s_dalConfig?.Reset();
                            s_dalDelivery?.DeleteAll();
                            s_dalOrder?.DeleteAll();
                            s_dalCourier?.DeleteAll();
                            break;

                        default:
                            Console.WriteLine("Please enter one of the following options: ");
                            break;
                    }

                } while (choice != 0);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
        }
    }
}
