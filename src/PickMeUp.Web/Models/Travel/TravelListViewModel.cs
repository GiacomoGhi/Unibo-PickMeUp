
using System;
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
}

public class TravelListItemViewModel
{    
    /// <summary>
    /// User travel identifier.
    /// </summary>
    public int UserTravelId { get; set; }

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
}

public class TravelListRequest
{
    public bool IsFromFindTravel { get; set; } = false;

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