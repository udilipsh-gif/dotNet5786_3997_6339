using Dal;
using DalApi;
using DO;
using System.Data;
using System.Diagnostics.Metrics;

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

        private static void PrintCourierDetails(Courier courier)
        {
            Console.WriteLine($"Courier Details: " +
                $"ID: {courier.Id} " +
                $"Name: {courier.Name} " +
                $"Phone: {courier.Phone} " +
                $"Email: {courier.Email} " +
                $"Active: {courier.Active} " +
                $"Max Distance Delivery: {courier.MaxDistanceDelivery} " +
                $"Type Shipment: {courier.TypeShipment} " +
                $"Working Since: {courier.WorkingSince}");
        }
        private static void PrintOrderDetails(Order order)
        {
            Console.WriteLine($"Order Details: " +
                $"ID: {order.Id} " +
                $"Type of order: {order.TypeOfOrder} " +
                $"Details: {order.Details} " +
                $"Address: {order.Addres} " +
                $"Name: {order.Name} " +
                $"Phone: {order.Phone} " +
                $"Weight: {order.Weight} " +
                $"OrderDate: {order.OrderDate} " +
                $"OrderStatus: {order.OrderStatus} " +
                $"Distance Km: {order.DistanceKm}");
        }
        private static void PrintDeliveryDetails(Delivery delivery)
        {
            Console.WriteLine($"Delivery Details: " +
                $"ID: {delivery.Id} " +
                $"Order ID: {delivery.OrderId} " +
                $"Courier ID: {delivery.CourierId} " +
                $"Type of Order: {delivery.TypeOfOrder} " +
                $"Order Date: {delivery.OrderDate} " +
                $"Actual Distance: {delivery.ActualDistance} " +
                $"End Delivery: {delivery.EndDelivery} " +
                $"Time End Delivery: {delivery.TimeEndDelivery}");
        }
        private static object DataReception(string action, string type)
        {
            if (type == "delivery")
            {
                Console.WriteLine("     Enter Order ID: ");
                int orderId = int.Parse(Console.ReadLine());
                Console.WriteLine("     Enter Courier ID: ");
                int courierId = int.Parse(Console.ReadLine());


                Console.WriteLine("     Enter End Delivery Status (0=PENDING, 1=IN_PROGRESS, 2=DELIVERED): ");
                DO.EndDelivery endDelivery = (DO.EndDelivery)int.Parse(Console.ReadLine());
                Console.WriteLine("     Enter Time End Delivery (yyyy-MM-dd HH:mm:ss): ");
                DateTime timeEndDelivery = DateTime.Parse(Console.ReadLine());
                if (MatchTypeShipmentAndOrder(s_dalCourier.Read(courierId).TypeShipment, s_dalOrder.Read(orderId).TypeOfOrder) is false)
                {
                    throw new Exception("The courier's shipment type does not match the order type.");
                }
                if (s_dalOrder.Read(orderId).OrderStatus != OrderStatus.OPEN)
                {
                    throw new Exception("Order status must be IN_PROGRESS to create a delivery.");
                }
                if (s_dalCourier.Read(courierId).Active is false)
                {
                    throw new Exception("Courier must be active to create a delivery.");
                }
                if (s_dalCourier.Read(courierId).MaxDistanceDelivery < s_dalOrder.Read(orderId).DistanceKm)
                {
                    throw new Exception("Courier's maximum delivery distance is less than the order distance.");
                }
                double? actualDistance;
                if (s_dalCourier.Read(courierId).TypeShipment == TheTypeShipment.CAR || s_dalCourier.Read(courierId).TypeShipment == TheTypeShipment.MOTORCYCLE)
                {
                    actualDistance = s_dalOrder.Read(orderId).DistanceKmRoad;
                }
                else
                {
                    actualDistance = s_dalOrder.Read(orderId).DistanceKmWalk;
                }
                Delivery newDelivery = new DO.Delivery
                {
                    Id = 0, // ID will be set by DAL
                    OrderId = orderId,
                    CourierId = courierId,
                    TypeOfOrder = s_dalOrder.Read(orderId).TypeOfOrder,
                    OrderDate = DateTime.Now,
                    ActualDistance = actualDistance,
                    EndDelivery = endDelivery,
                    TimeEndDelivery = timeEndDelivery
                };
                return newDelivery;
            }

            //פרטים עבור שליח והזמנה, משותפים לשניהם, לא רלוונטי למשלוח שכבר טופל והחזיר ולא יגיע לכאן
            Console.Write("     Enter Name: ");
            string name = Console.ReadLine();

            Console.Write("     Enter Phone: ");
            string phone = Console.ReadLine();


            if (type == "order")
            {
                Console.Write("     Enter Customer Address: ");
                string address = Console.ReadLine();
                Console.Write("     Enter datails of yor order");
                string? datails = Console.ReadLine();
                Console.Write("     enter Weight of order");
                int Weight = int.Parse(Console.ReadLine());
                Console.Write("     Enter Type Shipment (0=STANDART, 1=FAST DELIVERY, 2=DELIVER IMMEDIATELY): ");
                DO.TypeOfOrder typeOfOrder = (DO.TypeOfOrder)int.Parse(Console.ReadLine());

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
                    OrderDate = DateTime.Now
                };
                return newOrder;

            }

            if (type == "courier")
            {
                int id = 0;//במקרה עדכון בין כך לא נשתמש במשתנה הזה, ובמקרה יצירה ניקח את הערך שהוזן ע"י המשתמש
                if (action is "create")
                {
                    Console.Write("     Enter ID: ");
                    id = int.Parse(Console.ReadLine());
                }

                Console.Write("     Enter Email: ");
                string email = Console.ReadLine();

                Console.Write("     Enter Password: ");
                string password = Console.ReadLine();

                Console.Write("     Enter Max Distance Delivery (in km): ");
                double maxDistanceDelivery = double.Parse(Console.ReadLine());

                Console.Write("     Enter Type Shipment (0=CAR, 1=MOTORCYCLE, 2=BICYCLE, 3=FOOT): ");
                DO.TheTypeShipment typeShipment = (DO.TheTypeShipment)int.Parse(Console.ReadLine());

                Courier newCourier = new DO.Courier
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
                return newCourier;

            }

            else
                return null;

        }


        private static void SetCourier()
        {
            Console.WriteLine("SetCourier method called.");
            int choiche;
            do
            {
                Console.WriteLine(
                "   to exit press 0\n" +
                "   to create courier press 1\n" +
                "   to read courier press 2\n" +
                "   to read all couriers press 3\n" +
                "   to update press 4\n" +
                "   to delete courier press 5\n" +
                "   to delete all couriers press 6\n");


                choiche = int.Parse(Console.ReadLine());
                switch (choiche)
                {
                    case 0:
                        break;
                    case 1:
                        Console.WriteLine("Create Courier selected.");

                        try
                        {
                            object a = DataReception("create", "courier");
                            Courier temp = (Courier)a;

                            Courier newCourier = new DO.Courier

                            {
                                Id = temp.Id,
                                Name = temp.Name,
                                Phone = temp.Phone,
                                Email = temp.Email,
                                Password = temp.Password,
                                Active = true,
                                MaxDistanceDelivery = temp.MaxDistanceDelivery,
                                TypeShipment = temp.TypeShipment,
                                WorkingSince = DateTime.Now
                            };

                            s_dalCourier.Create(newCourier);

                            Console.WriteLine("Courier created successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter numbers where required.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;

                    case 2:
                        Console.WriteLine("Read Courier selected.\n enter id of courior");
                        int idRead = int.Parse(Console.ReadLine());
                        Courier? courier = s_dalCourier?.Read(idRead);
                        if (courier is not null)
                        {
                            PrintCourierDetails(courier);
                            break;
                        }


                        Console.WriteLine("Courier not found.");


                        break;
                    case 3:
                        Console.WriteLine("Read All Couriers selected.");

                        s_dalCourier?.ReadAll().ForEach(courier =>
                        {
                            PrintCourierDetails(courier);
                        });

                        break;
                    case 4:
                        Console.WriteLine("Update Courier selected.\nenter id of courior to update");
                        int idUpdate = int.Parse(Console.ReadLine());//בקשת תז
                        Courier courierUpdate = s_dalCourier?.Read(idUpdate);//הדפסה של פרטי השליח
                        if (courierUpdate == null)//*****לא ברור לי למה המתודה של עדכון בודקת גם אם קיים כזה שליח הרי אני בודק את זה כאן כבר*****
                        {
                            Console.WriteLine("Courier not found.");
                            break;
                        }
                        else
                        {
                            PrintCourierDetails(courierUpdate);
                        }

                        try
                        {
                            //בקשת פרטים חדשים לעדכון לא כולל תז


                            object a = DataReception("update", "courier");
                            Courier temp = (Courier)a;



                            Courier newCourier = new DO.Courier
                            {
                                Id = courierUpdate.Id,
                                Name = temp.Name,
                                Phone = temp.Phone,
                                Email = temp.Email,
                                Password = temp.Password,
                                Active = true,
                                MaxDistanceDelivery = temp.MaxDistanceDelivery,
                                TypeShipment = temp.TypeShipment,
                                WorkingSince = courierUpdate.WorkingSince
                            };
                            s_dalCourier.Update(newCourier);
                            Console.WriteLine("Courier updated successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter numbers where required.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }

                        break;
                    case 5:
                        Console.WriteLine("Delete Courier selected.");

                        try
                        {
                            Console.Write("Enter ID to delete: ");
                            int idToDelete = int.Parse(Console.ReadLine());
                            s_dalCourier?.Delete(idToDelete);
                            Console.WriteLine("Courier deleted successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter a valid number for ID.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }

                        break;
                    case 6:
                        Console.WriteLine("Delete All Couriers selected.");
                        s_dalCourier?.DeleteAll();
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            } while (choiche != 0);


        }
        private static void SetOrder()
        {
            Console.WriteLine("SetOrder method called.");
            int choiche;
            do
            {
                Console.WriteLine(
                "   to exit press 0\n" +
                "   to create order press 1\n" +
                "   to read order press 2\n" +
                "   to read all orders press 3\n" +
                "   to update press 4\n" +
                "   to delete order press 5\n" +
                "   to delete all orders press 6\n");
                choiche = int.Parse(Console.ReadLine());
                switch (choiche)
                {
                    case 0:
                        break;
                    case 1:
                        object a = DataReception("create", "order");
                        Order temp = (Order)a;

                        Order newOrder = new DO.Order
                        {
                            Id = 0, // ID will be set by DAL
                            Name = temp.Name,
                            Phone = temp.Phone,
                            Addres = temp.Addres,
                            Details = temp.Details,
                            Weight = temp.Weight,
                            TypeOfOrder = temp.TypeOfOrder,
                            OrderStatus = 0,
                            Latitude = 0.0, // Placeholder, should be set appropriately
                            Longitude = 0.0, // Placeholder, should be set appropriately
                            OrderDate = DateTime.Now
                        };
                        s_dalOrder.Create(newOrder);
                        Console.WriteLine("Order created successfully!");
                        break;
                    case 2:
                        Console.WriteLine("Read Order selected.");
                        Console.WriteLine(" enter id of order");
                        int idRead = int.Parse(Console.ReadLine());
                        Order order = s_dalOrder?.Read(idRead);
                        if (order is not null)
                        {
                            PrintOrderDetails(order);
                        }
                        else
                        {
                            Console.WriteLine("Order not found.");
                        }

                        break;
                    case 3:
                        Console.WriteLine("Read All Orders selected.");
                        s_dalOrder?.ReadAll().ForEach(order =>
                        {
                            PrintOrderDetails(order);
                        });

                        break;
                    case 4:
                        Console.WriteLine("Update Order selected.");
                        int idUpdate = int.Parse(Console.ReadLine());//בקשת תז
                        Order orderUpdate = s_dalOrder?.Read(idUpdate);
                        if (orderUpdate is null)//*****לא ברור לי למה המתודה של עדכון בודקת גם אם קיים כזה שליח הרי אני בודק את זה כאן כבר*****
                        {
                            Console.WriteLine("Order not found.");
                            break;
                        }
                        else
                        {
                            PrintOrderDetails(orderUpdate);
                        }
                        try
                        {
                            a = DataReception("update", "order");
                            Order temp1 = (Order)a;

                            Order newOrder1 = new DO.Order
                            {
                                Id = orderUpdate.Id,
                                Name = temp1.Name,
                                Phone = temp1.Phone,
                                Addres = temp1.Addres,
                                Details = temp1.Details,
                                Weight = temp1.Weight,
                                TypeOfOrder = temp1.TypeOfOrder,
                                OrderStatus = orderUpdate.OrderStatus,//נדרש טיפול בחלק הזה
                                Latitude = orderUpdate.Latitude,
                                Longitude = orderUpdate.Longitude,
                                OrderDate = orderUpdate.OrderDate,
                            };


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;
                    case 5:
                        Console.WriteLine("Delete Order selected.");

                        try
                        {
                            Console.Write("Enter ID to delete: ");
                            int idToDelete = int.Parse(Console.ReadLine());
                            s_dalOrder?.Delete(idToDelete);
                            Console.WriteLine("Order deleted successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter a valid number for ID.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;
                    case 6:
                        Console.WriteLine("Delete All Orders selected.");
                        s_dalOrder?.DeleteAll();
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            } while (choiche != 0);
        }

        private static void SetDelivery()
        {
            Console.WriteLine("SetDelivery method called.");
            int choiche;
            do
            {
                Console.WriteLine(
                "   to exit press 0\n" +
                "   to create delivery press 1\n" +
                "   to read delivery press 2\n" +
                "   to read all deliveries press 3\n" +
                "   to update press 4\n" +
                "   to delete delivery press 5\n" +
                "   to delete all deliveries press 6\n");
                choiche = int.Parse(Console.ReadLine());
                switch (choiche)
                {
                    case 0:
                        break;
                    case 1:
                        Console.WriteLine("Create Delivery selected.");
                        try
                        {
                            object a = DataReception("create", "delivery");
                            Delivery temp = (Delivery)a;
                            Delivery newDelivery = new DO.Delivery
                            {
                                Id = 0, // ID will be set by DAL
                                OrderId = temp.OrderId,
                                CourierId = temp.CourierId,
                                TypeOfOrder = temp.TypeOfOrder,
                                OrderDate = DateTime.Now,
                                ActualDistance = temp.ActualDistance,
                                EndDelivery = temp.EndDelivery,
                                TimeEndDelivery = temp.TimeEndDelivery
                            };
                            s_dalDelivery.Create(newDelivery);
                            Console.WriteLine("Delivery created successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter numbers where required.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;
                    case 2:
                        Console.WriteLine("Read Delivery selected.");
                        Console.WriteLine(" enter id of delivery");
                        int idRead = int.Parse(Console.ReadLine());
                        Delivery delivery = s_dalDelivery?.Read(idRead);
                        if (delivery is not null)
                        {
                            PrintDeliveryDetails(delivery);
                            break;
                        }

                        Console.WriteLine("Delivery not found.");

                        break;
                    case 3:
                        Console.WriteLine("Read All Deliveries selected.");

                        s_dalDelivery?.ReadAll().ForEach(delivery =>
                        {
                            PrintDeliveryDetails(delivery);
                        });
                        break;
                    case 4:
                        Console.WriteLine("Update Delivery selected.");
                        int idUpdate = int.Parse(Console.ReadLine());//בקשת תז
                        Delivery deliveryUpdate = s_dalDelivery?.Read(idUpdate);
                        if (deliveryUpdate is null)//*****לא ברור לי למה המתודה של עדכון בודקת גם אם קיים כזה שליח הרי אני בודק את זה כאן כבר*****
                        {
                            Console.WriteLine("Delivery not found.");
                            break;
                        }
                        else
                        {
                            PrintDeliveryDetails(deliveryUpdate);
                        }
                        try
                        {
                            object a1 = DataReception("update", "delivery");
                            Delivery temp1 = (Delivery)a1;
                            Delivery newDelivery1 = new DO.Delivery
                            {
                                Id = deliveryUpdate.Id,
                                OrderId = temp1.OrderId,
                                CourierId = temp1.CourierId,
                                TypeOfOrder = temp1.TypeOfOrder,
                                OrderDate = deliveryUpdate.OrderDate,
                                ActualDistance = temp1.ActualDistance,
                                EndDelivery = temp1.EndDelivery,
                                TimeEndDelivery = temp1.TimeEndDelivery
                            };
                            s_dalDelivery.Update(newDelivery1);
                            Console.WriteLine("Delivery updated successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;
                    case 5:
                        Console.WriteLine("Delete Delivery selected.");


                        try
                        {
                            Console.Write("Enter ID to delete: ");
                            int idToDelete = int.Parse(Console.ReadLine());
                            s_dalDelivery?.Delete(idToDelete);
                            Console.WriteLine("Delivery deleted successfully!");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid input format. Please enter a valid number for ID.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        break;
                    case 6:
                        Console.WriteLine("Delete All Deliveries selected.");
                        s_dalDelivery?.DeleteAll();
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
            MainMenu();
        }
        static void MainMenu()
        {
            int choice;
            do
            {

                Console.WriteLine("Main Menu \n" +
                    " to exit press 0\n" +
                    "   to set courier press 1\n" +
                    "  to set order press 2\n" +
                    "  to set delivery press 3\n" +
                    "       enter your choice: ");

                //string? input = Console.ReadLine();
                choice = int.Parse(Console.ReadLine());

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
