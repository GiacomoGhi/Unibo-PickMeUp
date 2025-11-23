using PickMeUp.Enums.UserPickUpRequest;

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
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}
