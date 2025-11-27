using PickMeUp.Enums.UserPickUpRequest;
using PickMeUp.Web.Models.Shared;

namespace PickMeUp.Web.Models.Travel;

public class PickUpRequestLookupViewModel
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int PickUpRequestId { get; set; }

    /// <summary>
    /// Identifier of the user associated with this pick up request.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Nominative of the user associated with this pick up request.
    /// </summary>
    public string UserNominative { get; set; } = default!;
    
    /// <summary>
    /// Location associated with this pick up request.
    /// </summary>
    public LocationLookupViewModel Location { get; set; } = default!;

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}
