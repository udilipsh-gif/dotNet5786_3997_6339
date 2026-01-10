namespace DalTest;
using DalApi;
using DO;
using System.Xml.Linq;


/// <summary>
/// Static class for initializing the data store with sample data for couriers, orders, deliveries, and configuration settings.
/// </summary>
public static class Initialization
{
    /// <summary>
    /// Minimum value for generating random courier IDs.
    /// </summary>
    const int MIN_ID = 200000000;

    /// <summary>
    /// Maximum value for generating random courier IDs.
    /// </summary>
    const int MAX_ID = 400000000;


    /// <summary>
    /// Random number generator for creating randomized test data.
    /// </summary>
    private static readonly Random s_rand = new();

    private static IDal? s_dal; //stage 2


    /// <summary>
    /// Represents a collection of predefined addresses with associated geographic coordinates and distance metrics.
    /// Each entry contains: address string, latitude, longitude, distance in km, walking distance, and road distance.
    /// </summary>
    private static object[][] s_addresses =
    {
      ["הירקון 50 תל אביב", 32.09487, 34.825308, 0.45, 0.5625, 0.7425],
      ["שד' התמרים 12 רמת גן", 32.088052, 34.820129, 0.49, 0.6125, 0.8085],
      ["המסגר 20 רמת גן", 32.086789, 34.819234, 0.65, 0.8125, 1.0725],
      ["ראשון לציון 35 רמת גן", 32.085432, 34.818901, 0.8, 1.0, 1.32],
      ["שד' בן גוריון 55 רמת גן", 32.084567, 34.820123, 0.86, 1.075, 1.419],
      ["הירקון 75 תל אביב", 32.098123, 34.828765, 0.93, 1.1625, 1.5345],
      ["הרצל 40 רמת גן", 32.083456, 34.819012, 1.01, 1.2625, 1.6665],
      ["השלום 50 רמת גן", 32.082345, 34.819876, 1.11, 1.3875, 1.8315],
      ["שד' יצחק רבין 25 רמת גן", 32.082345, 34.818345, 1.14, 1.425, 1.881],
      ["הרצל 15 רמת גן", 32.0810786, 34.8180973, 1.28, 1.6, 2.112],
      ["שד' בן גוריון 40 רמת גן", 32.080456, 34.821233, 1.31, 1.6375, 2.1615],
      ["ראשון לציון 50 בני ברק", 32.080234, 34.824345, 1.35, 1.6875, 2.2275],
      ["שד' ההסתדרות 14 בני ברק", 32.080123, 34.825678, 1.39, 1.7375, 2.2935],
      ["שד' רוקח 5 רמת גן", 32.079876, 34.817654, 1.42, 1.775, 2.343],
      ["שד' גולדה מאיר 50 תל אביב", 32.101234, 34.832345, 1.42, 1.775, 2.343],
      ["העצמאות 45 בני ברק", 32.078901, 34.823456, 1.49, 1.8625, 2.4585],
      ["שד' התמרים 20 בני ברק", 32.079012, 34.824567, 1.49, 1.8625, 2.4585],
      ["הבנים 20 בני ברק", 32.077654, 34.823789, 1.63, 2.0375, 2.6895],
      ["שד' בן צבי 18 בני ברק", 32.076543, 34.821234, 1.74, 2.175, 2.871],
      ["השלום 80 בני ברק", 32.075123, 34.825678, 1.93, 2.4125, 3.1845],
      ["שד' ירושלים 12 בני ברק", 32.07491, 34.824194, 1.94, 2.425, 3.201],
      ["שד' ירושלים 45 רמת גן", 32.073393, 34.822611, 2.09, 2.6125, 3.4485],
      ["הירקון 85 בני ברק", 32.073456, 34.823901, 2.09, 2.6125, 3.4485],
      ["הירקון 100 תל אביב", 32.109456, 34.831234, 2.12, 2.65, 3.498],
      ["הירקון 30 בני ברק", 32.071234, 34.822345, 2.33, 2.9125, 3.8445],
      ["ויצמן 5 גבעתיים", 32.074703, 34.807631, 2.36, 2.95, 3.894],
      ["הבנים 9 רמת גן", 32.065432, 34.812345, 3.1, 3.875, 5.115],
      ["ויצמן 22 תל אביב", 32.08197, 34.787879, 3.39, 4.2375, 5.5935],
      ["שד' יצחק רבין 7 פתח תקווה", 32.076664, 34.859017, 3.91, 4.8875, 6.4515],
      ["רמת חן 30 רמת גן", 32.054005, 34.815657, 4.29, 5.3625, 7.0785],
      ["סוקלוב 30 תל אביב", 32.087445, 34.776397, 4.3, 5.375, 7.095],
      ["ויאצה 15 תל אביב", 32.075678, 34.77789, 4.52, 5.65, 7.458],
      ["שד' ירושלים 30 תל אביב", 32.072345, 34.779012, 4.59, 5.7375, 7.5735],
      ["הבנים 35 תל אביב", 32.073567, 34.778123, 4.6, 5.75, 7.59],
      ["שד' התמרים 35 תל אביב", 32.069234, 34.780123, 4.68, 5.85, 7.722],
      ["הרצל 25 תל אביב", 32.068901, 34.779345, 4.76, 5.95, 7.854],
      ["השלום 18 תל אביב", 32.070789, 34.777345, 4.81, 6.0125, 7.9365],
      ["ריינס 5 תל אביב", 32.077475, 34.773417, 4.84, 6.05, 7.986],
      ["המסגר 8 תל אביב", 32.071567, 34.776234, 4.86, 6.075, 8.019],
      ["שד' יצחק רבין 10 תל אביב", 32.067345, 34.778901, 4.89, 6.1125, 8.0685],
      ["הרצל 15 תל אביב", 32.06682, 34.777819, 5.01, 6.2625, 8.2665],
      ["ראשון לציון 20 פתח תקווה", 32.104233, 34.874897, 5.18, 6.475, 8.547],
      ["העצמאות 60 פתח תקווה", 32.073523, 34.872251, 5.19, 6.4875, 8.5635],
      ["העצמאות 22 תל אביב", 32.065789, 34.775432, 5.26, 6.575, 8.679],
      ["בני ברק 8 תל אביב-יפו", 32.057181, 34.778104, 5.66, 7.075, 9.339],
      ["ויצמן 40 פתח תקווה", 32.090123, 34.885678, 6.03, 7.5375, 9.9495],
      ["שד' גולדה מאיר 25 פתח תקווה", 32.098765, 34.885432, 6.04, 7.55, 9.966],
      ["בני ברק 15 פתח תקווה", 32.092345, 34.887654, 6.21, 7.7625, 10.2465],
      ["המסגר 12 פתח תקווה", 32.085678, 34.890123, 6.48, 8.1, 10.692],
      ["השלום 25 פתח תקווה", 32.088765, 34.892345, 6.66, 8.325, 10.989],
      ["השלום 60 פתח תקווה", 32.092456, 34.893456, 6.76, 8.45, 11.154],
      ["שד' ההסתדרות 30 פתח תקווה", 32.095678, 34.895678, 6.98, 8.725, 11.517],
      ["הרצל 55 פתח תקווה", 32.094123, 34.896789, 7.07, 8.8375, 11.6655],
      ["שד' רוקח 10 פתח תקווה", 32.089923, 34.899951, 7.37, 9.2125, 12.1605]
    };

    /// <summary>
    /// Validates whether the courier's shipment type is compatible with the order type.
    /// </summary>
    /// <param name="courier">The type of shipment the courier uses (car, motorcycle, bike, or foot).</param>
    /// <param name="order">The type of order (standard, fast delivery, or deliver immediately).</param>
    /// <returns>True if the shipment type matches the order requirements; otherwise, false.</returns>
    public static bool MatchTypeShipmentAndOrder(TheTypeShipment courier, TypeOfOrder order)
    {
        return order switch
        {
            TypeOfOrder.STANDART => courier == TheTypeShipment.CAR || courier == TheTypeShipment.MOTORCYCLE,
            TypeOfOrder.FAST_DELIVERY => courier == TheTypeShipment.MOTORCYCLE,
            TypeOfOrder.DELIVER_IMMEDIATELY => courier == TheTypeShipment.FOOT,
            _ => false
        };
    }

    /// <summary>
    /// Creates and initializes a collection of 20 couriers with randomized data.
    /// </summary>
    /// <remarks>
    /// This method generates courier records with random attributes including:
    /// <list type="bullet">
    /// <item><description>Unique IDs validated to avoid duplicates.</description></item>
    /// <item><description>Names from a predefined list.</description></item>
    /// <item><description>Email addresses generated from names.</description></item>
    /// <item><description>Random phone numbers and passwords.</description></item>
    /// <item><description>Active status (80% chance of being active).</description></item>
    /// <item><description>Shipment type (car, motorcycle, bike, or foot).</description></item>
    /// <item><description>Working since date (up to 1 year ago).</description></item>
    /// <item><description>Maximum delivery distance based on shipment type.</description></item>
    /// </list>
    /// </remarks>
    private static void CreateCourier()
    {
        string[] courierNames =
        {
                "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof",
                "Nadav Katz", "Rina Cohen", "Moshe Bar", "Rachel Adler", "Itay Mizrahi", "Noa Ben-David",
                "Yaniv Shapiro", "Maya Rosen", "Omer Azulay", "Lior Kaplan", "Tamar Weiss", "Ariel Gold",
                "Galit Peretz", "Eden Harari"
            };

        /// <summary>
        /// Generates a unique courier ID that doesn't already exist in the system.
        /// </summary>
        /// <returns>A unique integer ID between MIN_ID and MAX_ID.</returns>
        static int getUniqueId()
        {
            int id;
            do
                id = s_rand.Next(MIN_ID, MAX_ID);
            while (s_dal?.Courier.Read(id) != null);
            return id;
        }

        /// <summary>
        /// Calculates the maximum delivery distance for a courier based on their shipment type.
        /// </summary>
        /// <param name="shipment">The type of shipment (car, motorcycle, bike, or foot).</param>
        /// <returns>
        /// A nullable double representing the maximum delivery distance in kilometers.
        /// Returns null if the distance exceeds 100 km.
        /// </returns>
        static double? getMaxDistanceDelivery(TheTypeShipment shipment)
        {
            double? distans = shipment switch
            {
                TheTypeShipment.CAR => s_rand.Next(10, 100),
                TheTypeShipment.MOTORCYCLE => s_rand.Next(2, 25),
                TheTypeShipment.BIKE => s_rand.Next(1, 5),
                TheTypeShipment.FOOT => s_rand.NextDouble() * (3.5 - 0.5) + 0.5,
                _ => null
            };
            return distans > 100 ? null : distans;
        }

        foreach (var name in courierNames)
        {
            var typeShipment = (TheTypeShipment)s_rand.Next(0, 4);

            s_dal?.Courier!.Create(new()
            {
                Id = getUniqueId(),
                Name = name,
                Email = name.Replace(" ", ".").ToLower() + "@courier.com",
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Password = "Pass#" + s_rand.Next(100000, 500000).ToString(),
                Active = s_rand.Next(0, 5) != 0 ? true : false,
                TypeShipment = typeShipment,
                WorkingSince = s_dal.Config.Clock.AddDays(-s_rand.Next(5, 366)),
                MaxDistanceDelivery = getMaxDistanceDelivery(typeShipment)
            });
        }
    }

    /// <summary>
    /// Creates and initializes a collection of 50 orders with randomized data.
    /// </summary>
    /// <remarks>
    /// This method generates 50 orders with randomized properties such as order type, phone number, 
    /// address, geographic coordinates, customer name, weight, and order details. The orders are created using the
    /// data access layer and are assigned timestamps within the last year.
    /// Order statuses are distributed as follows:
    /// <list type="bullet">
    /// <item><description>First 20 orders: OPEN status</description></item>
    /// <item><description>Orders 21-30: DELIVERING status</description></item>
    /// <item><description>Orders 31-50: Random status (COMPLETED, REFUSED, or CANCELLED)</description></item>
    /// </list>
    /// </remarks>
    private static void CreateOrders()
    {
        //int num_of_order = 0;

        ///// <summary>
        ///// Determines the order status based on the order number.
        ///// </summary>
        ///// <param name="num_of_order">The current order number being processed.</param>
        ///// <returns>An OrderStatus value based on the order count.</returns>
        //static OrderStatus getRandomOrderStatus(int num_of_order)
        //{
        //    return num_of_order++ switch
        //    {
        //        < 20 => OrderStatus.OPEN,
        //        < 30 => OrderStatus.DELIVERING,
        //        _ => (OrderStatus)s_rand.Next(2, 5)
        //    };
        //}

        for (int i = 0; i < 50; i++)
        {
            int adressIndex = -1;
            string addres = string.Empty;

            (double Lat, double Lng)? adressCoordinates = null;
            while (adressCoordinates == null || adressCoordinates.Value.Lat == 0 || adressCoordinates.Value.Lng == 0)
            {
                adressIndex = s_rand.Next(0, s_addresses.Length);
                addres = (string)s_addresses[adressIndex][0];
                try
                {
                   // Console.WriteLine($"Getting geocoding for order {i} address: {addres}");
                    adressCoordinates = s_getGeocodingSync(addres);
                }
                catch //(Exception ex)
                {
                  // Console.WriteLine($"Error retrieving geocoding: {ex.Message}");
                }
            }
            double lat = adressCoordinates?.Lat ?? throw new DO.DalValueIsNotValid("Latitude is missing.");
            double lng = adressCoordinates?.Lng ?? throw new DO.DalValueIsNotValid("Longitude is missing.");
            double storeLat = s_dal?.Config.Latitude ?? throw new InvalidOperationException("Store latitude is not set.");
            double storeLng = s_dal?.Config.Longitude ?? throw new InvalidOperationException("Store longitude is not set.");
            double DistanceKm = s_getDistance(lat, lng, storeLat, storeLng);

                        s_dal?.Order.Create(new()
            {
                Id = 0,
                TypeOfOrder = (TypeOfOrder)s_rand.Next(0, 2),
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Addres = addres,
                Latitude = lat,
                Longitude = lng,
                Name = "Customer" + i,
                Weight = s_rand.Next(1, 21),
                Details = "Order details for order " + i,
                OrderDate = s_dal.Config.Clock.AddDays(-s_rand.Next(0, 366)),
                DistanceKm = DistanceKm,
                OrderStatus = OrderStatus.OPEN,  
            });
        }
    }

    /// <summary>
    /// Creates and assigns a batch of delivery records based on available orders and couriers.
    /// </summary>
    /// <remarks>
    /// This method generates 50 delivery records by randomly selecting orders and matching them with couriers.
    /// Orders that are not in "OPEN" status are excluded from selection. The method ensures that the
    /// courier's shipment type matches the order's requirements before assigning the delivery.
    /// The deliveries are initialized with:
    /// <list type="bullet">
    /// <item><description>Matched courier based on shipment type, active status, and delivery distance capability</description></item>
    /// <item><description>Calculated actual distance based on courier's shipment type</description></item>
    /// <item><description>Estimated delivery duration based on courier's average speed</description></item>
    /// <item><description>Random delivery end status (DELIVERED, REFUSED, CANCELLED, NOTFOUND, or FAILED)</description></item>
    /// <item><description>Updated order status based on delivery outcome</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="Exception">Thrown if no orders or no couriers are available.</exception>
    /// <exception cref="Exception">Thrown if no matched couriers are available for an order.</exception>
    private static void CreateDelivery()
    {
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
        /// Calculates the end time of a delivery based on the order date, duration, and delivery outcome.
        /// </summary>
        /// <param name="orderDate">The date the order was placed.</param>
        /// <param name="duration">The expected delivery duration.</param>
        /// <param name="endDelivery">The outcome of the delivery.</param>
        /// <returns>
        /// A DateTime representing when the delivery ended, or null if the delivery was cancelled.
        /// For refused/not found/failed deliveries, adds additional delay time.
        /// </returns>
        static DateTime? getTimeEndDelivery(DateTime orderDate, TimeSpan duration, EndDelivery endDelivery)
        {
            return endDelivery switch
            {
                EndDelivery.DELIVERED => orderDate.Add(duration),
                EndDelivery.REFUSED => orderDate.Add(duration).AddMinutes(s_rand.Next(5, 31)),
                EndDelivery.CONCELLED => null,
                EndDelivery.NOTFOUND => orderDate.Add(duration).AddMinutes(s_rand.Next(10, 61)),
                EndDelivery.FAILED => orderDate.Add(duration).AddMinutes(s_rand.Next(15, 91)),
                _ => null,
            };
        }

       

        for (int i = 0; i < 50; i++)
        {
            var list_order = s_dal?.Order?.ReadAll(o => o.OrderStatus == OrderStatus.OPEN)// קלבת ההזמנות הפתוחות בלבד, לינקיו שלב 2
            ?.ToList()
           ?? throw new DalisNotAvailable("Order");

            //var randomOrder = list_order[s_rand.Next(list_order.Count)];//הגרלת הזמנה אקראית מתוך הרשימה שלב 1
            var randomOrder = list_order[s_rand.Next(list_order.Count)];//הגרלת הזמנה אקראית מתוך הרשימה שלב 2

            //סינון שליחים לפי שלב 2 באמצעות תנאי מסנן אחד
            var list_courier = s_dal?.Courier?.ReadAll(Courier => Courier.Active == true &&
            MatchTypeShipmentAndOrder(Courier.TypeShipment, randomOrder.TypeOfOrder) &&
            Courier.MaxDistanceDelivery >= randomOrder.DistanceKm)
                 ?.ToList()
                 ?? throw new DalisNotAvailable("Courier");

            var selectedCourier = list_courier[s_rand.Next(list_courier.Count)];//בחירת שליח אקראי מתוך רשימת השליחים המסוננת
            randomOrder = randomOrder with { OrderStatus = OrderStatus.DELIVERING };//עדכון סטטוס ההזמנה 
            s_dal?.Order.Update(randomOrder);

            double? getActualDistance =
                (selectedCourier.TypeShipment is TheTypeShipment.CAR or TheTypeShipment.MOTORCYCLE)
                ? randomOrder.DistanceKmRoad
                : randomOrder.DistanceKmWalk;

            TimeSpan duration = getActualDistance.HasValue
            ? TimeSpan.FromHours(getActualDistance.Value /
            (selectedCourier.TypeShipment switch
            {
                TheTypeShipment.CAR => s_dal!.Config!.AvgSpeedCar,
                TheTypeShipment.MOTORCYCLE => s_dal!.Config!.AvgSpeedMotorcycle,
                TheTypeShipment.BIKE => s_dal!.Config!.AvgSpeedBike,
                TheTypeShipment.FOOT => s_dal!.Config!.AvgSpeedFoot,
                _ => 1.0
            }))
    : TimeSpan.FromHours(1);

            DateTime orderData = (DateTime)(s_dal!.Config!.Clock.AddHours(-s_rand.Next(0, duration.Hours)));

            EndDelivery getEndDelivery = (EndDelivery)s_rand.Next(0, 8);

            if (getEndDelivery == EndDelivery.DELIVERED)
                s_dal?.Order.Update(randomOrder with { OrderStatus = OrderStatus.COMPLETED });
            if (getEndDelivery == EndDelivery.REFUSED)
                s_dal?.Order.Update(randomOrder with { OrderStatus = OrderStatus.REFUSED });
            if (getEndDelivery == EndDelivery.CONCELLED)
                s_dal?.Order.Update(randomOrder with { OrderStatus = OrderStatus.CONCELLED });
            //הוספתי בשלב 6 את שתי התנאים הבאים
            if(getEndDelivery == EndDelivery.FAILED)
                s_dal?.Order.Update(randomOrder with { OrderStatus=OrderStatus.OPEN });
            if (getEndDelivery== EndDelivery.NOTFOUND)
                s_dal?.Order.Update(randomOrder with { OrderStatus = OrderStatus.OPEN });


            s_dal?.Delivery!.Create(new()
            {
                Id = 0,
                OrderId = randomOrder.Id,
                TypeShipment = selectedCourier.TypeShipment,
                ActualDistance = getActualDistance,
                CourierId = selectedCourier.Id,
                OrderDate = orderData,
                EndDelivery = getEndDelivery,
                TimeEndDelivery = getTimeEndDelivery(orderData, duration, getEndDelivery) ?? default
            });
        }
    }

    /// <summary>
    /// Main initialization method that sets up the entire data store with configuration, couriers, orders, and deliveries.
    /// </summary>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item><description>Retrieves the DAL instance from the Factory</description></item>
    /// <item><description>Resets all existing configuration and data</description></item>
    /// <item><description>Creates initial configuration settings</description></item>
    /// <item><description>Generates 20 sample couriers</description></item>
    /// <item><description>Generates 50 sample orders</description></item>
    /// <item><description>Generates 50 sample deliveries with matched couriers</description></item>
    /// </list>
    /// </remarks>
    public static void Do() //stage 2
    {

        //s_dal = dal ?? throw new DalErrorConfig("DAL object can not be null!"); // stage 2
        s_dal = Factory.Get; //stage 4

        Console.WriteLine("Reset Configuration values and List values...");
        s_dal.ResetDB(); // stage 2

        Console.WriteLine("Creating Courier values...");
        CreateCourier();
        Console.WriteLine("Creating Order values...");
        CreateOrders();
        Console.WriteLine("Creating Delivery values...");
        CreateDelivery();
        Console.WriteLine("Data initialization completed.");
    }


    /// <summary>
    /// Converts a street address to geographic coordinates using the Google Geocoding API.
    /// </summary>
    /// <param name="address">The street address to geocode.</param>
    /// <returns>
    /// A tuple containing the latitude and longitude coordinates if successful,
    /// or null if the geocoding fails or the address is not found.
    /// </returns>
    /// <exception cref="DO.DalValueIsNotValid">
    /// Thrown when the address is not found in Google's database (ZERO_RESULTS),
    /// or when the address is not precise enough (APPROXIMATE, RANGE_INTERPOLATED, or GEOMETRIC_CENTER location types).
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the API request fails, returns an error status, or encounters a parsing error.
    /// </exception>
    /// <remarks>
    /// This method makes a synchronous HTTP request to the Google Geocoding API.
    /// The API key is retrieved from the system configuration.
    /// The response is in XML format and parsed to extract the location coordinates.
    /// 
    /// Location type validation:
    /// - ROOFTOP: Precise address (accepted)
    /// - APPROXIMATE: City/area level (rejected - throws exception)
    /// - RANGE_INTERPOLATED: Interpolated between two points (rejected)
    /// - GEOMETRIC_CENTER: Center of an area (rejected)
    /// 
    /// Possible error scenarios:
    /// - Invalid API key (throws Exception)
    /// - Address not found (throws DalValueIsNotValid)
    /// - Imprecise address (throws DalValueIsNotValid)
    /// - Network errors (throws Exception)
    /// - Malformed XML response (throws Exception)
    /// </remarks>
    public static (double Lat, double Lng)? s_getGeocodingSync(string address)
    {
        var apiKey = s_dal?.Config.GoogleApiKey;

        string url = $"https://maps.googleapis.com/maps/api/geocode/xml?address={address}&key={apiKey}";

        using HttpClient client = new HttpClient();
        {
            client.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");
            try
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string xmlContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    XDocument doc = XDocument.Parse(xmlContent);
                    string? status = doc.Element("GeocodeResponse")?.Element("status")?.Value;

                    if (status == "ZERO_RESULTS")
                        throw new DO.DalValueIsNotValid("הכתובת לא נמצאה במאגר של גוגל.");


                    if (status == "OK")
                    {
                        var geometry = doc.Element("GeocodeResponse")?
                                             .Element("result")?
                                             .Element("geometry");
                        var locationType = geometry?.Element("location_type")?.Value;
                        if (locationType is "APPROXIMATE" or "RANGE_INTERPOLATED" or "GEOMETRIC_CENTER")
                            throw new DO.DalValueIsNotValid("The address is not precise enough.");

                        var locationElement = geometry?.Element("location");

                        if (locationElement != null)
                        {
                            double lat = double.Parse(locationElement.Element("lat")!.Value);
                            double lng = double.Parse(locationElement.Element("lng")!.Value);

                            return (lat, lng);
                        }
                        else
                        {
                            throw new Exception("Location element not found in the response.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Failed to get geocoding data.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Adderss: {address} \n Exception: {ex.Message}");
            }
        }
        return null;
    }

    /// <summary>
    /// Calculates the distance between two geographic coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">The latitude of the first point in decimal degrees.</param>
    /// <param name="lon1">The longitude of the first point in decimal degrees.</param>
    /// <param name="lat2">The latitude of the second point in decimal degrees.</param>
    /// <param name="lon2">The longitude of the second point in decimal degrees.</param>
    /// <returns>The distance between the two points in kilometers.</returns>
    /// <remarks>
    /// This method uses the Haversine formula to calculate the great-circle distance
    /// between two points on Earth's surface. The Earth's radius is assumed to be 6371 km.
    /// Implementation based on GIMINI algorithm.
    /// </remarks>
    public static double s_getDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;

        double dLat = s_toRadians(lat2 - lat1);
        double dLon = s_toRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(s_toRadians(lat1)) * Math.Cos(s_toRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    /// <summary>
    /// Calculates the distance from the store to the order's delivery location.
    /// </summary>
    /// <param name="order">The order containing the delivery location coordinates.</param>
    /// <returns>The distance from the store to the order location in kilometers.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the store's latitude or longitude is not configured in the system.
    /// </exception>
    /// <remarks>
    /// This overload retrieves the store's coordinates from the system configuration
    /// and calculates the distance to the order's delivery location.
    /// </remarks>
    public static double s_getDistance(Order order) //פונקציית העמסה למרחק מהחנות להזמנה
    {
        double storeLatitude = s_dal?.Config.Latitude ??
            throw new InvalidOperationException("Latitude is not set in configuration.");

        double storeLongitude = s_dal?.Config.Longitude ??
            throw new InvalidOperationException("Longitude is not set in configuration.");

        return s_getDistance(order.Latitude, order.Longitude, storeLatitude, storeLongitude);
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="angleIn10thofaDegree">The angle in degrees to convert.</param>
    /// <returns>The angle converted to radians.</returns>
    /// <remarks>
    /// This is a helper method used by the Haversine distance calculation.
    /// </remarks>
    private static double s_toRadians(double angleIn10thofaDegree)
    {
        return (angleIn10thofaDegree * Math.PI) / 180;
    }

}
