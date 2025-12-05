using System;

namespace PickMeUp.Core.Services.UserTravel;

public class UserTravelList
{
    /// <summary>
    /// User travel identifier.
    /// </summary>
    public int UserTravelId { get; set; }

    /// <summary
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
