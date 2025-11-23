using System;
using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Web.Models.Travel;

public class TravelRouteInfoViewModel
{
    public string EncodedPolyline { get; set; } = default!;
    public long DistanceMeters { get; set; }
    public long DurationSeconds { get; set; }
}

public class UserTravelViewModel
{
    public string? UserNominative { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int UserTravelId { get; set; }

    [Required]
    public string DeparturePlaceId { get; set; } = default!;

    [Required]
    public string DepartureAddress { get; set; } = default!;

    [Required]
    public double DepartureLat { get; set; }

    [Required]
    public double DepartureLng { get; set; }

    [Required]
    public DateOnly DepartureDate { get; set; }

    [Required]
    public TimeOnly DepartureTime { get; set; }

    [Required]
    public string DestinationPlaceId { get; set; } = default!;

    [Required]
    public string DestinationAddress { get; set; } = default!;

    [Required]
    public double DestinationLat { get; set; }

    [Required]
    public double DestinationLng { get; set; }

    public TravelRouteInfoViewModel? Route { get; set; }

    public PickUpRequestViewModel[] PickUpRequests { get; set; } = [];
}
