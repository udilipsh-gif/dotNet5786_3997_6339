using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Helpers;

/// <summary>
/// Service for Google Maps API integration - routes and static map images.
/// Provides methods for geocoding, distance calculations, and route retrieval
/// using the Google Maps Platform APIs.
/// </summary>
public static class GoogleMapsService
{
    /// <summary>
    /// User agent string for API requests.
    /// </summary>
    private const string UserAgent = "dotNet5786_3997_6339";

    /// <summary>
    /// Shared HTTP client instance for making API requests.
    /// Reused across all requests to avoid socket exhaustion.
    /// </summary>
    private static readonly HttpClient s_httpClient = new();

    private static readonly SemaphoreSlim _gateKeeper = new SemaphoreSlim(10);

    /// <summary>
    /// Thread-safe cache for storing distance calculations to avoid repeated API calls.
    /// Key format: "origin|destination|mode" (normalized to lowercase).
    /// Value: Distance in kilometers.
    /// </summary> 
    private static readonly ConcurrentDictionary<string, double> s_distanceCache = new();

    /// <summary>
    /// Cache for route data (polyline, duration, distance) to avoid repeated API calls.
    /// Key: "origin|destination|mode" (normalized to lowercase).
    /// Value: RouteInfo object containing route details.
    /// </summary>
    private static readonly ConcurrentDictionary<string, RouteInfo> s_routeCache = new();

    /// <summary>
    /// Indicates whether the cache has been initialized with predefined addresses.
    /// Used to ensure initialization occurs only once.
    /// </summary>
    private static bool s_cacheInitialized = false;

    /// <summary>
    /// Lock object for thread-safe cache initialization.
    /// Ensures only one thread initializes the cache at a time.
    /// </summary>
    private static readonly object s_cacheLock = new();

    /// <summary>
    /// Route information including polyline for map display and travel metrics.
    /// Contains all data returned from the Google Directions API for a single route.
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// Gets or sets the encoded polyline string representing the route path.
        /// Can be used with Google Static Maps API to display the route.
        /// </summary>
        public string EncodedPolyline { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total distance of the route in kilometers.
        /// </summary>
        public double DistanceKm { get; set; }

        /// <summary>
        /// Gets or sets the estimated travel duration for the route.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the human-readable duration text (e.g., "15 mins").
        /// </summary>
        public string DurationText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable distance text (e.g., "5.2 km").
        /// </summary>
        public string DistanceText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes the distance cache with predefined address data.
    /// Should be called once at application startup.
    /// Uses double-checked locking pattern for thread safety.
    /// </summary>
    public static void InitializeDistanceCache()
    {
        if (s_cacheInitialized) return;

        lock (s_cacheLock)
        {
            if (s_cacheInitialized) return;

            string storeAddress = AdminManager.GetConfig().StoreAddress ?? "";
            string storeAddressLower = storeAddress.ToLowerInvariant();

            foreach (var address in Addresses)
            {
                string destination = ((string)address[0]).ToLowerInvariant();

                if (double.TryParse((string)address[3], out double drivingMeters) && drivingMeters > 0)
                {
                    string drivingKey = $"{storeAddressLower}|{destination}|driving";
                    s_distanceCache.TryAdd(drivingKey, drivingMeters / 1000.0);
                }

                if (double.TryParse((string)address[4], out double walkingMeters) && walkingMeters > 0)
                {
                    string walkingKey = $"{storeAddressLower}|{destination}|walking";
                    s_distanceCache.TryAdd(walkingKey, walkingMeters / 1000.0);
                }
            }

            s_cacheInitialized = true;
        }
    }

    /// <summary>
    /// Clears the distance cache and resets initialization flag.
    /// Call this when the store address changes.
    /// </summary>
    public static void ClearDistanceCache()
    {
        lock (s_cacheLock)
        {
            s_distanceCache.Clear();
            s_cacheInitialized = false;
        }
    }

    /// <summary>
    /// Clears the route cache.
    /// Call this method to force fresh route calculations on subsequent requests.
    /// </summary>
    public static void ClearCache() => s_routeCache.Clear();

    /// <summary>
    /// Converts shipment type to Google Maps travel mode.
    /// </summary>
    /// <param name="shipmentType">The type of shipment/vehicle.</param>
    /// <returns>
    /// "walking" for FOOT and BIKE shipment types,
    /// "driving" for MOTORCYCLE, CAR, and default cases.
    /// </returns>
    private static string s_getTravelMode(BO.TheTypeShipment shipmentType)
    {
        return shipmentType switch
        {
            BO.TheTypeShipment.FOOT => "walking",
            BO.TheTypeShipment.BIKE => "walking",
            BO.TheTypeShipment.MOTORCYCLE => "driving",
            BO.TheTypeShipment.CAR => "driving",
            _ => "driving"
        };
    }

    /// <summary>
    /// Prepares the HTTP client for API requests by setting the User-Agent header.
    /// </summary>
    private static void s_prepareHttpClient()
    {
        s_httpClient.DefaultRequestHeaders.Clear();
        s_httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    /// <summary>
    /// Parses route information from a Google Directions API XML response.
    /// </summary>
    /// <param name="route">The route XML element.</param>
    /// <param name="leg">The leg XML element containing distance and duration.</param>
    /// <returns>A RouteInfo object with parsed data.</returns>
    private static RouteInfo s_parseRouteInfo(XElement route, XElement leg)
    {
        return new RouteInfo
        {
            EncodedPolyline = route.Element("overview_polyline")?.Element("points")?.Value ?? string.Empty,
            DistanceKm = double.TryParse(leg.Element("distance")?.Element("value")?.Value, out double distMeters)
                        ? distMeters / 1000.0
                        : 0,
            DistanceText = leg.Element("distance")?.Element("text")?.Value ?? string.Empty,
            Duration = double.TryParse(leg.Element("duration")?.Element("value")?.Value, out double durationSec)
                      ? TimeSpan.FromSeconds(durationSec)
                      : TimeSpan.Zero,
            DurationText = leg.Element("duration")?.Element("text")?.Value ?? string.Empty
        };
    }

    /// <summary>
    /// Gets route information between two points using Google Directions API (XML format).
    /// Results are cached to minimize API calls.
    /// </summary>
    /// <param name="originLat">Origin latitude coordinate.</param>
    /// <param name="originLng">Origin longitude coordinate.</param>
    /// <param name="destLat">Destination latitude coordinate.</param>
    /// <param name="destLng">Destination longitude coordinate.</param>
    /// <param name="mode">Travel mode: "driving", "walking", "bicycling", or "transit". Defaults to "driving".</param>
    /// <returns>
    /// A <see cref="RouteInfo"/> object containing route details if successful,
    /// or null if the route calculation fails.
    /// </returns>
    public static async Task<RouteInfo?> GetRoute(double originLat, double originLng,
                                       double destLat, double destLng,
                                       string mode = "driving")
    {
        string cacheKey = $"{originLat:F6},{originLng:F6}|{destLat:F6},{destLng:F6}|{mode}".ToLowerInvariant();

        if (s_routeCache.TryGetValue(cacheKey, out var cachedRoute))
            return cachedRoute;

        try
        {
            string apiKey = AdminManager.GetConfig().GoogleApiKey;
            string origin = $"{originLat},{originLng}";
            string destination = $"{destLat},{destLng}";

            string url = $"https://maps.googleapis.com/maps/api/directions/xml" +
                        $"?origin={origin}" +
                        $"&destination={destination}" +
                        $"&mode={mode}" +
                        $"&key={apiKey}";

            s_prepareHttpClient();

            string xmlContent = await s_httpClient.GetStringAsync(url);
            XDocument doc = XDocument.Parse(xmlContent);

            string? status = doc.Root?.Element("status")?.Value;

            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] URL: {url}");
            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Status: {status}");

            if (status != "OK")
            {
                string? errorMsg = doc.Root?.Element("error_message")?.Value;
                System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Error: {errorMsg}");
                return null;
            }

            var route = doc.Root?.Element("route");
            var leg = route?.Element("leg");

            if (route == null || leg == null)
                return null;

            var routeInfo = s_parseRouteInfo(route, leg);
            s_routeCache.TryAdd(cacheKey, routeInfo);
            return routeInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Exception: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets route between two addresses (not coordinates).
    /// Results are cached to minimize API calls.
    /// </summary>
    /// <param name="originAddress">The origin address string.</param>
    /// <param name="destinationAddress">The destination address string.</param>
    /// <param name="mode">Travel mode: "driving", "walking", "bicycling", or "transit". Defaults to "driving".</param>
    /// <returns>
    /// A <see cref="RouteInfo"/> object containing route details if successful,
    /// or null if the route calculation fails.
    /// </returns>
    public static async Task<RouteInfo?> GetRouteByAddress(string originAddress, string destinationAddress, string mode = "driving")
    {
        string cacheKey = $"{originAddress}|{destinationAddress}|{mode}".ToLowerInvariant();

        if (s_routeCache.TryGetValue(cacheKey, out var cachedRoute))
            return cachedRoute;

        try
        {
            string apiKey = AdminManager.GetConfig().GoogleApiKey;
            string origin = Uri.EscapeDataString(originAddress);
            string destination = Uri.EscapeDataString(destinationAddress);

            string url = $"https://maps.googleapis.com/maps/api/directions/xml" +
                        $"?origin={origin}" +
                        $"&destination={destination}" +
                        $"&mode={mode}" +
                        $"&key={apiKey}";

            s_prepareHttpClient();

            string xmlContent = await s_httpClient.GetStringAsync(url);
            XDocument doc = XDocument.Parse(xmlContent);

            string? status = doc.Root?.Element("status")?.Value;

            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Address URL: {url}");
            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Status: {status}");

            if (status != "OK")
            {
                string? errorMsg = doc.Root?.Element("error_message")?.Value;
                System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Error: {errorMsg}");
                return null;
            }

            var route = doc.Root?.Element("route");
            var leg = route?.Element("leg");

            if (route == null || leg == null)
                return null;

            var routeInfo = s_parseRouteInfo(route, leg);
            s_routeCache.TryAdd(cacheKey, routeInfo);
            return routeInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GoogleMapsService] Exception: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets route from store to a destination using coordinates.
    /// </summary>
    /// <param name="destLat">Destination latitude coordinate.</param>
    /// <param name="destLng">Destination longitude coordinate.</param>
    /// <param name="shipmentType">The type of shipment/vehicle to determine travel mode.</param>
    /// <returns>
    /// A <see cref="RouteInfo"/> object containing route details if successful,
    /// or null if the route calculation fails.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when store latitude or longitude is not configured.
    /// </exception>
    public static async Task<RouteInfo?> GetRouteFromStore(double destLat, double destLng, BO.TheTypeShipment shipmentType)
    {
        var config = AdminManager.GetConfig();
        double storeLat = config.Latitude ?? throw new InvalidOperationException("Store latitude not configured");
        double storeLng = config.Longitude ?? throw new InvalidOperationException("Store longitude not configured");

        string mode = s_getTravelMode(shipmentType);
        return await GetRoute(storeLat, storeLng, destLat, destLng, mode);
    }

    /// <summary>
    /// Gets route from store to a destination using address string.
    /// </summary>
    /// <param name="destinationAddress">The destination address string.</param>
    /// <param name="shipmentType">The type of shipment/vehicle to determine travel mode.</param>
    /// <returns>
    /// A <see cref="RouteInfo"/> object containing route details if successful,
    /// or null if the route calculation fails.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when store address is not configured.
    /// </exception>
    public static async Task<RouteInfo?> GetRoute(string destinationAddress, BO.TheTypeShipment shipmentType)
    {
        var config = AdminManager.GetConfig();
        string storeAddress = config.StoreAddress ?? throw new InvalidOperationException("Store address not configured");

        string mode = s_getTravelMode(shipmentType);
        return await GetRouteByAddress(storeAddress, destinationAddress, mode);
    }

    /// <summary>
    /// Builds a Static Map URL with route overlay.
    /// </summary>
    /// <param name="originLat">Origin latitude coordinate.</param>
    /// <param name="originLng">Origin longitude coordinate.</param>
    /// <param name="destLat">Destination latitude coordinate.</param>
    /// <param name="destLng">Destination longitude coordinate.</param>
    /// <param name="encodedPolyline">The encoded polyline string representing the route.</param>
    /// <param name="width">Image width in pixels. Defaults to 400.</param>
    /// <param name="height">Image height in pixels. Defaults to 300.</param>
    /// <returns>A URL string for the Google Static Maps API with markers and route overlay.</returns>
    public static async Task<string> GetStaticMapUrl(double originLat, double originLng,
                                          double destLat, double destLng,
                                          string encodedPolyline,
                                          int width = 400, int height = 300)
    {
        string apiKey = AdminManager.GetConfig().GoogleApiKey;
        string encodedPath = Uri.EscapeDataString(encodedPolyline);

        return await Task.FromResult($"https://maps.googleapis.com/maps/api/staticmap" +
               $"?size={width}x{height}" +
               $"&markers=color:green|label:S|{originLat},{originLng}" +
               $"&markers=color:red|label:D|{destLat},{destLng}" +
               $"&path=enc:{encodedPath}" +
               $"&key={apiKey}");
    }

    /// <summary>
    /// Gets static map URL from store to destination with route.
    /// </summary>
    /// <param name="destLat">Destination latitude coordinate.</param>
    /// <param name="destLng">Destination longitude coordinate.</param>
    /// <param name="shipmentType">The type of shipment/vehicle to determine travel mode.</param>
    /// <param name="width">Image width in pixels. Defaults to 400.</param>
    /// <param name="height">Image height in pixels. Defaults to 300.</param>
    /// <returns>
    /// A URL string for the Google Static Maps API if successful,
    /// or null if the route calculation fails.
    /// </returns>
    public static async Task<string?> GetStaticMapUrlFromStore(double destLat, double destLng,
                                                    BO.TheTypeShipment shipmentType,
                                                    int width = 400, int height = 300)
    {
        var route = await GetRouteFromStore(destLat, destLng, shipmentType);
        if (route == null || string.IsNullOrEmpty(route.EncodedPolyline))
            return null;

        var config = AdminManager.GetConfig();
        double storeLat = config.Latitude ?? 0;
        double storeLng = config.Longitude ?? 0;

        return await GetStaticMapUrl(storeLat, storeLng, destLat, destLng,
                               route.EncodedPolyline, width, height);
    }

    /// <summary>
    /// Converts a street address to geographic coordinates using the Google Geocoding API.
    /// </summary>
    /// <param name="address">The street address to geocode.</param>
    /// <returns>
    /// A tuple containing the latitude and longitude coordinates if successful,
    /// or null if the geocoding fails or the address is not found.
    /// </returns>
    /// <exception cref="BO.BlInvalidValueException">
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
    /// </remarks>
    public static async Task<(double Lat, double Lng)?> GetGeocodingAsync(string address)
    {
        var apiKey = AdminManager.GetConfig().GoogleApiKey;
        string url = $"https://maps.googleapis.com/maps/api/geocode/xml?address={Uri.EscapeDataString(address)}&key={apiKey}";

        try
        {
            s_prepareHttpClient();
            HttpResponseMessage response = await s_httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to get geocoding data.");

            string xmlContent = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(xmlContent);
            string? status = doc.Element("GeocodeResponse")?.Element("status")?.Value;

            if (status == "ZERO_RESULTS")
                throw new BO.BlInvalidValueException("The address was not found in Google's database.");

            if (status != "OK")
                throw new Exception($"Geocoding API returned status: {status}");

            var geometry = doc.Element("GeocodeResponse")?
                             .Element("result")?
                             .Element("geometry");

            var locationType = geometry?.Element("location_type")?.Value;
            if (locationType is "APPROXIMATE" or "GEOMETRIC_CENTER")
                throw new BO.BlInvalidValueException("The address entered is not precise enough. Please enter a more complete address.");

            var locationElement = geometry?.Element("location");
            if (locationElement == null)
                throw new Exception("Location element not found in the response.");

            double lat = double.Parse(locationElement.Element("lat")!.Value);
            double lng = double.Parse(locationElement.Element("lng")!.Value);

            return (lat, lng);
        }
        catch (BO.BlInvalidValueException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Geocoding error: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculates the actual road distance from the store to a delivery address using the Google Distance Matrix API.
    /// </summary>
    /// <param name="address">The destination address for the delivery.</param>
    /// <param name="typeShipment">The type of shipment/vehicle to be used for the delivery, which determines the travel mode.</param>
    /// <returns>
    /// The actual road distance in kilometers if successful, or null if the calculation fails.
    /// </returns>
    /// <exception cref="BO.BlInvalidValueException">
    /// Thrown when:
    /// - Google API Key is not configured in the system
    /// - Store Address is not configured in the system
    /// - The address was not found in Google's database (ZERO_RESULTS)
    /// - Unable to calculate distance for the provided address (element status not OK)
    /// </exception>
    /// <exception cref="BO.BlDoesNotExistException">
    /// Thrown when the API request fails or encounters an error during execution.
    /// </exception>
    /// <remarks>
    /// This method makes a synchronous HTTP request to the Google Distance Matrix API.
    /// The API key and store address are retrieved from the system configuration.
    /// Results are cached to minimize API calls.
    /// 
    /// The method calculates the actual road distance based on real routes, which may differ
    /// from the straight-line distance calculated by the Haversine formula.
    /// </remarks>
    public static async Task<double?> GetActualDistance(double latitude, double longitude, BO.TheTypeShipment typeShipment)
    {
        InitializeDistanceCache();

        string apiKey = AdminManager.GetConfig().GoogleApiKey
            ?? throw new BO.BlInvalidValueException("Google API Key is not configured.");

        double storeLatitude = AdminManager.GetConfig().Latitude
            ?? throw new BO.BlInvalidValueException("Store Latitude is not configured.");

        double storeLongitude = AdminManager.GetConfig().Longitude
            ?? throw new BO.BlInvalidValueException("Store Longitude is not configured.");

        string mode = s_getTravelMode(typeShipment);
        string cacheKey = $"{storeLatitude}|{storeLongitude}|{latitude}|{longitude}|{mode}".ToLowerInvariant();

        // Check if distance already exists in cache
        if (s_distanceCache.TryGetValue(cacheKey, out double cachedDistance))
        {
            return cachedDistance;
        }

        string url = $"https://maps.googleapis.com/maps/api/distancematrix/xml" +
                    $"?origins={Uri.EscapeDataString($"{storeLatitude},{storeLongitude}")}" +
                    $"&destinations={Uri.EscapeDataString($"{latitude},{longitude}")}" +
                    $"&mode={mode}" +
                    $"&key={apiKey}";

        try
        {
            s_prepareHttpClient();
            HttpResponseMessage response = await s_httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to get distance matrix data.");

            string xmlContent = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(xmlContent);
            string? status = doc.Element("DistanceMatrixResponse")?.Element("status")?.Value;

            if (status == "ZERO_RESULTS")
                throw new BO.BlInvalidValueException("The address was not found in Google's database.");

            if (status != "OK")
                throw new Exception($"Distance Matrix API returned status: {status}");

            var element = doc.Element("DistanceMatrixResponse")?
                             .Element("row")?
                             .Element("element");

            var elementStatus = element?.Element("status")?.Value;
            if (elementStatus != "OK")
                throw new BO.BlInvalidValueException("Unable to calculate distance for the provided address.");

            var distanceElement = element?.Element("distance");
            if (distanceElement == null)
                return null;

            double distance = double.Parse(distanceElement.Element("value")!.Value);
            double distanceKm = distance / 1000.0;
            s_distanceCache.TryAdd(cacheKey, distanceKm);
            return distanceKm;
        }
        catch (BO.BlInvalidValueException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BO.BlDoesNotExistException($"Distance calculation error: {ex.Message}");
        }
    }




    public static async Task<T> NetworkKeeper<T>(Func<Task<T>> action)
    {
        await _gateKeeper.WaitAsync();

        try
        {
            await Task.Delay(100);
            T result = await action();
            
            return result;
        }
        finally
        {
            _gateKeeper.Release();
        }
    }

    //public static async Task NetworkKeeper(Func<Task> action)
    //{
    //    await _gateKeeper.WaitAsync();
    //    try
    //    {
    //        await action();
    //        await Task.Delay(100);
    //    }
    //    finally
    //    {
    //        _gateKeeper.Release();
    //    }
    //}


    /// <summary>
    /// Checks if a given ID belongs to the system manager.
    /// </summary>
    /// <param name="id">The ID to verify.</param>
    /// <returns>True if the ID matches the manager ID configured in the system; otherwise, false.</returns>
    /// <remarks>
    /// This method is used for authorization checks to determine if a user has manager privileges.
    /// The manager ID is retrieved from the system configuration.
    /// </remarks>
    public static bool CheckManager(int id)
    {
        return id == AdminManager.GetConfig().ManagerId;
    }

    /// <summary>
    /// Predefined address data for caching distance calculations.
    /// Each entry contains:
    /// <list type="bullet">
    ///   <item><description>Index 0: Address string (street, city)</description></item>
    ///   <item><description>Index 1: Latitude coordinate</description></item>
    ///   <item><description>Index 2: Longitude coordinate</description></item>
    ///   <item><description>Index 3: Driving distance in meters from store</description></item>
    ///   <item><description>Index 4: Walking distance in meters from store</description></item>
    /// </list>
    /// Used to pre-populate the distance cache and avoid API calls for known addresses.
    /// </summary>
    private static readonly object[][] Addresses =
    [
        [" רבי עקיבא 50, בני ברק", "32.0876045", "34.8278091", "912", "898"],
        [" נחמיה 8, בני ברק", "32.0793696", "34.8352634", "3349", "2361"],
        [" בן גוריון 25, גבעת שמואל", "32.0952663", "34.8220752", "421", "308"],
        [" דסלר 4, בני ברק", "32.0818569", "34.8315636", "2697", "1740"],
        [" סוקולוב 30, בני ברק", "32.0889927", "34.8341474", "1588", "1468"],
        [" הרב קוק 18, בני ברק", "32.0865247", "34.8262516", "1367", "1047"],
        [" הירקון 5, בני ברק", "32.0965116", "34.822183", "327", "437"],
        [" ביאליק 40, רמת גן", "32.0828089", "34.8147672", "1993", "1853"],
        [" הרצל 60, רמת גן", "32.0850858", "34.8155354", "1949", "1528"],
        [" אבא הלל 15, רמת גן", "32.085569", "34.803969", "2578", "2267"],
        [" ז'בוטינסקי 55, רמת גן", "32.0854799", "34.8101674", "1772", "1697"],
        [" חזון איש 20, בני ברק", "32.0834133", "34.8356933", "2466", "1975"],
        [" קריניצי 20, רמת גן", "32.0797548", "34.8172104", "1907", "1833"],
        [" הראה 80, רמת גן", "32.0817288", "34.8254736", "1741", "1599"],
        [" ארלוזורוב 10, רמת גן", "32.0794038", "34.8138358", "2233", "2159"],
        [" שדרות ירושלים 30, רמת גן", "32.0847242", "34.8297262", "1349", "1335"],
        [" בן גוריון 100, רמט גן", "32.0865566", "34.8216913", "923", "842"],
        [" נגבה 25, רמת גן", "32.070547", "34.8249663", "3026", "2884"],
        [" הירדן 40, רמת גן", "32.066254", "34.8281993", "3809", "3520"],
        [" אלוף שדה 15, רמת גן", "32.0594879", "34.8249646", "4698", "4569"],
        [" רוקח 10, רמת גן", "32.0876832", "34.8111166", "2103", "1626"],
        [" תרצה 8, רמת גן", "32.0752286", "34.8280531", "2913", "2520"],
        [" ז'בוטינסקי 100, בני ברק", "32.0917476", "34.8321991", "1109", "1095"],
        [" המעגל 12, רמת גן", "32.0829031", "34.8124271", "2213", "1935"],
        [" ויצמן 20, גבעתיים", "32.0737141", "34.8083944", "3465", "3131"],
        [" כצנלסון 50, גבעתיים", "32.0750742", "34.8076031", "4016", "3061"],
        [" שיינקין 15, גבעתיים", "32.0748186", "34.8107739", "2974", "2884"],
        [" רמב''ם 10, גבעתיים", "32.0700634", "34.8038533", "4371", "3933"],
        [" גורדון 5, גבעתיים", "32.076085", "34.807312", "3383", "2953"],
        [" סירקין 12, גבעתיים", "32.0775637", "34.814622", "2702", "2297"],
        [" בורוכוב 8, גבעתיים", "32.0775935", "34.8029331", "3526", "3065"],
        [" המאבק 25, גבעתיים", "32.0638384", "34.8099736", "4389", "4217"],
        [" עליית הנוער 10, גבעתיים", "32.0770387", "34.8020075", "3997", "3232"],
        [" ירושלים 15, בני ברק", "32.0857512", "34.8300664", "1231", "1217"],
        [" דרך השלום 40, גבעתיים", "32.0694025", "34.8022553", "4712", "4245"],
        [" דיזנגוף 100, תל אביב", "32.0793963", "34.7740011", "6268", "5571"],
        [" בן יהודה 50, תל אביב", "32.0780449", "34.7688332", "7231", "6157"],
        [" אבן גבירול 70, תל אביב", "32.0806057", "34.7815385", "5393", "4886"],
        [" רוטשילד 45, תל אביב", "32.0642206", "34.7747952", "6615", "6204"],
        [" אלנבי 80, תל אביב", "32.0678218", "34.7710389", "7120", "6334"],
        [" המלך ג'ורג' 30, תל אביב", "32.0722931", "34.7741392", "6502", "5666"],
        [" שינקין 20, תל אביב", "32.0693286", "34.7725233", "7396", "6205"],
        [" דרך מנחם בגין 120, תל אביב", "32.071223", "34.7898718", "5442", "4494"],
        [" המסגר 15, תל אביב", "32.0628255", "34.7850239", "7517", "5476"],
        [" הרב שך 10, בני ברק", "32.0864414", "34.8357055", "2044", "1700"],
        [" יגאל אלון 60, תל אביב", "32.0619207", "34.7927464", "8326", "5292"],
        [" דרך ההגנה 40, תל אביב", "32.0540338", "34.7869603", "9038", "6506"],
        [" הירקון 150, תל אביב", "32.0837404", "34.7693911", "9225", "6139"],
        [" פרישמן 10, תל אביב", "32.0798034", "34.7688166", "6705", "6009"],
        [" בוגרשוב 25, תל אביב", "32.0769875", "34.7698549", "10607", "5951"],
        [" יפת 80, יפו (תל אביב)", "32.0464433", "34.7523983", "12889", "8511"],
        [" דרך קיבוץ גלויות 30, תל אביב", "32.0517973", "34.7680502", "13038", "7756"],
        [" לה גווארדיה 20, תל אביב", "32.059112", "34.788742", "7291", "5820"],
        [" ארלוזורוב 90, תל אביב", "32.0854282", "34.7806379", "5603", "4724"],
        [" כהנמן 60, בני ברק", "32.0845043", "34.8399999", "2550", "2213"],
        [" נמיר 50, תל אביב", "32.0850486", "34.7953535", "3666", "3252"],
        [" חיים עוזר 10, פתח תקווה", "32.0890587", "34.8861437", "7837", "6418"],
        [" רוטשילד 50, פתח תקווה", "32.0903978", "34.8804676", "7675", "5723"],
        [" ז'בוטינסקי 80, פתח תקווה", "32.091785", "34.8641828", "5401", "4110"],
        [" העצמאות 20, פתח תקווה", "32.074777", "34.880136", "11278", "7102"],
        [" אורלוב 30, פתח תקווה", "32.0934753", "34.8820009", "7199", "5893"],
        [" שטמפפר 15, פתח תקווה", "32.0901497", "34.8855149", "7737", "6262"],
        [" עין גנים 40, פתח תקווה", "32.086933", "34.894794", "8738", "7412"],
        [" סלומון 12, פתח תקווה", "32.0846599", "34.8805418", "8231", "6309"],
        [" פינסקר 8, פתח תקווה", "32.0907496", "34.8845311", "7920", "6310"],
        [" אהרונוביץ' 12, בני ברק", "32.090832", "34.8386345", "2020", "1982"],
        [" גיסין 25, פתח תקווה", "32.0976387", "34.8797159", "6395", "5893"],
        [" סוקולוב 50, חולון", "32.0227089", "34.7745575", "11404", "10429"],
        [" שנקר 20, חולון", "32.0261601", "34.7765943", "11279", "10237"],
        [" דב הוז 30, חולון", "32.0236133", "34.7667955", "11864", "10636"],
        [" שדרות קוגל 15, חולון", "32.0257767", "34.774554", "11779", "10077"],
        [" הופיין 25, חולון", "32.0141212", "34.7685729", "12662", "11698"],
        [" בלפור 40, בת ים", "32.0261767", "34.7449781", "14614", "11734"],
        [" יוספטל 60, בת ים", "32.0165585", "34.7464493", "16092", "12857"],
        [" העצמאות 30, בת ים", "32.0231832", "34.7465895", "14429", "11887"],
        [" רוטשילד 20, בת ים", "32.026609", "34.7454099", "14696", "11652"],
        [" השומר 5, בני ברק", "32.0815759", "34.8217171", "1970", "1509"],
        [" אנילביץ' 10, בת ים", "32.0218369", "34.7517549", "13880", "12175"],
        [" ז'בוטינסקי 40, ראשון לציון", "32.0246214", "34.7821867", "11871", "10753"],
        [" הרצל 60, ראשון לציון", "31.9656402", "34.8025163", "19145", "18843"],
        [" רוטשילד 30, ראשון לציון", "31.9641474", "34.803856", "21869", "19060"],
        [" משה דיין 20, ראשון לציון", "32.0009662", "34.7665794", "16891", "13088"],
        [" לוי אשכול 15, ראשון לציון", "31.9754762", "34.7770157", "20073", "16880"],
        [" סוקולוב 40, הרצליה", "32.1669612", "34.844759", "16277", "9419"],
        [" הרב קוק 20, הרצליה", "32.161374", "34.8408944", "8778", "8642"],
        [" בן גוריון 30, הרצליה", "32.1613286", "34.8423012", "13900", "8531"],
        [" שבעת הכוכבים 10, הרצליה", "32.1636245", "34.8243901", "12923", "9945"],
        [" עזרא 25, בני ברק", "32.0782597", "34.8377103", "3640", "2686"],
        [" אוסישקין 50, רמת השרון", "32.1422739", "34.8434807", "7103", "7016"],
        [" סוקולוב 20, רמת השרון", "32.1398478", "34.8362248", "6149", "6062"],
        [" ביאליק 15, רמת השרון", "32.137793", "34.840278", "6589", "6503"],
        [" לוי אשכול 30, קריית אונו", "32.0661125", "34.8604378", "8484", "5571"],
        [" שלמה המלך 20, קריית אונו", "32.0544159", "34.8603455", "8697", "7055"],
        [" העצמאות 15, יהוד", "32.0745531", "34.8811245", "11185", "7358"],
        [" ויצמן 10, יהוד", "32.0300273", "34.8924598", "18607", "11744"],
        [" העצמאות 40, אור יהודה", "32.0289963", "34.8655017", "12632", "9877"],
        [" אליהו סעדון 20, אור יהודה", "32.0255384", "34.8549182", "12238", "9905"]
    ];

}