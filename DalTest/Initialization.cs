namespace DalTest;
using DalApi;
using DO;
using System.Runtime.CompilerServices;
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
    private static readonly object[][] Addresses =
    {
         [ " רבי עקיבא 50, בני ברק", "32.0876045", "34.8278091", "912", "898"],
         [ " נחמיה 8, בני ברק", "32.0793696", "34.8352634", "3349", "2361"],
         [ " בן גוריון 25, גבעת שמואל", "32.0952663", "34.8220752", "421", "308"],
         [ " דסלר 4, בני ברק", "32.0818569", "34.8315636", "2697", "1740"],
         [ " סוקולוב 30, בני ברק", "32.0889927", "34.8341474", "1588", "1468"],
         [ " הרב קוק 18, בני ברק", "32.0865247", "34.8262516", "1367", "1047"],
         [ " הירקון 5, בני ברק", "32.0965116", "34.822183", "327", "437"],
         [ " ביאליק 40, רמת גן", "32.0828089", "34.8147672", "1993", "1853"],
         [ " הרצל 60, רמת גן", "32.0850858", "34.8155354", "1949", "1528"],
         [ " אבא הלל 15, רמת גן", "32.085569", "34.803969", "2578", "2267"],
         [ " ז'בוטינסקי 55, רמת גן", "32.0854799", "34.8101674", "1772", "1697"],
         [ " חזון איש 20, בני ברק", "32.0834133", "34.8356933", "2466", "1975"],
         [ " קריניצי 20, רמת גן", "32.0797548", "34.8172104", "1907", "1833"],
         [ " הראה 80, רמת גן", "32.0817288", "34.8254736", "1741", "1599"],
         [ " ארלוזורוב 10, רמת גן", "32.0794038", "34.8138358", "2233", "2159"],
         [ " שדרות ירושלים 30, רמת גן", "32.0847242", "34.8297262", "1349", "1335"],
         [ " בן גוריון 100, רמת גן", "32.0865566", "34.8216913", "923", "842"],
         [ " נגבה 25, רמת גן", "32.070547", "34.8249663", "3026", "2884"],
         [ " הירדן 40, רמת גן", "32.066254", "34.8281993", "3809", "3520"],
         [ " אלוף שדה 15, רמת גן", "32.0594879", "34.8249646", "4698", "4569"],
         [ " רוקח 10, רמת גן", "32.0876832", "34.8111166", "2103", "1626"],
         [ " תרצה 8, רמת גן", "32.0752286", "34.8280531", "2913", "2520"],
         [ " ז'בוטינסקי 100, בני ברק", "32.0917476", "34.8321991", "1109", "1095"],
         [ " המעגל 12, רמת גן", "32.0829031", "34.8124271", "2213", "1935"],
         [ " ויצמן 20, גבעתיים", "32.0737141", "34.8083944", "3465", "3131"],
         [ " כצנלסון 50, גבעתיים", "32.0750742", "34.8076031", "4016", "3061"],
         [ " שיינקין 15, גבעתיים", "32.0748186", "34.8107739", "2974", "2884"],
         [ " רמב''ם 10, גבעתיים", "32.0700634", "34.8038533", "4371", "3933"],
         [ " גורדון 5, גבעתיים", "32.076085", "34.807312", "3383", "2953"],
         [ " סירקין 12, גבעתיים", "32.0775637", "34.814622", "2702", "2297"],
         [ " בורוכוב 8, גבעתיים", "32.0775935", "34.8029331", "3526", "3065"],
         [ " המאבק 25, גבעתיים", "32.0638384", "34.8099736", "4389", "4217"],
         [ " עליית הנוער 10, גבעתיים", "32.0770387", "34.8020075", "3997", "3232"],
         [ " ירושלים 15, בני ברק", "32.0857512", "34.8300664", "1231", "1217"],
         [ " דרך השלום 40, גבעתיים", "32.0694025", "34.8022553", "4712", "4245"],
         [ " דיזנגוף 100, תל אביב", "32.0793963", "34.7740011", "6268", "5571"],
         [ " בן יהודה 50, תל אביב", "32.0780449", "34.7688332", "7231", "6157"],
         [ " אבן גבירול 70, תל אביב", "32.0806057", "34.7815385", "5393", "4886"],
         [ " רוטשילד 45, תל אביב", "32.0642206", "34.7747952", "6615", "6204"],
         [ " אלנבי 80, תל אביב", "32.0678218", "34.7710389", "7120", "6334"],
         [ " המלך ג'ורג' 30, תל אביב", "32.0722931", "34.7741392", "6502", "5666"],
         [ " שינקין 20, תל אביב", "32.0693286", "34.7725233", "7396", "6205"],
         [ " דרך מנחם בגין 120, תל אביב", "32.071223", "34.7898718", "5442", "4494"],
         [ " המסגר 15, תל אביב", "32.0628255", "34.7850239", "7517", "5476"],
         [ " הרב שך 10, בני ברק", "32.0864414", "34.8357055", "2044", "1700"],
         [ " יגאל אלון 60, תל אביב", "32.0619207", "34.7927464", "8326", "5292"],
         [ " דרך ההגנה 40, תל אביב", "32.0540338", "34.7869603", "9038", "6506"],
         [ " הירקון 150, תל אביב", "32.0837404", "34.7693911", "9225", "6139"],
         [ " פרישמן 10, תל אביב", "32.0798034", "34.7688166", "6705", "6009"],
         [ " בוגרשוב 25, תל אביב", "32.0769875", "34.7698549", "10607", "5951"],
         [ " יפת 80, יפו (תל אביב)", "32.0464433", "34.7523983", "12889", "8511"],
         [ " דרך קיבוץ גלויות 30, תל אביב", "32.0517973", "34.7680502", "13038", "7756"],
         [ " לה גווארדיה 20, תל אביב", "32.059112", "34.788742", "7291", "5820"],
         [ " ארלוזורוב 90, תל אביב", "32.0854282", "34.7806379", "5603", "4724"],
         [ " כהנמן 60, בני ברק", "32.0845043", "34.8399999", "2550", "2213"],
         [ " נמיר 50, תל אביב", "32.0850486", "34.7953535", "3666", "3252"],
         [ " חיים עוזר 10, פתח תקווה", "32.0890587", "34.8861437", "7837", "6418"],
         [ " רוטשילד 50, פתח תקווה", "32.0903978", "34.8804676", "7675", "5723"],
         [ " ז'בוטינסקי 80, פתח תקווה", "32.091785", "34.8641828", "5401", "4110"],
         [ " העצמאות 20, פתח תקווה", "32.074777", "34.880136", "11278", "7102"],
         [ " אורלוב 30, פתח תקווה", "32.0934753", "34.8820009", "7199", "5893"],
         [ " שטמפפר 15, פתח תקווה", "32.0901497", "34.8855149", "7737", "6262"],
         [ " עין גנים 40, פתח תקווה", "32.086933", "34.894794", "8738", "7412"],
         [ " סלומון 12, פתח תקווה", "32.0846599", "34.8805418", "8231", "6309"],
         [ " פינסקר 8, פתח תקווה", "32.0907496", "34.8845311", "7920", "6310"],
         [ " אהרונוביץ' 12, בני ברק", "32.090832", "34.8386345", "2020", "1982"],
         [ " גיסין 25, פתח תקווה", "32.0976387", "34.8797159", "6395", "5893"],
         [ " סוקולוב 50, חולון", "32.0227089", "34.7745575", "11404", "10429"],
         [ " שנקר 20, חולון", "32.0261601", "34.7765943", "11279", "10237"],
         [ " דב הוז 30, חולון", "32.0236133", "34.7667955", "11864", "10636"],
         [ " שדרות קוגל 15, חולון", "32.0257767", "34.774554", "11779", "10077"],
         [ " הופיין 25, חולון", "32.0141212", "34.7685729", "12662", "11698"],
         [ " בלפור 40, בת ים", "32.0261767", "34.7449781", "14614", "11734"],
         [ " יוספטל 60, בת ים", "32.0165585", "34.7464493", "16092", "12857"],
         [ " העצמאות 30, בת ים", "32.0231832", "34.7465895", "14429", "11887"],
         [ " רוטשילד 20, בת ים", "32.026609", "34.7454099", "14696", "11652"],
         [ " השומר 5, בני ברק", "32.0815759", "34.8217171", "1970", "1509"],
         [ " אנילביץ' 10, בת ים", "32.0218369", "34.7517549", "13880", "12175"],
         [ " ז'בוטינסקי 40, ראשון לציון", "32.0246214", "34.7821867", "11871", "10753"],
         [ " הרצל 60, ראשון לציון", "31.9656402", "34.8025163", "19145", "18843"],
         [ " רוטשילד 30, ראשון לציון", "31.9641474", "34.803856", "21869", "19060"],
         [ " משה דיין 20, ראשון לציון", "32.0009662", "34.7665794", "16891", "13088"],
         [ " לוי אשכול 15, ראשון לציון", "31.9754762", "34.7770157", "20073", "16880"],
         [ " סוקולוב 40, הרצליה", "32.1669612", "34.844759", "16277", "9419"],
         [ " הרב קוק 20, הרצליה", "32.161374", "34.8408944", "8778", "8642"],
         [ " בן גוריון 30, הרצליה", "32.1613286", "34.8423012", "13900", "8531"],
         [ " שבעת הכוכבים 10, הרצליה", "32.1636245", "34.8243901", "12923", "9945"],
         [ " עזרא 25, בני ברק", "32.0782597", "34.8377103", "3640", "2686"],
         [ " אוסישקין 50, רמת השרון", "32.1422739", "34.8434807", "7103", "7016"],
         [ " סוקולוב 20, רמת השרון", "32.1398478", "34.8362248", "6149", "6062"],
         [ " ביאליק 15, רמת השרון", "32.137793", "34.840278", "6589", "6503"],
         [ " לוי אשכול 30, קריית אונו", "32.0661125", "34.8604378", "8484", "5571"],
         [ " שלמה המלך 20, קריית אונו", "32.0544159", "34.8603455", "8697", "7055"],
         [ " העצמאות 15, יהוד", "32.0745531", "34.8811245", "11185", "7358"],
         [ " ויצמן 10, יהוד", "32.0300273", "34.8924598", "18607", "11744"],
         [ " העצמאות 40, אור יהודה", "32.0289963", "34.8655017", "12632", "9877"],
         [ " אליהו סעדון 20, אור יהודה", "32.0255384", "34.8549182", "12238", "9905"]
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
        // המרת המערך לרשימה כדי לאפשר הסרה
        var availableAddresses = Addresses.ToList();

        for (int i = 0; i < 50 && availableAddresses.Count > 0; i++)
        {
            double storeLat = s_dal?.Config.Latitude ?? throw new InvalidOperationException("Store latitude is not set.");
            double storeLng = s_dal?.Config.Longitude ?? throw new InvalidOperationException("Store longitude is not set.");

            int addressIndex = s_rand.Next(0, availableAddresses.Count);
            var address = availableAddresses[addressIndex];

            // הסרת הכתובת מהרשימה כדי שלא תיבחר שוב
            availableAddresses.RemoveAt(addressIndex);

            s_dal?.Order.Create(new()
            {
                Id = 0,
                TypeOfOrder = (TypeOfOrder)s_rand.Next(0, 2),
                Phone = "0" + s_rand.Next(500000000, 599999999).ToString(),
                Addres = (string)address[0],
                Latitude = double.Parse((string)address[1]),
                Longitude = double.Parse((string)address[2]),
                Name = "Customer" + i,
                Weight = s_rand.Next(1, 15),
                Details = "Order details for order " + i,
                OrderDate = s_dal.Config.Clock.AddDays(-s_rand.Next(0, 3)),
                DistanceKm = s_getDistance(storeLat, storeLng, double.Parse((string)address[1]), double.Parse((string)address[2])),
                OrderStatus = OrderStatus.OPEN,
                DistanceKmRoad = double.Parse((string)address[3]),
                DistanceKmWalk = double.Parse((string)address[4])
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
        static DateTime? getTimeEndDelivery(DateTime orderDate, TimeSpan duration, EndDelivery? endDelivery)
        {
            return endDelivery switch
            {
                EndDelivery.DELIVERED => orderDate.Add(duration),
                EndDelivery.REFUSED => orderDate.Add(duration).AddMinutes(s_rand.Next(5, 31)),
                EndDelivery.CONCELLED => orderDate,
                EndDelivery.NOTFOUND => orderDate.Add(duration).AddMinutes(s_rand.Next(10, 61)),
                EndDelivery.FAILED => orderDate.Add(duration).AddMinutes(s_rand.Next(15, 91)),
                _ => null,
            };
        }


        DateTime? timeEndDelivery;
        DateTime globalMaxTime = DateTime.MinValue;
        DateTime maxTime = DateTime.MinValue;

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
            Courier.MaxDistanceDelivery >= randomOrder.DistanceKm)//לבדוק את המרחק של ההזמנה
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
            DateTime orderDate;

            var delivery = s_dal!.Delivery.ReadAll(d => d.OrderId == randomOrder.Id).ToList();//מציאת משלוחים על ההזמנה הזו


            maxTime = (maxTime > globalMaxTime) ? maxTime : globalMaxTime;//שמירת הזמן המקסימלי
            // בדיקה אם יש בכלל משלוחים קודמים
            if (delivery.Count > 0)
            {
                DateTime maxEndTime = delivery.Max(d => d.TimeEndDelivery) ?? DateTime.MinValue;
                globalMaxTime = maxEndTime;
                // קביעת הזמן החדש לזמן הסיום האחרון + 10 דק 
                orderDate = maxEndTime.AddMinutes(10);
            }
            else
            {
                orderDate = (DateTime)(s_dal!.Config!.Clock.AddHours(-s_rand.Next(0, duration.Hours)));
            }



            EndDelivery getEndDelivery = (EndDelivery)s_rand.Next(0, 6);

            timeEndDelivery = getTimeEndDelivery(orderDate, duration, getEndDelivery);

            s_dal?.Order.Update(randomOrder with
            {
                OrderStatus = getEndDelivery switch
                {
                    EndDelivery.DELIVERED => OrderStatus.COMPLETED,
                    EndDelivery.REFUSED => OrderStatus.REFUSED,
                    EndDelivery.CONCELLED => OrderStatus.CONCELLED,
                    EndDelivery.FAILED => OrderStatus.OPEN,
                    EndDelivery.NOTFOUND => OrderStatus.OPEN,
                    _ => randomOrder.OrderStatus // keep current status
                }
            });

            EndDelivery? endDelivery = (int)getEndDelivery > 4 ? null : getEndDelivery;


            s_dal?.Delivery!.Create(new()
            {
                Id = 0,
                OrderId = randomOrder.Id,
                TypeShipment = selectedCourier.TypeShipment,
                ActualDistance = getActualDistance,
                CourierId = selectedCourier.Id,
                OrderDate = orderDate,
                EndDelivery = endDelivery,
                TimeEndDelivery = timeEndDelivery
            });
        }
        s_dal!.Config!.Clock = maxTime;
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
