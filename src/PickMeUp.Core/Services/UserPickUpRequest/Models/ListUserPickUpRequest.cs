using PickMeUp.Enums.UserPickUpRequest;

namespace PickMeUp.Core.Services.UserPickUpRequest;

public class ListUserPickUpRequest
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int UserPickUpRequestId { get; set; }

    /// <summary>
    /// Nominative of the user that created the travel.
    /// </summary>
    public string UserNominative { get; set; } = default!;

    /// <summary>
    /// Address of the pick up point location.
    /// </summary>
    public string PickUpPointAddress { get; set; } = default!;

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}
