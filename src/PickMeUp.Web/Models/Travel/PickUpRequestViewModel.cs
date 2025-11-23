using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;

namespace PickMeUp.Web.Models.Travel;

public class PickUpRequestViewModel
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int PickUpRequestId { get; set; }

    /// <summary>
    /// Identifier of the travel associated with this pick up request.
    /// </summary>
    public int TravelId { get; set; }

    /// <summary>
    /// Nominative of the user associated with this pick up request.
    /// </summary>
    public string? UserNominative { get; set; }

    /// <summary>
    /// Address of the pick up point location.
    /// </summary>
    public string PickUpPointAddress { get; set; } = default!;

    /// <summary>
    /// Longitude of the pick up point location.
    /// </summary>
    public Coordinates Location { get; set; } = default!;

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}