using System.Collections.Concurrent;
using System.Net.Http;
using System.Xml.Linq;

namespace Helpers;

/// <summary>
/// Service for Google Maps API integration - routes and static map images
/// </summary>
public static class GoogleMapsService
{
    private static readonly HttpClient s_httpClient = new();

    /// <summary>
    /// Cache for route data (polyline, duration, distance) to avoid repeated API calls
    /// Key: "origin|destination|mode" (normalized to lowercase)
    /// </summary>
    private static readonly ConcurrentDictionary<string, RouteInfo> s_routeCache = new();

    /// <summary>
    /// Route information including polyline for map display and travel metrics
    /// </summary>
    public class RouteInfo
    {
        public string EncodedPolyline { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public string DistanceText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Converts shipment type to Google Maps travel mode
    /// </summary>
    private static string GetTravelMode(BO.TheTypeShipment shipmentType)
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
    /// Gets route information between two points using Google Directions API (XML format)
    /// </summary>
    public static RouteInfo? GetRoute(double originLat, double originLng,
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

            s_httpClient.DefaultRequestHeaders.Clear();
            s_httpClient.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");

            string xmlContent = s_httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            XDocument doc = XDocument.Parse(xmlContent);

            string? status = doc.Root?.Element("status")?.Value;
            
            // Debug logging
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

            var routeInfo = new RouteInfo
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
    /// Gets route from store to a destination using coordinates
    /// </summary>
    public static RouteInfo? GetRouteFromStore(double destLat, double destLng, BO.TheTypeShipment shipmentType)
    {
        var config = AdminManager.GetConfig();
        double storeLat = config.Latitude ?? throw new InvalidOperationException("Store latitude not configured");
        double storeLng = config.Longitude ?? throw new InvalidOperationException("Store longitude not configured");

        string mode = GetTravelMode(shipmentType);
        return GetRoute(storeLat, storeLng, destLat, destLng, mode);
    }

    /// <summary>
    /// Gets route from store to a destination using address string
    /// </summary>
    public static RouteInfo? GetRouteFromStore(string destinationAddress, BO.TheTypeShipment shipmentType)
    {
        var config = AdminManager.GetConfig();
        string storeAddress = config.StoreAddress ?? throw new InvalidOperationException("Store address not configured");

        string mode = GetTravelMode(shipmentType);
        return GetRouteByAddress(storeAddress, destinationAddress, mode);
    }

    /// <summary>
    /// Gets route between two addresses (not coordinates)
    /// </summary>
    public static RouteInfo? GetRouteByAddress(string originAddress, string destinationAddress, string mode = "driving")
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

            s_httpClient.DefaultRequestHeaders.Clear();
            s_httpClient.DefaultRequestHeaders.Add("User-Agent", "dotNet5786_3997_6339");

            string xmlContent = s_httpClient.GetStringAsync(url).GetAwaiter().GetResult();
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

            var routeInfo = new RouteInfo
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
    /// Builds a Static Map URL with route overlay
    /// </summary>
    public static string GetStaticMapUrl(double originLat, double originLng,
                                          double destLat, double destLng,
                                          string encodedPolyline,
                                          int width = 400, int height = 300)
    {
        string apiKey = AdminManager.GetConfig().GoogleApiKey;
        string encodedPath = Uri.EscapeDataString(encodedPolyline);

        return $"https://maps.googleapis.com/maps/api/staticmap" +
               $"?size={width}x{height}" +
               $"&markers=color:green|label:S|{originLat},{originLng}" +
               $"&markers=color:red|label:D|{destLat},{destLng}" +
               $"&path=enc:{encodedPath}" +
               $"&key={apiKey}";
    }

    /// <summary>
    /// Gets static map URL from store to destination with route
    /// </summary>
    public static string? GetStaticMapUrlFromStore(double destLat, double destLng,
                                                    BO.TheTypeShipment shipmentType,
                                                    int width = 400, int height = 300)
    {
        var route = GetRouteFromStore(destLat, destLng, shipmentType);
        if (route == null || string.IsNullOrEmpty(route.EncodedPolyline))
            return null;

        var config = AdminManager.GetConfig();
        double storeLat = config.Latitude ?? 0;
        double storeLng = config.Longitude ?? 0;

        return GetStaticMapUrl(storeLat, storeLng, destLat, destLng,
                               route.EncodedPolyline, width, height);
    }

    /// <summary>
    /// Clears the route cache
    /// </summary>
    public static void ClearCache() => s_routeCache.Clear();
}