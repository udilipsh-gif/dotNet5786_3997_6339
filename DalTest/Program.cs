using Dal;
using DalApi;
using DO;
using System.Data;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection;

namespace DalTest
{
    /// <summary>
    /// Main program class for testing the Data Access Layer (DAL) functionality.
    /// Provides interactive console menus for CRUD operations on couriers, orders, and deliveries.
    /// </summary>
    /// <remarks>
    /// This test program serves as a comprehensive testing interface for the delivery management system.
    /// It allows testing of all CRUD operations, system configuration management, and clock simulation
    /// for time-based delivery scenarios.
    /// </remarks>
    internal class Program
    {
        /// <summary>
        /// Static instance of the Data Access Layer interface used throughout the program.
        /// Initialized with <see cref="DalList"/> implementation.
        /// </summary>
        static readonly IDal s_dal = new DalList();

        /// <summary>
        /// Generic method to retrieve an item from a list by prompting the user for an ID.
        /// Filters the list based on the provided condition and continues prompting until a valid item is selected.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="list">The list of items to select from.</param>
        /// <param name="condition">A predicate to filter available items.</param>
        /// <param name="idSelector">A function to extract the ID from each item.</param>
        /// <returns>The selected item that matches the user-entered ID.</returns>
        /// <exception cref="DalisNotAvailable">Thrown when no items are available that match the condition.</exception>
        /// <remarks>
        /// This method displays available item IDs, prompts the user to enter an ID,
        /// and validates that the selected item exists and meets the specified condition.
        /// </remarks>
        static T getItemFromListById<T>(List<T> list, Predicate<T> condition, Func<T, int> idSelector)
        {
            int itemId;
            var availableItems = list.Where(item => condition(item));
            if (availableItems.Any() && typeof(T).GetProperties().Any(p => p.Name == "Id"))
            {
                T? selectedItem = default(T);
                Console.WriteLine(availableItems.Any()
                        ? $"Available items: {string.Join(", ", availableItems.Select(item => idSelector(item)))}"
                        : "No available items.");
                while (true)
                {
                    Console.Write($"Enter {typeof(T).Name} ID: ");
                    itemId = GetIntInput();
                    selectedItem = list.FirstOrDefault(c => idSelector(c) == itemId);
                    if (selectedItem != null)
                    {
                        Console.WriteLine($"Selected {typeof(T).Name} ID: {idSelector(selectedItem)}");
                        return selectedItem;
                    }
                    else
                    {
                        Console.WriteLine($"{typeof(T).Name} ID {itemId} not found or not suitable. Please try again.");
                    }
                }
            }
            else
                 throw new DalisNotAvailable($"{typeof(T).Name}s");
        }

        /// <summary>
        /// Validates whether the courier's shipment type is compatible with the order type.
        /// </summary>
        /// <param name="courierType">The type of shipment the courier uses.</param>
        /// <param name="order">The type of order to be delivered.</param>
        /// <returns>True if the courier can handle the order type; otherwise, false.</returns>
        /// <remarks>
        /// Compatibility rules:
        /// - STANDART orders: All courier types can deliver
        /// - FAST_DELIVERY orders: Only MOTORCYCLE or CAR couriers
        /// - DELIVER_IMMEDIATELY orders: Only MOTORCYCLE couriers
        /// </remarks>
        static bool matchTypeShipmentAndOrder(TheTypeShipment courierType, TypeOfOrder order)
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
        /// <remarks>
        /// This method collects all required courier information from the console:
        /// ID, Name, Phone, Email, Password, Type of Shipment, and Maximum Delivery Distance.
        /// The courier is created as active by default with the working start date set to the current system clock.
        /// </remarks>
        private static Courier createCourier()
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
        /// <remarks>
        /// Allows updating the following courier properties:
        /// Name, Phone, Email, Password, Active status, Maximum Distance, and Type of Shipment.
        /// Each update is immediately persisted to the DAL.
        /// The menu loops until the user chooses to exit (option 0).
        /// </remarks>
        private static void updateCourier(int id)
        {
            int choice = 0;
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
                choice = GetIntInput();
                Action action = choice switch
                {
                    0 => () => Console.WriteLine("good bye"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Name: ");
                        string name = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Name = name;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Phone = phone;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Email: ");
                        string email = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Email = email;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        courierToUpdate.Password = password;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Active status (true/false): ");
                        bool isActive = bool.Parse(Console.ReadLine() ?? "true");
                        courierToUpdate.Active = isActive;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Max Distance Delivery (in km): ");
                        double maxDistance = double.Parse(Console.ReadLine() ?? "0");
                        courierToUpdate.MaxDistanceDelivery = maxDistance;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                        int typeShipmentInput = GetIntInput();
                        courierToUpdate.TypeShipment = (TheTypeShipment)typeShipmentInput;
                        s_dal.Courier?.Update(courierToUpdate);
                    },
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                };
                action();
            }
            while (choice != 0);
        }

        /// <summary>
        /// Prompts the user for order information and creates a new order object.
        /// </summary>
        /// <returns>A new Order instance with user-provided data.</returns>
        /// <remarks>
        /// Collects customer information (name, phone, address), order details, weight, and type.
        /// The order is created with OPEN status and the order date is set to the current system clock.
        /// Latitude and Longitude are initialized to 0.0 and should be set by the system later.
        /// </remarks>
        private static Order createOrder()
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
        /// <remarks>
        /// Allows updating the following order properties:
        /// Name, Phone, Address, Details, Weight, Type of Order, and Order Status.
        /// Each update is immediately persisted to the DAL.
        /// The menu loops until the user chooses to exit (option 0).
        /// </remarks>
        private static void updateOrder(int id)
        {
            int choice = 0;
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
                choice = GetIntInput();
                Action action = choice switch
                {
                    0 => () => Console.WriteLine("good bye"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Name: ");
                        string name = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Name = name;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Phone: ");
                        string phone = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Phone = phone;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Addres = address;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    4 => () =>
                    {
                        Console.WriteLine("Enter new Details: ");
                        string details = Console.ReadLine() ?? string.Empty;
                        orderToUpdate.Details = details;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Weight: ");
                        int weight = GetIntInput();
                        orderToUpdate.Weight = weight;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Type of Order (0=STANDARD, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
                        int typeOfOrderInput = GetIntInput();
                        orderToUpdate.TypeOfOrder = (TypeOfOrder)typeOfOrderInput;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Order Status (0=OPEN, 1=IN PROGRESS, 2=DELIVERED): ");
                        int orderStatusInput = GetIntInput();
                        orderToUpdate.OrderStatus = (OrderStatus)orderStatusInput;
                        s_dal.Order?.Update(orderToUpdate);
                    },
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                };
                action();
            }
            while (choice != 0);
        }

        /// <summary>
        /// Provides an interactive menu for updating system configuration settings.
        /// </summary>
        /// <remarks>
        /// Allows updating various system settings including:
        /// - Manager credentials (ID and password)
        /// - Store location (address, latitude, longitude)
        /// - Delivery constraints (max delivery range)
        /// - Average speeds for different vehicle types
        /// - Time constraints (max delivery time, risk range, max inactivity time)
        /// The menu loops until the user chooses to exit (option 0).
        /// </remarks>
        private static void updateSetting()
        {
            if (s_dal.Config == null)
            {
                Console.WriteLine("Configuration DAL is not initialized.");
                return;
            }
            int choice = 0;
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
                choice = GetIntInput();
                Action action = choice switch
                {
                    1 => () =>
                    {
                        Console.WriteLine("Enter new Manager ID: ");
                        int id = GetIntInput();
                        s_dal.Config.ManagerId = id;
                    },
                    2 => () =>
                    {
                        Console.WriteLine("Enter new Menu Password: ");
                        string password = Console.ReadLine() ?? string.Empty;
                        s_dal.Config.PasswordManager = password;
                    },
                    3 => () =>
                    {
                        Console.WriteLine("Enter new Store Address: ");
                        string address = Console.ReadLine() ?? string.Empty;
                        s_dal.Config.storeAddress = address;
                    },
                    5 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Latitude: ");
                        double latitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.Latitude = latitude;
                    },
                    6 => () =>
                    {
                        Console.WriteLine("Enter new Delivery Longitude: ");
                        double longitude = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.Longitude = longitude;
                    },
                    7 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Range (in km): ");
                        double maxRange = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.MaxDeliveryRange = maxRange;
                    },
                    8 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Car (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedCar = speed;
                    },
                    9 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Motorcycle (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedMotorcycle = speed;
                    },
                    10 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Bike (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedBike = speed;
                    },
                    11 => () =>
                    {
                        Console.WriteLine("Enter new Average Speed for Foot (in km/h): ");
                        double speed = double.Parse(Console.ReadLine() ?? "0");
                        s_dal.Config.AvgSpeedFoot = speed;
                    },
                    12 => () =>
                    {
                        Console.WriteLine("Enter new Max Delivery Time (in minutes): ");
                        int minutes = GetIntInput();
                        s_dal.Config.MaxDeliveryTime = TimeSpan.FromMinutes(minutes);
                    },
                    13 => () =>
                    {
                        Console.WriteLine("Enter new Risk Range (in minutes): ");
                        int minutes = GetIntInput();
                        s_dal.Config.RiskRange = TimeSpan.FromMinutes(minutes);
                    },
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                };
                action();
            }
            while (choice != 0);
        }

        /// <summary>
        /// Provides an interactive menu for reading and displaying system configuration settings.
        /// </summary>
        /// <remarks>
        /// Displays various system settings including:
        /// - Manager credentials
        /// - Store location details
        /// - Delivery constraints
        /// - Average speeds for different vehicle types
        /// - Time constraints
        /// The menu loops until the user chooses to exit (option 0).
        /// </remarks>
        private static void readSetting()
        {
            if (s_dal.Config == null)
            {
                Console.WriteLine("Configuration DAL is not initialized.");
                return;
            }
            int choice = 0;
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
                choice = GetIntInput();
                Action action = choice switch
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
                };
                action();
            }
            while (choice != 0);
        }

        /// <summary>
        /// Prompts the user to create a new delivery by selecting either an order first or a courier first.
        /// Filters available couriers based on order requirements or vice versa.
        /// Updates the selected order status to DELIVERING.
        /// </summary>
        /// <returns>A new Delivery instance with user-selected order and courier.</returns>
        /// <exception cref="DalisNotAvailable">Thrown when orders or couriers are not available.</exception>
        /// <remarks>
        /// The method offers two workflows:
        /// 1. Select order first - then filters couriers compatible with the order
        /// 2. Select courier first - then filters orders compatible with the courier
        /// Compatibility is based on shipment type, delivery distance, and order status.
        /// </remarks>
        private static Delivery createDelivery()
        {
            Console.WriteLine($"        Creating a new delivery\n" +
                $"          enter 1 for create by order, 2 by courier");
            int choice = GetIntInput();
            Order? selectedOrder;
            Courier? selectedCourier;

            if (choice == 1)
            {
                selectedOrder = getItemFromListById<Order>(s_dal.Order.ReadAll().ToList(),
                    o => o.OrderStatus == OrderStatus.OPEN,
                    o => o.Id);

                selectedCourier = getItemFromListById<Courier>(s_dal.Courier.ReadAll().ToList(),
                    c => c.Active == true &&
                    matchTypeShipmentAndOrder(c.TypeShipment, selectedOrder.TypeOfOrder) &&
                    c.MaxDistanceDelivery >= selectedOrder.DistanceKm,
                    c => c.Id);
            }
            else
            {
                selectedCourier = getItemFromListById<Courier>(s_dal.Courier.ReadAll().ToList(),
                    c => c.Active == true,
                    c => c.Id);

                selectedOrder = getItemFromListById<Order>(s_dal.Order.ReadAll().ToList(),
                    o => o.OrderStatus == OrderStatus.OPEN &&
                    matchTypeShipmentAndOrder(selectedCourier.TypeShipment, o.TypeOfOrder) &&
                    selectedCourier.MaxDistanceDelivery >= o.DistanceKm,
                    o => o.Id);
            }

            Console.Write("Enter Actual Distance: ");
            double actualDistance = double.Parse(Console.ReadLine() ?? "0");
            s_dal.Order?.Update(selectedOrder with { OrderStatus = OrderStatus.DELIVERING });

            return new Delivery
            {
                Id = 0,
                OrderId = selectedOrder.Id,
                CourierId = selectedCourier.Id,
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
        /// <remarks>
        /// This helper method ensures that all integer inputs in the application are properly validated.
        /// It handles invalid input gracefully by displaying an error message and re-prompting the user.
        /// </remarks>
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
        /// <remarks>
        /// This generic menu handles:
        /// - Create: Creates a new entity using type-specific creation methods
        /// - Read: Retrieves and displays a single entity by ID
        /// - ReadAll: Retrieves and displays all entities of the type
        /// - Update: Updates an entity using type-specific update methods
        /// - Delete: Deletes a single entity by ID
        /// - DeleteAll: Removes all entities of the type
        /// Supports Courier, Order, and Delivery entity types.
        /// </remarks>
        private static void dataMenu<T>(ICrud<T>? dal) where T : class
        {
            string typeName = typeof(T).Name.ToLower();
            Console.WriteLine($"Set {typeName} Menu.");
            int choice;
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

                choice = GetIntInput();

                try
                {
                    Action action = choice switch
                    {
                        0 => () => Console.WriteLine($"exit from set {typeof(T).Name}"),
                        1 => () =>
                        {
                            T? newItem = typeof(T).Name switch
                            {
                                nameof(Courier) => createCourier() as T,
                                nameof(Order) => createOrder() as T,
                                nameof(Delivery) => createDelivery() as T,
                                _ => throw new DalErrorConfig($"Unknown type: {typeof(T).Name}")
                            };

                            if (newItem != null)
                            {
                                dal?.Create(newItem);
                                Console.WriteLine($"{typeName} created successfully!");
                            }
                        },
                        2 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id: ");
                            int id = GetIntInput();
                            var result = dal?.Read(id);
                            if (result == null)
                                Console.WriteLine($"No {typeName} found with id {id}");
                            else
                                Console.WriteLine(result);
                        },
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
                        },
                        4 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id to update: ");
                            int id = GetIntInput();
                            switch (typeof(T).Name)
                            {
                                case nameof(Courier):
                                    updateCourier(id);
                                    break;
                                case nameof(Order):
                                    updateOrder(id);
                                    break;
                                default:
                                    throw new DalErrorConfig($"Update not supported for type: {typeof(T).Name}");
                            }
                        },
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
                        },
                        6 => () =>
                        {
                            dal?.DeleteAll();
                            Console.WriteLine($"All {typeName}s deleted successfully!");
                        },
                        _ => () => Console.WriteLine("Invalid choice, please try again.")
                    };

                    action();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            } while (choice != 0);
        }

        /// <summary>
        /// Provides an interactive menu for system settings management, including clock manipulation and configuration updates.
        /// </summary>
        /// <remarks>
        /// This menu allows:
        /// - Time manipulation: Move system clock forward by minutes, hours, or days
        /// - Configuration viewing: Display current system date and time
        /// - Settings management: Update and read configuration values
        /// - System reset: Reset all settings to defaults
        /// Useful for testing time-based delivery scenarios without waiting for real time.
        /// </remarks>
        private static void settingMenu()
        {
            int choice;
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
                choice = GetIntInput();
                Action action = choice switch
                {
                    0 => () => Console.WriteLine("exit from settings menu"),
                    1 => () =>
                    {
                        Console.WriteLine("Enter number of minutes to move forward: ");
                        int minutes = GetIntInput();
                        if (s_dal.Config != null)
                        {
                            s_dal.Config.Clock = s_dal.Config.Clock.AddMinutes(minutes);
                            Console.WriteLine($"System clock moved forward by {minutes} minutes.");
                        }
                    },
                    2 => () =>
                    {
                        Console.WriteLine("Enter number of hours to move forward: ");
                        int hours = GetIntInput();
                        if (s_dal?.Config != null)
                        {
                            s_dal!.Config.Clock = s_dal!.Config.Clock.AddHours(hours);
                            Console.WriteLine($"System clock moved forward by {hours} hours.");
                        }
                    },
                    3 => () =>
                    {
                        Console.WriteLine("Enter number of days to move forward: ");
                        int days = GetIntInput();
                        if (s_dal?.Config != null)
                        {
                            s_dal!.Config.Clock = s_dal!.Config.Clock.AddDays(days);
                            Console.WriteLine($"System clock moved forward by {days} days.");
                        }
                    },
                    4 => () =>
                    {
                        if (s_dal.Config != null)
                        {
                            Console.WriteLine($"Current system date and time: {s_dal.Config.Clock}");
                        }
                    },
                    5 => () =>
                    {
                        updateSetting();
                    },
                    6 => () =>
                    {
                        readSetting();
                    },
                    7 => () => s_dal?.Config?.Reset(),
                    _ => () => Console.WriteLine("Invalid choice, please try again.")
                };
                action();
            }
            while (choice != 0);
        }

        /// <summary>
        /// Main entry point of the application.
        /// Displays the main menu and handles user navigation between different entity management menus.
        /// </summary>
        /// <param name="args">Command line arguments (not used).</param>
        /// <remarks>
        /// The main menu provides access to:
        /// - Courier management (CRUD operations)
        /// - Order management (CRUD operations)
        /// - Delivery management (CRUD operations)
        /// - Random data initialization for testing
        /// - Display all data across all entity types
        /// - Settings and configuration management
        /// - Complete data removal
        /// All operations are wrapped in exception handling to ensure graceful error recovery.
        /// </remarks>
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
                            dataMenu(s_dal!.Courier!);
                            break;
                        case 2:
                            dataMenu(s_dal!.Order!);
                            break;
                        case 3:
                            dataMenu(s_dal!.Delivery!);
                            break;
                        case 5:
                            Initialization.Do(s_dal);
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
                            settingMenu();
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
