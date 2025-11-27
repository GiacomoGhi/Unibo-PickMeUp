using System;
using System.ComponentModel.DataAnnotations;
using PickMeUp.Web.Models.Shared;

namespace PickMeUp.Web.Models.Travel;

public class TravelRouteInfoViewModel
{
    public string EncodedPolyline { get; set; } = default!;
    public long DistanceMeters { get; set; }
    public long DurationSeconds { get; set; }
}

public class TravelViewModel    
{
    public string? UserNominative { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int UserTravelId { get; set; }

    [Required]
    public DateOnly DepartureDate { get; set; }

    [Required]
    public TimeOnly DepartureTime { get; set; }

    [Required]
    public LocationViewModel DepartureLocation { get; set; } = new();

    [Required]
    public LocationViewModel DestinationLocation { get; set; } = new();

    [Required]
    public int TotalPassengersSeatsCount { get; set; }

    [Required]
    public int OccupiedPassengerSeatsCount { get; set; }

    public TravelRouteInfoViewModel? Route { get; set; }

    public PickUpRequestLookupViewModel[] PickUpRequests { get; set; } = [];
}
