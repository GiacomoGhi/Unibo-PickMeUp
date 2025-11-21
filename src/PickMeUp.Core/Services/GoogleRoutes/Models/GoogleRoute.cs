using System;

namespace PickMeUp.Core.Services.GoogleRoutes;

public class GoogleRoute
{
    /// <summary>
    /// Encoded polyline representing the route.
    /// </summary>
    public string EncodedPolyline { get; set; } = default!;

    /// <summary>
    /// Total distance of the route in meters.
    /// </summary>
    public long DistanceMeters { get; set; }

    /// <summary>
    /// Total duration of the route in seconds.
    /// </summary>
    public TimeSpan Duration { get; set; }
}