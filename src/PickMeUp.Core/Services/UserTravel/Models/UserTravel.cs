using PickMeUp.Core.Common.Models;
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
    /// User identifier of the user that created the travel.
    /// </summary>
    public int OwnerUserId { get; set; }

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
    /// Departure location.
    /// </summary>
    public Location DepartureLocation { get; set; } = default!;

    /// <summary>
    /// Date and time of departure.
    /// </summary>
    public DateTime DepartureDateTime { get; set; }

    /// <summary>
    /// Location of the destination.
    /// </summary>
    public Location DestinationLocation { get; set; } = default!;

    /// <summary>
    /// Pick up requests associated with this travel.
    /// </summary>
    public UserPickUpRequestLookup[] TravelPickUpRequests { get; set; } = [];
}
