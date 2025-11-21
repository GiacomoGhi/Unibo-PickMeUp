using PickMeUp.Core.Common.Models;

namespace PickMeUp.Core.Services.GoogleRoutes;

public class GetRouteParams
{
    /// <summary>
    /// Origin coordinates for the route.
    /// </summary>
    public Coordinates Origin { get; set; } = default!;
    
    /// <summary>
    /// Destination coordinates for the route.
    /// </summary>
    public Coordinates Destination { get; set; } = default!;
}