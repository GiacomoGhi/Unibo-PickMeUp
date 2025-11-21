using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PickMeUp.Core.Common.Models;

namespace PickMeUp.Core.Services.GoogleRoutes;

public class GoogleRoutesService(HttpClient httpClient, IOptions<GoogleSettings> googleSettings) : IGoogleRoutesService
{
    // Google Routes API endpoint
    private const string GOOGLE_ROUTES_URL = "https://routes.googleapis.com/directions/v2:computeRoutes";

    // JSON serialization options
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // Injected services
    private readonly HttpClient _httpClient = httpClient;
    private readonly GoogleSettings _googleSettings = googleSettings.Value;

    /// <inheritdoc/>
    public async Task<Result<GoogleRoute>> GetRouteAsync(GetRouteParams requestParams)
    {
        // Prepare request body
        var body = new
        {
            origin = new
            {
                location = new
                {
                    latLng = new
                    {
                        latitude = requestParams.Origin.Latitude,
                        longitude = requestParams.Origin.Longitude,
                    }
                }
            },
            destination = new
            {
                location = new
                {
                    latLng = new
                    {
                        latitude = requestParams.Destination.Latitude,
                        longitude = requestParams.Destination.Longitude,
                    }
                }
            },
            travelMode = "DRIVE",
            routingPreference = "TRAFFIC_AWARE",
            computeAlternativeRoutes = false,
            routeModifiers = new
            {
                avoidTolls = false,
                avoidHighways = false,
                avoidFerries = false,
            },
            languageCode = "it-IT",
            units = "METRIC",
        };

        // Prepare and send HTTP request
        using var request = new HttpRequestMessage(HttpMethod.Post, GOOGLE_ROUTES_URL);
        request.Headers.Add("X-Goog-Api-Key", _googleSettings.MapsApiKey);
        request.Headers.Add("X-Goog-FieldMask", "routes.distanceMeters,routes.duration,routes.polyline.encodedPolyline");
        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request);

        // Handle response
        if (!response.IsSuccessStatusCode)
        {
            return Result.Error($"Status code: {response.StatusCode}. Error: {response.ReasonPhrase}");
        } 

        // Deserialize response content
        var json = await response.Content.ReadAsStringAsync();

        var routes = JsonSerializer.Deserialize<RoutesApiResponse>(json, _jsonSerializerOptions)!;
        var route = routes.Routes.First();

        // duration is in format "600s"
        var secondsString = route.Duration.TrimEnd('s');
        if (!long.TryParse(secondsString, out var seconds))
        {
            seconds = 0;
        }

        return Result.Success(
            new GoogleRoute
            {
                DistanceMeters = route.DistanceMeters,
                Duration = TimeSpan.FromSeconds(seconds),
                EncodedPolyline = route.Polyline.EncodedPolyline
            });
    }

    private class RoutesApiResponse
    {
        public List<Route> Routes { get; set; } = [];
    }

    private class Route
    {
        public long DistanceMeters { get; set; }
        public string Duration { get; set; } = default!;
        public Polyline Polyline { get; set; } = default!;
    }

    private class Polyline
    {
        public string EncodedPolyline { get; set; } = default!;
    }
}