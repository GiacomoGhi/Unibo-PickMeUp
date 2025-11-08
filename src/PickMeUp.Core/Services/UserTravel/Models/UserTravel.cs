using PickMeUp.Core.Services.UserPickUpRequest;
using System;

namespace PickMeUp.Core.Services.UserTravel;

public class UserTravel
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
    /// Latitude coordinate of the departure location.
    /// </summary>
    public double DepartureLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the departure location.
    /// </summary>
    public double DepartureLongitude { get; set; }

    /// <summary>
    /// Address of the departure location.
    /// </summary>
    public string DepartureAddress { get; set; } = default!;

    /// <summary>
    /// Date and time of departure.
    /// </summary>
    public DateTime DepartureDateTime { get; set; }

    /// <summary>
    /// Latitude coordinate of the destination location.
    /// </summary>
    public double DestinationLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the destination location.
    /// </summary>
    public double DestinationLongitude { get; set; }

    /// <summary>
    /// Address of the destination location.
    /// </summary>
    public string DestinationAddress { get; set; } = default!;

    /// <summary>
    /// Date and time of arrival at the destination.
    /// </summary>
    public DateTime ArrivalDateTime { get; set; }

    /// <summary>
    /// Pick up requests associated with this travel.
    /// </summary>
    public UserPickUpRequestLookupItem[] TravelPickUpRequests { get; set; } = [];
}
