using PickMeUp.Enums.UserPickUpRequest;
using System;

namespace PickMeUp.Core.Services.UserPickUpRequest;

public class UserPickUpRequest
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int UserPickUpRequestId { get; set; }

    /// <summary>
    /// Identifier of the user associated with this pick up request.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Identifier of the travel that this request is for.
    /// </summary>
    public int UserTravelId { get; set; }

    /// <summary>
    /// Date and time for the pick up.
    /// </summary>
    public DateTime PickUpDateTime { get; set; }

    /// <summary>
    /// Latitude coordinate of the pick up point location.
    /// </summary>
    public double PickUpPointLatitude { get; set; }

    /// <summary>
    /// Longitude coordinate of the pick up point location.
    /// </summary>
    public double PickUpPointLongitude { get; set; }

    /// <summary>
    /// Address of the pick up point location.
    /// </summary>
    public string PickUpPointAddress { get; set; } = default!;

    /// <summary>
    /// Distance in kilometers from the pick up point to the destination.
    /// </summary>
    public decimal PickUpPointToDestinationDistanceInKm { get; set; }

    /// <summary>
    /// Costs refund for the pick up that the user will pay to
    /// the driver.
    /// </summary>
    public decimal CostsRefund { get; set; }

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}
