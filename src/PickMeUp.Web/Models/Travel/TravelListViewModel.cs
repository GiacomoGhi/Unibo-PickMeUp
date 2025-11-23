
using System;

namespace PickMeUp.Web.Models.Travel;

public class TravelListViewModel
{
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