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

        private class DalWrapper
        {
            private readonly object _dal;
            private readonly string _typeName;

            public DalWrapper(ICourier dal) { _dal = dal; _typeName = "courier"; }
            public DalWrapper(IOrder dal) { _dal = dal; _typeName = "order"; }
            public DalWrapper(IDelivery dal) { _dal = dal; _typeName = "delivery"; }

            public string TypeName => _typeName;
            public void Create()
            {
                // Implementation for Create can be added here
                Console.WriteLine($"Create method for {_typeName} called.");
                switch (_dal)
                {
                    case ICourier courier:
                        {
                            Console.WriteLine("Creating a new courier...");
                            Console.Write(" Enter ID: ");
                            Console.WriteLine(" Enter Name: ");
                            string name = Console.ReadLine() ?? string.Empty;
                            Console.WriteLine(" Enter Phone: ");
                            string phone = Console.ReadLine() ?? string.Empty;
                            int id = GetIntInput();
                            Console.WriteLine(" Enter Email: ");
                            string email = Console.ReadLine() ?? string.Empty;
                            string password = string.Empty;
                            while (password == string.Empty)
                            {
                                Console.WriteLine(" Enter Password: ");
                                password = Console.ReadLine() ?? string.Empty;
                            }
                            Console.WriteLine(" Enter Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                            int typeShipmentInput = GetIntInput();
                            Console.WriteLine(" Enter Max Distance Delivery (in km): ");
                            string maxDistanceInput = Console.ReadLine() ?? "0";
                            double maxDistanceDelivery = double.Parse(maxDistanceInput);
                            Courier newCourier = new DO.Courier
                            {
                                Id = id,
                                Name = name,
                                Phone = phone,
                                Email = email,
                                Password = password,
                                Active = true,
                                MaxDistanceDelivery = maxDistanceDelivery,
                                TypeShipment = (TheTypeShipment)typeShipmentInput,
                                WorkingSince = DateTime.Now
                            };
                            s_dalCourier?.Create(newCourier);
                            break;
                        }
                    case IOrder order:
                        {
                            Console.WriteLine("Creating a new order...");
                            Console.Write(" Enter Name: ");
                            string name = Console.ReadLine() ?? string.Empty;
                            Console.Write(" Enter Phone: ");
                            string phone = Console.ReadLine() ?? string.Empty;
                            Console.Write(" Enter Customer Address: ");
                            string address = Console.ReadLine() ?? string.Empty;
                            Console.Write(" Enter datails of yor order");
                            string? datails = Console.ReadLine();
                            Console.Write(" enter Weight of order: ");
                            int Weight = GetIntInput();
                            Console.Write(" Enter Type Shipment (0=STANDART, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
                            var typeOfOrder = (TypeOfOrder)GetIntInput();

                            Order newOrder = new DO.Order
                            {
                                Id = 0, // ID will be set by DAL
                                Name = name,
                                Phone = phone,
                                Addres = address,
                                Details = datails,
                                Weight = Weight,
                                TypeOfOrder = typeOfOrder,
                                OrderStatus = 0,
                                Latitude = 0.0, // Placeholder, should be set appropriately
                                Longitude = 0.0, // Placeholder, should be set appropriately
                                OrderDate = s_dalConfig?.Clock ?? DateTime.Now,
                            };
                            s_dalOrder?.Create(newOrder);
                            break;
                        }
                    case IDelivery delivery:
                        {
                            Console.WriteLine("Creating a new delivery...");
                            var matchedOrders = s_dalOrder?.ReadAll() ??
                                throw new Exception("No orders available to create a delivery.");

                            // סינון הזמנות פתוחות
                            matchedOrders = matchedOrders.Where(o => o.OrderStatus == OrderStatus.OPEN).ToList();

                            if (matchedOrders.Count == 0)
                            {
                                Console.WriteLine("No open orders available.");
                                break;
                            }

                            Console.WriteLine($"Available open orders: {string.Join(", ", matchedOrders.Select(o => o.Id))}");

                            int orderid;
                            Order? selectedOrder = null;
                            do
                            {
                                Console.WriteLine("Enter open Order ID: ");
                                orderid = GetIntInput();
                                selectedOrder = matchedOrders.Find(o => o.Id == orderid);

                                if (selectedOrder == null)
                                {
                                    Console.WriteLine($"Order ID {orderid} not found or not open. Please try again.");
                                }
                            }
                            while (selectedOrder == null); // ממשיך כל עוד לא מצאנו!

                            Console.WriteLine($"Selected order: {selectedOrder.Id}");

                            // עכשיו מסננים שליחים מתאימים
                            var matchedCouriers = s_dalCourier?.ReadAll() ??
                                throw new Exception("No couriers available to create a delivery.");

                            matchedCouriers = matchedCouriers.Where(courier =>
                                courier.Active &&
                                MatchTypeShipmentAndOrder(courier.TypeShipment, selectedOrder.TypeOfOrder) &&
                                courier.MaxDistanceDelivery >= selectedOrder.DistanceKm
                            ).ToList();

                            if (matchedCouriers.Count == 0)
                            {
                                Console.WriteLine("No suitable couriers available for this order.");
                                break;
                            }

                            Console.WriteLine($"Available couriers: {string.Join(", ", matchedCouriers.Select(c => c.Id))}");

                            int courierId;
                            Courier? selectedCourier = null;
                            do
                            {
                                Console.WriteLine("Enter Courier ID: ");
                                courierId = GetIntInput();
                                selectedCourier = matchedCouriers.Find(c => c.Id == courierId);

                                if (selectedCourier == null)
                                {
                                    Console.WriteLine($"Courier ID {courierId} not found or not suitable. Please try again.");
                                }
                            }
                            while (selectedCourier == null);

                            Console.WriteLine("Enter Actual Distance: ");
                            double actualDistance = double.Parse(Console.ReadLine() ?? "0");

                            Delivery newDelivery = new DO.Delivery
                            {
                                Id = 0, // ID will be set by DAL
                                OrderId = orderid,
                                CourierId = courierId,
                                TypeOfOrder = selectedOrder.TypeOfOrder,
                                OrderDate = s_dalConfig?.Clock ?? DateTime.Now,
                                ActualDistance = actualDistance,
                                TimeEndDelivery = null
                            };

                            s_dalDelivery?.Create(newDelivery);
                            Console.WriteLine("Delivery created successfully!");
                            break;
                        }

                }
            }

            public object? Read(int id)
            {
                return _dal switch
                {
                    ICourier courier => courier.Read(id),
                    IOrder order => order.Read(id),
                    IDelivery delivery => delivery.Read(id),
                    _ => null
                };
            }

            public void Update(object item)
            {
                switch (_dal)
                {
                    case ICourier courier:
                        courier.Update((Courier)item);
                        break;
                    case IOrder order:
                        order.Update((Order)item);
                        break;
                    case IDelivery delivery:
                        delivery.Update((Delivery)item);
                        break;
                }
            }
            public List<object> ReadAll()
            {
                return _dal switch
                {
                    ICourier courier => courier.ReadAll().Cast<object>().ToList(),
                    IOrder order => order.ReadAll().Cast<object>().ToList(),
                    IDelivery delivery => delivery.ReadAll().Cast<object>().ToList(),
                    _ => new List<object>()
                };
            }

            public void Delete(int id)
            {
                switch (_dal)
                {
                    case ICourier courier:
                        courier.Delete(id);
                        break;
                    case IOrder order:
                        order.Delete(id);
                        break;
                    case IDelivery delivery:
                        delivery.Delete(id);
                        break;
                }
            }

            public void DeleteAll()
            {
                switch (_dal)
                {
                    case ICourier courier:
                        courier.DeleteAll();
                        break;
                    case IOrder order:
                        order.DeleteAll();
                        break;
                    case IDelivery delivery:
                        delivery.DeleteAll();
                        break;
                }
            }
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
        private static void setObject(DalWrapper dal)
        {
            Console.WriteLine("setObject method called.");
        }

        private static void DataMenu(DalWrapper dal)  // הוסף static!
        {
            string typeName = dal.TypeName;
            Console.WriteLine($"Set {typeName} method called.");
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
                        1 => () => dal.Create(),
                        2 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id: ");
                            int id = GetIntInput();
                            var result = dal.Read(id);
                            if (result == null)
                                Console.WriteLine($"No {typeName} found with id {id}");
                            else
                                Console.WriteLine(result);
                        }
                        ,
                        3 => () =>
                        {
                            var items = dal.ReadAll();
                            if (items.Count == 0)
                                Console.WriteLine($"No {typeName}s found.");
                            else
                                items.ForEach(item => Console.WriteLine(item));
                        }
                        ,
                        4 => () => { Console.WriteLine($"Update {typeName} - Not yet implemented"); }
                        ,
                        5 => () =>
                        {
                            Console.WriteLine($"Enter {typeName} id: ");
                            int id = GetIntInput();
                            try
                            {
                                dal.Delete(id);
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
                            dal.DeleteAll();
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
                            DataMenu(new DalWrapper(s_dalCourier!));
                            break;
                        case 2:
                            DataMenu(new DalWrapper(s_dalOrder!));
                            break;
                        case 3:
                            DataMenu(new DalWrapper(s_dalDelivery!));
                            break;
                        case 5:
                            Initialization.Do(s_dalCourier, s_dalOrder, s_dalDelivery, s_dalConfig);
                            break;
                        case 6:
                            s_dalCourier?.ReadAll().ForEach(courier => Console.WriteLine(courier));
                            s_dalOrder?.ReadAll().ForEach(oreder => Console.WriteLine(oreder));
                            s_dalDelivery?.ReadAll().ForEach(delivery => Console.WriteLine(delivery));
                            break;
                        case 7:
                            // s_dalConfig?.Read();
                            break;
                        case 8:
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
