using Dal;
using DalApi;
using DO;
using System.Data;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace DalTest
{
    /// <summary>
    /// Main program class for testing the Data Access Layer (DAL) functionality.
    /// Provides interactive console menus for CRUD operations on couriers, orders, and deliveries.
    /// </summary>
    internal class Program
    {
       

        static readonly IDal s_dal = new DalList(); //stage 2

        /// <summary>
        /// Validates whether the courier's shipment type is compatible with the order type.
        /// </summary>
        /// <param name="courierType">The type of shipment the courier uses.</param>
        /// <param name="order">The type of order to be delivered.</param>
        /// <returns>True if the courier can handle the order type; otherwise, false.</returns>
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

        /// <summary>
        /// Prompts the user for courier information and creates a new courier object.
        /// </summary>
        /// <returns>A new Courier instance with user-provided data.</returns>
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
                WorkingSince = s_dal.Config?.Clock ?? DateTime.Now
            };
        }

        /// <summary>
        /// Provides an interactive menu for updating courier properties.
        /// </summary>
        /// <param name="id">The ID of the courier to update.</param>
        private static void UpdateCourier(int id)
        {
            int choiche = 0;
            do
            {
                Courier? courierToUpdate = s_dal.Courier?.Read(id);
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
                        s_dal.Courier?.Update(courierToUpdate);

                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Phone = phone;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Email: ");
                        string email = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Email = email;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Password = password;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Active status (true/false): ");
                        bool isActive = bool.Parse(Console.ReadLine() ?? "true");
                        courierToUpdate.Active = isActive;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Max Distance Delivery (in km): ");
                        double maxDistance = double.Parse(Console.ReadLine() ?? "0");
                        courierToUpdate.MaxDistanceDelivery = maxDistance;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                        int typeShipmentInput = GetIntInput();
                        courierToUpdate.TypeShipment = (TheTypeShipment)typeShipmentInput;
                        s_dal.Courier?.Update(courierToUpdate);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }

        /// <summary>
        /// Prompts the user for order information and creates a new order object.
        /// </summary>
        /// <returns>A new Order instance with user-provided data.</returns>
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
                Id = 0,
                Name = name,
                Phone = phone,
                Addres = address,
                Details = details,
                Weight = weight,
                TypeOfOrder = typeOfOrder,
                OrderStatus = OrderStatus.OPEN,
                Latitude = 0.0,
                Longitude = 0.0,
                OrderDate = s_dal.Config?.Clock ?? DateTime.Now,
            };
        }

        /// <summary>
        /// Provides an interactive menu for updating order properties.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        private static void UpdateOrder(int id)
        {
            int choiche = 0;
            do
            {
                Order? orderToUpdate = s_dal.Order?.Read(id);
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
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Phone = phone;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Addres = address;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Details: ");
                        string details = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Details = details;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Weight: ");
                        int weight = GetIntInput();
                        orderToUpdate.Weight = weight;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Type of Order (0=STANDARD, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
                        int typeOfOrderInput = GetIntInput();
                        orderToUpdate.TypeOfOrder = (TypeOfOrder)typeOfOrderInput;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Order Status (0=OPEN, 1=IN PROGRESS, 2=DELIVERED): ");
                        int orderStatusInput = GetIntInput();
                        orderToUpdate.OrderStatus = (OrderStatus)orderStatusInput;
                        s_dal.Order?.Update(orderToUpdate);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }

        /// <summary>
        /// Provides an interactive menu for updating system configuration settings.
        /// </summary>
        private static void UpdateSetting()
        {
            if (s_dal.Config == null)
            {
                Console.WriteLine("Configuration DAL is not initialized.");
                return;
            }
            int choiche = 0;
            do
            {
                Console.WriteLine(@$"
    Updating settings...
        to exit press 0
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
                        s_dal.Config.ManagerId = id;
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Menu Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        s_dal.Config.PasswordManager = password;
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Store Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        s_dal.Config.storeAddress = address;
                    }
                    ,
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Latitude: ");
                        double latitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.Latitude = latitude;
                    }
                    ,
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Longitude: ");
                        double longitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.Longitude = longitude;
                    }
                    ,
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Range (in km): ");
                        double maxRange = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.MaxDeliveryRange = maxRange;
                    }
                    ,
                    8 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Car (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedCar = speed;
                    }
                    ,
                    9 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Motorcycle (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedMotorcycle = speed;
                    }
                    ,
                    10 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Bike (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedBike = speed;
                    }
                    ,
                    11 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Foot (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedFoot = speed;
                    }
                    ,
                    12 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Time (in minutes): ");
                        int minutes = GetIntInput();
                        s_dal.Config.MaxDeliveryTime = TimeSpan.FromMinutes(minutes);
                    }
                    ,
                    13 => () =>
                    {
                        Console.WriteLine("Enter new Risk Range (in minutes): ");
                        int minutes = GetIntInput();
                        s_dal.Config.RiskRange = TimeSpan.FromMinutes(minutes);
                    }
                    ,
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();
            }
            while (choiche != 0);
        }

        /// <summary>
        /// Provides an interactive menu for reading and displaying system configuration settings.
        /// </summary>
        private static void ReadSetting()
        {
            if (s_dal.Config == null)
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
                    1 => () => Console.WriteLine($"Manager ID: {s_dal.Config.ManagerId}"),
                    2 => () => Console.WriteLine($"Menu Password: {s_dal.Config.PasswordManager}"),
                    3 => () => Console.WriteLine($"Store Address: {s_dal.Config.storeAddress}"),
                    5 => () => Console.WriteLine($"Delivery Latitude: {s_dal.Config.Latitude}"),
                    6 => () => Console.WriteLine($"Delivery Longitude: {s_dal.Config.Longitude}"),
                    7 => () => Console.WriteLine($"Max Delivery Range (in km): {s_dal.Config.MaxDeliveryRange}"),
                    8 => () => Console.WriteLine($"Average Speed for Car (in km/h): {s_dal.Config.AvgSpeedCar}"),
                    9 => () => Console.WriteLine($"Average Speed for Motorcycle (in km/h): {s_dal.Config.AvgSpeedMotorcycle}"),
                    10 => () => Console.WriteLine($"Average Speed for Bike (in km/h): {s_dal.Config.AvgSpeedBike}"),
                    11 => () => Console.WriteLine($"Average Speed for Foot (in km/h): {s_dal.Config.AvgSpeedFoot}"),
                    12 => () => Console.WriteLine($"Max Delivery Time (in minutes): {s_dal.Config.MaxDeliveryTime.TotalMinutes}"),
                    13 => () => Console.WriteLine($"Risk Range (in minutes): {s_dal.Config.RiskRange.TotalMinutes}"),
                    14 => () => Console.WriteLine($"Max Time Inactivity (in minutes): {s_dal.Config.MaxTimeInactivity.TotalMinutes}"),
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();

            }
            while (choiche != 0);
        }

        /// <summary>
        /// Prompts the user to select an open order and a suitable courier, then creates a new delivery object.
        /// Filters orders by OPEN status and couriers by active status, shipment type compatibility, and distance capability.
        /// </summary>
        /// <returns>A new Delivery instance with user-selected order and courier.</returns>
        /// <exception cref="Exception">Thrown when no orders, couriers, or suitable matches are available.</exception>
        private static Delivery CreateDelivery()
        {
            Console.WriteLine("Creating a new delivery...");

            var list_order = s_dal?.Order?.ReadAll(o => o.OrderStatus == OrderStatus.OPEN)// קלבת ההזמנות הפתוחות בלבד, לינקיו שלב 2
             ?.ToList()
            ?? throw new DalisNotAvailable("orders");
          

            Console.WriteLine(list_order.Any()
    ? $"Available open orders: {string.Join(", ", list_order.Select(o => o.Id))}"
    : "No available open orders.");

            int orderid;
            Order? selectedOrder = null;
            do
            {
                Console.Write("Enter open Order ID: ");
                orderid = GetIntInput();
                selectedOrder = list_order.FirstOrDefault(o => o.Id == orderid);

                if (selectedOrder != null)
                {
                    Console.WriteLine($"Selected Order ID: {selectedOrder.Id}");
                    
                }
                else
                {
                    Console.WriteLine($"Order ID {orderid} not found or not open. Please try again.");
                }

               
            }
            while (selectedOrder == null);

            Console.WriteLine($"Selected order: {selectedOrder.Id}");

           

            var list_courier = s_dal?.Courier?.ReadAll(Courier => Courier.Active == true &&
            MatchTypeShipmentAndOrder(Courier.TypeShipment, selectedOrder.TypeOfOrder) &&
            Courier.MaxDistanceDelivery >= selectedOrder.DistanceKm)
                 ?.ToList()
                 ?? throw new DalisNotAvailable("Couriers");

            Console.WriteLine(list_courier.Any()
    ? $"Available open orders: {string.Join(", ", list_courier.Select(o => o.Id))}"
    : "No available open orders.");

           

            int courierId;
            Courier? selectedCourier = null;
            do
            {
                Console.Write("Enter Courier ID: ");
                courierId = GetIntInput();
                selectedCourier = list_courier.FirstOrDefault(c => c.Id == courierId);
                if (selectedCourier != null)
                {
                    Console.WriteLine($"Selected Courier ID: {selectedCourier.Id}");
                }
                else
                {
                    Console.WriteLine($"Courier ID {courierId} not found or not suitable. Please try again.");
                }

               
            }
            while (selectedCourier == null);

            Console.Write("Enter Actual Distance: ");
            double actualDistance = double.Parse(Console.ReadLine() ?? "0");

            return new Delivery
            {
                Id = 0,
                OrderId = orderid,
                CourierId = courierId,
                TypeOfOrder = selectedOrder.TypeOfOrder,
                OrderDate = s_dal.Config?.Clock ?? DateTime.Now,
                ActualDistance = actualDistance,
                TimeEndDelivery = null
            };
        }

        /// <summary>
        /// Prompts the user for integer input and validates it.
        /// Continues prompting until valid integer input is received.
        /// </summary>
        /// <returns>A valid integer entered by the user.</returns>
        public static int GetIntInput()
        {
            int result;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out result))
                {
                    return result;
                }
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }

        /// <summary>
        /// Provides a generic interactive menu for CRUD operations on any entity type.
        /// </summary>
        /// <typeparam name="T">The entity type (must be a class).</typeparam>
        /// <param name="dal">The data access layer interface for the entity type.</param>
        private static void DataMenu<T>(ICrud<T>? dal) where T : class
        {
            string typeName = typeof(T).Name.ToLower();
            Console.WriteLine($"Set {typeName} Menu.");
            int choiche;
            do
            {
                Console.WriteLine(@$"
    Set {typeName} menu.
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
                        0 => () => Console.WriteLine($"exit from set {typeof(T).Name}"),
                        1 => () =>
                        {
                            T? newItem = typeof(T).Name switch
                            {
                                nameof(Courier) => CreateCourier() as T,
                                nameof(Order) => CreateOrder() as T,
                                nameof(Delivery) => CreateDelivery() as T,
                                _ => throw new DalErrorConfig($"Unknown type: {typeof(T).Name}")
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

                            if (items == null || !items.Any())
                            {
                                Console.WriteLine($"No {typeName}s found.");
                            }
                            else
                            {
                                items.ToList().ForEach(item => Console.WriteLine(item));
                            }

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
                                    throw new DalErrorConfig($"Update not supported for type: {typeof(T).Name}");
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
                                Console.Error.WriteLine(ex);
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
                    Console.Error.WriteLine(ex);
                }
            } while (choiche != 0);
        }

        /// <summary>
        /// Provides an interactive menu for system settings management, including clock manipulation and configuration updates.
        /// </summary>
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
                        if (s_dal.Config != null)
                        {
                            s_dal.Config.Clock = s_dal.Config.Clock.AddMinutes(minutes);
                            Console.WriteLine($"System clock moved forward by {minutes} minutes.");
                        }
                    }
                    ,
                    2 => () =>
                    {
                        Console.WriteLine("Enter number of hours to move forward: ");
                        int hours = GetIntInput();
                        if (s_dal?.Config != null)
                        {
                            s_dal!.Config.Clock = s_dal!.Config.Clock.AddHours(hours);
                            Console.WriteLine($"System clock moved forward by {hours} hours.");
                        }
                    }
                    ,
                    3 => () =>
                    {
                        Console.WriteLine("Enter number of days to move forward: ");
                        int days = GetIntInput();
                        if (s_dal?.Config != null)
                        {
                            s_dal!.Config.Clock = s_dal!.Config.Clock.AddDays(days);
                            Console.WriteLine($"System clock moved forward by {days} days.");
                        }
                    }
                    ,
                    4 => () =>
                    {
                        if (s_dal.Config != null)
                        {
                            Console.WriteLine($"Current system date and time: {s_dal.Config.Clock}");
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
                    7 => () => s_dal!.Config?.Reset(),
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                }; action();

            }
            while (choiche != 0);
        }

        /// <summary>
        /// Main entry point of the application.
        /// Displays the main menu and handles user navigation between different entity management menus.
        /// </summary>
        /// <param name="args">Command line arguments (not used).</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Book Soop!");

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
                try
                {
                    switch (choice)
                    {
                        case 0:
                            Console.WriteLine("good bye");
                            break;
                        case 1:
                            DataMenu(s_dal!.Courier!);
                            break;
                        case 2:
                            DataMenu(s_dal!.Order!);
                            break;
                        case 3:
                            DataMenu(s_dal!.Delivery!);
                            break;
                        case 5:
                            Initialization.Do(s_dal); //stage 2
                            break;
                        case 6:
                            
                            var couriers = s_dal!.Courier?.ReadAll();
                            if (couriers != null)
                            {
                                foreach (var courier in couriers)
                                    Console.WriteLine(courier);
                            }
                            var orders = s_dal!.Order?.ReadAll();
                            if (orders != null)
                            {
                                foreach (var order in orders)
                                    Console.WriteLine(order);
                            }
                            var deliveries = s_dal!.Delivery?.ReadAll();
                            if (deliveries != null)
                            {
                                foreach (var delivery in deliveries)
                                    Console.WriteLine(delivery);
                            }



                            break;
                        case 7:
                            SettingMenu();
                            break;
                        case 8:
                            Console.WriteLine("Delete all data.");
                            s_dal!.Config?.Reset();
                            s_dal!.Delivery?.DeleteAll();
                            s_dal!.Order?.DeleteAll();
                            s_dal!.Courier?.DeleteAll();
                            break;

                        default:
                            Console.WriteLine("Please enter one of the following options: ");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            } while (choice != 0);


        }
    }
}
