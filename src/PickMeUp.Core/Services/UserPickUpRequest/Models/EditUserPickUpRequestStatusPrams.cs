using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;

namespace PickMeUp.Core.Services.UserPickUpRequest;

public class EditUserPickUpRequestStatusParams : ParamsBase
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int UserPickUpRequestId { get; set; }

    /// <summary>
    /// Status of the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}