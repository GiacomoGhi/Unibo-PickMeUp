using System;
using PickMeUp.Enums.UserTravel;

namespace PickMeUp.Core.Common.Models;

public class ListItemsParams : ParamsBase
{
    /// <summary>
    /// Indicates if the request is from the "Find Travel" feature.
    /// If false, the request is from the "My Travels" section and 
    /// It will show all travels where the user is involved.
    /// </summary>
    public bool IsFromFindTravel { get; set; } = false;

    /// <summary>
    /// Indicates if only travels with pending pick-up requests should be shown.
    /// (Other filters will be ignored in this case.)
    /// </summary>
    public bool ShowOnlyTravelsWithPendingPickUpRequests { get; set; } = false;

    /// <summary>
    /// Role of the user in the travel to filter travels.
    /// (Other filters will be ignored in this case.)
    /// </summary>
    public UserTravelRole ShowOnlyTravelsWithRole { get; set; }

    /// <summary>
    /// Destination location to filter travels.
    /// </summary>
    public Location? DepartureLocation { get; set; }
    
    /// <summary>
    /// Destination location to filter travels.
    /// </summary>
    public Location? DestinationLocation { get; set; }

    /// <summary>
    /// Departure date to filter travels.
    /// </summary>
    public DateOnly? DepartureDate { get; set; }

    // TODO when pagination will be implemented
    // /// <summary>
    // /// Number of items to skip (for pagination/virtual scroll).
    // /// </summary>
    // public int Skip { get; set; } = 0;

    // /// <summary>
    // /// Number of items to take per request.
    // /// </summary>
    // public int Take { get; set; } = 20;

    // /// <summary>
    // /// Optional filter criteria.
    // /// </summary>
    // public string? SearchTerm { get; set; }

    // /// <summary>
    // /// Sort field name.
    // /// </summary>
    // public int? OrderBy { get; set; }

    // /// <summary>
    // /// Sort direction.
    // /// </summary>
    // public bool OrderByDescending { get; set; } = false;
}
