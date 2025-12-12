





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
    private static Order createOrder()
    {
        Console.WriteLine("Enter type of order ( STANDART=0, FAST_DELIVERY=1,DELIVER_IMMEDIATELY=2): ");
        int typeOfOrderInput = GetIntInput();
        Console.WriteLine("Enter datials of order");
        string? datails = Console.ReadLine();
        Console.WriteLine("Enter address of order: ");
        string? address = Console.ReadLine();
        Console.WriteLine("Enter weight of order (in grams): ");
        int weight = GetIntInput();
        Console.WriteLine("Enter name of customer: ");
        string? name = Console.ReadLine();
        Console.WriteLine("Enter phone of customer: ");
        string? phone = Console.ReadLine();
        return new Order
        {
            Id = 0,
            TypeOfOrder = (TypeOfOrder)typeOfOrderInput,
            Details = datails ?? string.Empty,
            Addres = address ?? string.Empty,
            Weight = weight,
            Latitude = 0,
            Longitude = 0,
            Distance = 0,
            Name = name ?? string.Empty,
            Phone = phone ?? string.Empty,
            OrderDate = s_bl.Admin.GetClock(),
            EstimatedDeliveryTime = s_bl.Admin.GetClock(),
            MaxDeliveryTime = s_bl.Admin.GetClock(),
            OrderStatus = OrderStatus.OPEN,
            ScheduleStatus = ScheduleStatus.ONTYME,
            TimeLeftForDelivery = TimeSpan.Zero,
            DeliveryPerOrderInLists = new List<DeliveryPerOrderInList>(),
        };

    }
    private static IEnumerable<CourierInList> readAllCouriers(int requesterId)
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
    private static IEnumerable<OrderInList> readAllOrders(int requesterId)
    {
        Console.WriteLine($@"
                Filter orders:                       
                    1. OPEN only 
                    2. IN_PROGRESS only 
                    3. COMPLETED only
                    4. CANCELED only
                    5. All
                        Choose");
        int filterChoice = GetIntInput();
        OrderStatus? orderStatus = filterChoice switch
        {
            1 => OrderStatus.OPEN,
            2 => OrderStatus.DELIVERING,
            3 => OrderStatus.COMPLETED,
            4 => OrderStatus.REFUSED,
            5 => OrderStatus.CONCELLED,
            6 => null,
            _ => null
        };
        Console.WriteLine($@"
      Choose a field to filter by value:
          1. OrderId
          2. CustomerName
          3. Distance (km)
          4. No value filter
              Choose");
        int valueChoice = GetIntInput();



        Console.WriteLine($@"
                Sort orders by:
                    1. OrderId
                    2. TypeOfOrder
                    3. DistanceKm
                    4. OrderStatus
                    5. ScheduleStatus
                    6. No sorting
                        Choose");
        int sortChoice = GetIntInput();
        OrderInListField? sort = sortChoice switch
        {
            1 => OrderInListField.OrderId,
            2 => OrderInListField.TypeOfOrder,
            3 => OrderInListField.DistanceKm,
            4 => OrderInListField.OrderStatus,
            5 => OrderInListField.ScheduleStatus,
            6 => null,
            _ => null
        };

        return s_bl.Order.ReadAll(requesterId, orderStatus, valueChoice, sort);
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
    private static Order updateOrder(int requesterId)
    {
        Console.WriteLine("Update of order");
        Console.WriteLine("Enter ID of order to update: ");
        int id = GetIntInput();
        Console.WriteLine("Enter type of order ( STANDART=0, FAST_DELIVERY=1,DELIVER_IMMEDIATELY=2): ");
        int typeOfOrderInput = GetIntInput();
        Console.WriteLine("Enter datials of order");
        string? datails = Console.ReadLine();
        Console.WriteLine("Enter address of order: ");
        string? address = Console.ReadLine();
        Console.WriteLine("Enter weight of order (in grams): ");
        int weight = GetIntInput();
        Console.WriteLine("Enter name of customer: ");
        string? name = Console.ReadLine();
        Console.WriteLine("Enter phone of customer: ");
        string? phone = Console.ReadLine();
        return new Order
        {
            Id = id,
            TypeOfOrder = (TypeOfOrder)typeOfOrderInput,
            Details = datails ?? string.Empty,
            Addres = address ?? string.Empty,
            Weight = weight,
            Latitude = 0,
            Longitude = 0,
            Distance = 0,
            Name = name ?? string.Empty,
            Phone = phone ?? string.Empty,
            OrderDate = s_bl.Admin.GetClock(),
            EstimatedDeliveryTime = s_bl.Admin.GetClock(),
            MaxDeliveryTime = s_bl.Admin.GetClock(),
            OrderStatus = OrderStatus.OPEN,
            ScheduleStatus = ScheduleStatus.ONTYME,
            TimeLeftForDelivery = TimeSpan.Zero,
            DeliveryPerOrderInLists = new List<DeliveryPerOrderInList>(),
        };
    }
    private static IEnumerable<ClosedDeliveryInList> getlistClosedDeliveries(int requesterId)
    {
        Console.WriteLine("Enter courier id");
        int courierId = GetIntInput();
        Console.WriteLine($@"
                Filter type of order:
                    1. STANDART only
                    2. FAST_DELIVERY only
                    3. DELIVER_IMMEDIATELY only
                        Choose");
        int filterChoice = GetIntInput();
        TypeOfOrder? typeOfOrder = filterChoice switch
        {
            1 => TypeOfOrder.STANDART,
            2 => TypeOfOrder.FAST_DELIVERY,
            3 => TypeOfOrder.DELIVER_IMMEDIATELY,
            _ => null
        };

        Console.WriteLine($@"
                Sort closed deliveries by:
                    1. DeliveryId
                    2. OrderId
                    3. TypeOfOrder
                    4. Address
                    5. ShipmentType
                    6. AqualDistens
                    7. DelyveryTime
                    8. EndDelivery
                    9. No sorting
                        Choose");
        int sortChoice = GetIntInput();
        ClosedDeliveryInListField? sort = sortChoice switch
        {
            1 => ClosedDeliveryInListField.DeliveryId,
            2 => ClosedDeliveryInListField.OrderId,
            3 => ClosedDeliveryInListField.TypeOfOrder,
            4 => ClosedDeliveryInListField.Address,
            5 => ClosedDeliveryInListField.ShipmentType,
            6 => ClosedDeliveryInListField.AqualDistens,
            7 => ClosedDeliveryInListField.DelyveryTime,
            8 => ClosedDeliveryInListField.EndDelivery,
            9 => null,
            _ => null
        };
        return s_bl.Order.GetClosed(requesterId, courierId, typeOfOrder, sort);
    }
    private static IEnumerable<OpenOrderInList> getlistOpenOrders(int requesterId)
    {
        Console.WriteLine("Enter courier id");
        int courierId = GetIntInput();
        Console.WriteLine($@"
                Filter type of order:
                    1. STANDART only
                    2. FAST_DELIVERY only
                    3. DELIVER_IMMEDIATELY only
                        Choose");
        int filterChoice = GetIntInput();
        TypeOfOrder? typeOfOrder = filterChoice switch
        {
            1 => TypeOfOrder.STANDART,
            2 => TypeOfOrder.FAST_DELIVERY,
            3 => TypeOfOrder.DELIVER_IMMEDIATELY,
            _ => null
        };
        Console.WriteLine($@"
                Sort open orders by:
                    1. OrderId
                    2. TypeOfOrder
                    3. DistanceKm
                    4. OrderStatus
                    5. ScheduleStatus
                    6. No sorting
                        Choose");
        int sortChoice = GetIntInput();
        OpenOrderInListField? sort = sortChoice switch
        {
            1 => OpenOrderInListField.OrderId,
            2 => OpenOrderInListField.TypeOfOrder,
            3 => OpenOrderInListField.DistanceKm,
            4 => OpenOrderInListField.Weight,
            5 => OpenOrderInListField.ScheduleStatus,
            6 => null,
            _ => null
        };
        return s_bl.Order.GetOpen(requesterId, courierId, typeOfOrder, sort);
    }
    private static void setCourier()
    {
        Console.WriteLine("Set Courier Menu.");
        Console.Write("Enter ID of the requester: ");
        int requesterId = GetIntInput();
        do
        {
            Console.WriteLine(@$"

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
                        IEnumerable<CourierInList> couriers = readAllCouriers(requesterId);

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
                        s_bl.Courier.Delete(requesterId, deleteId);
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
    private static void setOrder()
    {
        Console.WriteLine("Set Order Menu.");
        Console.WriteLine("Enter ID of the requester: ");
        int requesterId = GetIntInput();
        do
        {
            Console.WriteLine(@$"
    to exit press 0
    to create order press 1
    to read order press 2
    to read all orders press 3
    to update order press 4
    to delete order press 5
    to cancel order press 6
    To report a delivery press 7
    to start delivery press 8
    to get closed deliveries press 9
    to get open deliveries press 10
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
                        Order newOrder = createOrder();
                        s_bl.Order.Create(requesterId, newOrder);
                        Console.WriteLine("Order created successfully!");
                        break;
                    case 2:
                        Console.WriteLine("Enter order id: ");
                        int id = GetIntInput();
                        Order? result = s_bl.Order.Read(requesterId, id);
                        //האם צריך חריגה? והאם צריך סימן שאלה, הרי אם הוא נול כבר יש שם חריגה לתפוס
                        Console.WriteLine(result);
                        break;
                    case 3:
                        IEnumerable<OrderInList> orders = readAllOrders(requesterId);
                        if (!orders.Any())
                        {
                            Console.WriteLine("No orders found.");
                        }
                        else
                        {
                            foreach (OrderInList order in orders)
                                Console.WriteLine(order);
                        }
                        break;
                    case 4:
                        Order updatedOrder = updateOrder(requesterId);
                        s_bl.Order.Update(requesterId, updatedOrder);
                        break;
                    case 5:
                        Console.WriteLine("Enter order id: ");
                        int deleteId = GetIntInput();
                        s_bl.Order.Delete(requesterId, deleteId);
                        Console.WriteLine("Order deleted successfully!");
                        break;
                    case 6:
                        Console.WriteLine("Enter order id to cancel: ");
                        int cancelId = GetIntInput();
                        s_bl.Order.Cancel(requesterId, cancelId);
                        Console.WriteLine("Order canceled successfully!");
                        break;
                    case 7:
                        Console.WriteLine("Enter order id to report delivery: ");
                        int orderId = GetIntInput();
                        Console.WriteLine("Enter courier id");
                        int courierId = GetIntInput();
                        s_bl.Order.Deliver(requesterId, courierId, orderId);
                        Console.WriteLine("Delivery reported successfully!");
                        break;
                    case 8:
                        Console.WriteLine("Enter order id to start delivery: ");
                        orderId = GetIntInput();
                        Console.WriteLine("Enter courier id");
                        courierId = GetIntInput();
                        s_bl.Order.StartDelivery(requesterId, courierId, orderId);
                        Console.WriteLine("Delivery started successfully!");
                        break;
                    case 9:

                        IEnumerable<ClosedDeliveryInList> closedDeliveryInLists = getlistClosedDeliveries(requesterId);
                        if (!closedDeliveryInLists.Any())
                        {
                            Console.WriteLine("No closed deliveries found.");
                        }
                        else
                        {
                            foreach (ClosedDeliveryInList closedDelivery in closedDeliveryInLists)
                                Console.WriteLine(closedDelivery);
                        }
                        break;
                    case 10:
                        IEnumerable<OpenOrderInList> openOrderInLists = getlistOpenOrders(requesterId);
                        if (!openOrderInLists.Any())
                        {
                            Console.WriteLine("No open orders found.");
                        }
                        else
                        {
                            foreach (OpenOrderInList openOrder in openOrderInLists)
                                Console.WriteLine(openOrder);
                        }
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
                        setCourier();
                        break;
                    case 2:
                        setOrder();
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




