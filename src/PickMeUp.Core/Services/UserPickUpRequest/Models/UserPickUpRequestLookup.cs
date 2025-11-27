using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;

namespace PickMeUp.Core.Services.UserPickUpRequest;

public class UserPickUpRequestLookup
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
    /// Nominative of the user associated with this pick up request.
    /// </summary>
    public string UserNominative { get; set; } = default!;
    
    /// <summary>
    /// Location associated with this pick up request.
    /// </summary>
    public LocationLookup Location { get; set; } = default!;

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}
