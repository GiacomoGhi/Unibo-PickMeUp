using PickMeUp.Enums.UserPickUpRequest;
using PickMeUp.Web.Models.Shared;

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
    /// Location of the pick up point location.
    /// </summary>
    public LocationViewModel Location { get; set; } = default!;

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}