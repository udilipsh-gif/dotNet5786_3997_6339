




namespace BO;
internal class Program
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


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

    private static Courier createCourier()
    {


        Console.WriteLine("Creating a new courier...");

        Console.Write("Enter ID of courier: ");
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
            WorkingSince = s_bl.Admin.GetClock(),
            DeliveryOnTime = 0,
            DeliveryLate = 0,

        };

    }
    private static IEnumerable<CourierInList> ReadAllCouriers(int requesterId)
    {
        Console.WriteLine($@"
                Filter couriers:                       
                    1.Active only 
                    2. Inactive only 
                    3. All
                        Choose");
        int filterChoice = GetIntInput();

        bool? isActive = filterChoice switch
        {
            1 => true,
            2 => false,
            3 => null,
            _ => null
        };


        Console.WriteLine($@"
                Sort couriers by:
                    1. Name
                    2. Id
                    3. Phone
                    4. TypeShipment
                    5. AvailableDeliveries
                    6. No sorting
                        Choose");

        int sortChoice = GetIntInput();


        CourierFieldSort? sort = sortChoice switch
        {
            1 => CourierFieldSort.Name,
            2 => CourierFieldSort.Id,
            3 => CourierFieldSort.Phone,
            4 => CourierFieldSort.TypeShipment,
            5 => CourierFieldSort.AvailableDeliveries,
            6 => null,
            _ => null
        };

        return s_bl.Courier.ReadAll(requesterId, isActive, sort);

    }
    private static Courier updateCourier(int requesterId)
    {


        Console.WriteLine("Update of courier");

        Console.Write("Enter ID of courier: ");
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
            WorkingSince = s_bl.Admin.GetClock(),
            DeliveryOnTime = 0,
            DeliveryLate = 0,

        };


    }
    private static void SetCourier()
    {
        Console.Write("Enter ID of the requester: ");
        int requesterId = GetIntInput();
        do
        {
            Console.WriteLine(@$"
Set courier Menu.
    to exit press 0
    to create courier press 1
    to read courier press 2
    to read all couriers press 3
    to update courier press 4
    to delete courier press 5
");
            int choice = GetIntInput();

            try
            {
                switch (choice)
                {
                    case 0:
                        Console.WriteLine("exit from set courier");
                        break;
                    case 1:
                        Courier newCourier = createCourier();
                        s_bl.Courier.Create(requesterId, newCourier);
                        Console.WriteLine("Courier created successfully!");
                        break;
                    case 2:
                        Console.WriteLine("Enter courier id: ");
                        int id = GetIntInput();
                        Courier? result = s_bl.Courier.Read(requesterId, id);
                        //האם צריך חריגה? והאם צריך סימן שאלה, הרי אם הוא נול כבר יש שם חריגה לתפוס
                        Console.WriteLine(result);
                        break;

                    case 3:
                        IEnumerable<CourierInList> couriers = ReadAllCouriers(requesterId);

                        if (!couriers.Any())
                        {
                            Console.WriteLine("No couriers found.");
                        }
                        else
                        {
                            foreach (CourierInList courier in couriers)
                                Console.WriteLine(courier);
                        }
                        break;

                    case 4:
                        Courier updatedCourier = updateCourier(requesterId);
                        s_bl.Courier.Update(requesterId, updatedCourier);
                        break;
                    case 5:
                        Console.WriteLine("Enter courier id: ");
                        int deleteId = GetIntInput();
                        s_bl.Courier.Delete(requesterId,deleteId);
                        Console.WriteLine("Courier deleted successfully!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        } while (true);
    }


    //private static void dataMenu<T>(ICrud<T>? dal) where T : class
    //{
    //    string typeName = typeof(T).Name.ToLower();
    //    Console.WriteLine($"Set {typeName} Menu.");
    //    int choice;
    //    do
    //    {
    //        Console.WriteLine(@$"
    //Set {typeName} menu.
    //    to exit press 0
    //    to create {typeName} press 1
    //    to read {typeName} press 2
    //    to read all {typeName}s press 3
    //    to update press 4
    //    to delete {typeName} press 5
    //    to delete all {typeName}s press 6
    //    ");

    //        choice = GetIntInput();

    //        try
    //        {
    //            Action action = choice switch
    //            {
    //                0 => () => Console.WriteLine($"exit from set {typeof(T).Name}"),
    //                1 => () =>
    //                {
    //                    T? newItem = typeof(T).Name switch
    //                    {
    //                        nameof(Courier) => createCourier() as T,
    //                        nameof(Order) => createOrder() as T,
    //                        nameof(Delivery) => createDelivery() as T,
    //                        _ => throw new DalErrorConfig($"Unknown type: {typeof(T).Name}")
    //                    };

    //                    if (newItem != null)
    //                    {
    //                        dal?.Create(newItem);
    //                        Console.WriteLine($"{typeName} created successfully!");
    //                    }
    //                }
    //                ,
    //                2 => () =>
    //                {
    //                    Console.WriteLine($"Enter {typeName} id: ");
    //                    int id = GetIntInput();
    //                    var result = dal?.Read(id);
    //                    if (result == null)
    //                        Console.WriteLine($"No {typeName} found with id {id}");
    //                    else
    //                        Console.WriteLine(result);
    //                }
    //                ,
    //                3 => () =>
    //                {
    //                    var items = dal?.ReadAll();

    //                    if (items == null || !items.Any())
    //                    {
    //                        Console.WriteLine($"No {typeName}s found.");
    //                    }
    //                    else
    //                    {
    //                        items.ToList().ForEach(item => Console.WriteLine(item));
    //                    }
    //                }
    //                ,
    //                4 => () =>
    //                {
    //                    Console.WriteLine($"Enter {typeName} id to update: ");
    //                    int id = GetIntInput();
    //                    switch (typeof(T).Name)
    //                    {
    //                        case nameof(Courier):
    //                            updateCourier(id);
    //                            break;
    //                        case nameof(Order):
    //                            updateOrder(id);
    //                            break;
    //                        default:
    //                            throw new DalErrorConfig($"Update not supported for type: {typeof(T).Name}");
    //                    }
    //                }
    //                ,
    //                5 => () =>
    //                {
    //                    Console.WriteLine($"Enter {typeName} id: ");
    //                    int id = GetIntInput();
    //                    try
    //                    {
    //                        dal?.Delete(id);
    //                        Console.WriteLine($"{typeName} deleted successfully!");
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.Error.WriteLine(ex);
    //                    }
    //                }
    //                ,
    //                6 => () =>
    //                {
    //                    dal?.DeleteAll();
    //                    Console.WriteLine($"All {typeName}s deleted successfully!");
    //                }
    //                ,
    //                _ => () => Console.WriteLine("Invalid choice, please try again.")
    //            };

    //            action();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Error.WriteLine(ex);
    //        }
    //    } while (choice != 0);
    //}
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Book Soop! (stage 4)");



        int choice;
        do
        {
            Console.WriteLine("" +
                "Main Menu \n" +
                "   to exit press 0\n" +
                "   to set courier press 1\n" +
                "   to set order press 2\n" +
                "   to set delivery press 3\n" +
                "   to set admin press 5\n" +
                "   to print all data press 6\n" +
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
                        SetCourier();
                        break;
                    case 2:
                        //dataMenu(s_dal!.Order!);
                        break;
                    case 3:
                        //dataMenu(s_dal!.Delivery!);
                        break;
                    case 5:
                        Initialization.Do();
                        break;
                    case 6:
                        var couriers = s_dal!.Courier?.ReadAll();
                        if (!couriers!.Any())
                            Console.WriteLine("No couriers found.");

                        else
                            foreach (var courier in couriers!)
                                Console.WriteLine(courier);



                        var orders = s_dal!.Order?.ReadAll();
                        if (!orders!.Any())
                            Console.WriteLine("No orders found");
                        else
                            foreach (var order in orders!)
                                Console.WriteLine(order);


                        var deliveries = s_dal!.Delivery?.ReadAll();
                        if (!deliveries!.Any())
                            Console.WriteLine("No deliveries found");
                        else

                            foreach (var delivery in deliveries!)
                                Console.WriteLine(delivery);


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




