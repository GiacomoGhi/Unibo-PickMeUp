
using System;
using PickMeUp.Enums.UserTravel;
using PickMeUp.Web.Models.Shared;

namespace PickMeUp.Web.Models.Travel;

public class TravelListViewModel
{
    /// <summary>
    /// Indicates if the view is accessed from the "Find Travel" feature.
    /// If false, the view is accessed from the "My Travels" section.
    /// </summary>
    public bool IsFromFindTravel { get; set; }

    /// <summary>
    /// The original search request filters.
    /// </summary>
    public TravelListRequest Filters { get; set; } = new();

    /// <summary>
    /// User travel item.
    /// </summary>
    public TravelListItemViewModel[] Travels { get; set; } = [];

    /// <summary>
    /// Total number of user travels with pending pick-up requests.
    /// </summary>
    public int TotalTravelsWithPendingPickUpRequestsCount { get; set; }

    /// <summary>
    /// Total number of travels where the user is a driver.
    /// </summary>
    public int TotalTravelsAsDriver { get; set; }

    /// <summary>
    /// Total number of travels where the user is a guest.
    /// </summary>
    public int TotalTravelsAsGuest { get; set; }
}

public class TravelListItemViewModel
{    
    /// <summary>
    /// User travel identifier.
    /// </summary>
    public int UserTravelId { get; set; }

    /// <summary>
    /// Identifier of the user that created the travel.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Nominative of the user that created the travel.
    /// </summary>
    public string UserNominative { get; set; } = default!;

    /// <summary>
    /// Total passenger seats count.
    /// </summary>
    public int TotalPassengersSeatsCount { get; set; }

    /// <summary>
    /// Count of occupied passenger seats.
    /// </summary>
    public int OccupiedPassengerSeatsCount { get; set; }

    /// <summary>
    /// Address of the departure location.
    /// </summary>
    public string DepartureAddress { get; set; } = default!;

    /// <summary>
    /// Date and time of departure.
    /// </summary>
    public DateTime DepartureDateTime { get; set; }

    /// <summary>
    /// Address of the destination location.
    /// </summary>
    public string DestinationAddress { get; set; } = default!;

    /// <summary>
    /// Ids of the users that have an accepted pick-up request for this travel.
    /// </summary>
    public int[] AcceptedPickUpRequestUserIds { get; set; } = [];

    /// <summary>
    /// Ids of the users that have a pending pick-up request for this travel.
    /// </summary>
    public int[] PendingPickUpRequestUserIds { get; set; } = [];
}

public class TravelListRequest
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
    /// </summary>
    public UserTravelRole ShowOnlyTravelsWithRole { get; set; }

    /// <summary>
    /// Departure location to filter travels.
    /// </summary>
    public LocationViewModel? Departure { get; set; }

    /// <summary>
    /// Destination location to filter travels.
    /// </summary>
    public LocationViewModel? Destination { get; set; }

    /// <summary>
    /// Departure date to filter travels.
    /// </summary>
    public DateOnly? DepartureDate { get; set; }
}