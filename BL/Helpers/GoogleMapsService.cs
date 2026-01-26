using System.Collections.Concurrent;
using System.Diagnostics;
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
    private static readonly HttpClient s_httpClient = new()
    {
         DefaultRequestHeaders =
         {
             { "User-Agent", UserAgent }
         }
    };

    private static readonly SemaphoreSlim _gateKeeper = new SemaphoreSlim(10);

    /// <summary>
    /// Cache for route data (polyline, duration, distance) to avoid repeated API calls.
    /// Key: "origin|destination|mode" (normalized to lowercase).
    /// Value: RouteInfo object containing route details.
    /// </summary>
    private static readonly ConcurrentDictionary<string, RouteInfo> s_routeCache = new();


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

    static GoogleMapsService()
    {
        s_httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
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
        string cacheKey = $"{originLat:F6},{originLng:F6}|{destLat:F6},{destLng:F6}|{mode}|shortest".ToLowerInvariant();

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
                         $"&alternatives=true" +
                         $"&mode={mode}" +
                         $"&key={apiKey}";

            s_prepareHttpClient();

            string xmlContent = await s_httpClient.GetStringAsync(url);
            XDocument doc = XDocument.Parse(xmlContent);

            string? status = doc.Root?.Element("status")?.Value;

            if (status != "OK")
                return null;

            var routes = doc.Root?.Elements("route");

            if (routes == null || !routes.Any())
                return null;
            var shortestRoute = routes
                .OrderBy(r =>
                {
                    var distVal = r.Element("leg")?.Element("distance")?.Element("value")?.Value;
                    return int.TryParse(distVal, out int val) ? val : int.MaxValue;
                })
                .FirstOrDefault();

            var leg = shortestRoute?.Element("leg");

            if (shortestRoute == null || leg == null)
                return null;

            var routeInfo = s_parseRouteInfo(shortestRoute, leg);
            s_routeCache.TryAdd(cacheKey, routeInfo);
            return routeInfo;
        }
        catch
        {
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

        var Url = await Task.FromResult($"https://maps.googleapis.com/maps/api/staticmap" +
               $"?size={width}x{height}" +
               $"&markers=color:green|label:S|{originLat},{originLng}" +
               $"&markers=color:red|label:D|{destLat},{destLng}" +
               $"&path=enc:{encodedPath}" +
               $"&key={apiKey}");

        Debug.WriteLine($"Store = {originLat},{originLng}. Address = {destLat},{destLng}");
        Debug.WriteLine(Url);
        return Url;
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
        var route = await GoogleMapsService.NetworkKeeper(() =>
            GetRouteFromStore(destLat, destLng, shipmentType));
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
    public static async Task<(double Lat, double Lng)?> GetGeocodingAsync(string address, string api)
    {
        string apiKey = api;
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
        double? storeLat = AdminManager.GetConfig().Latitude;
        double? storeLng = AdminManager.GetConfig().Longitude;

        if (storeLat is null || storeLng is null)
            throw new BO.BlInvalidValueException("Store coordinates are not configured.");

        string mode = s_getTravelMode(typeShipment);

        RouteInfo? routeInfo = await GetRoute(storeLat.Value, storeLng.Value, latitude, longitude, mode);

        if (routeInfo is null)
            return null;

        return routeInfo.DistanceKm;
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

}