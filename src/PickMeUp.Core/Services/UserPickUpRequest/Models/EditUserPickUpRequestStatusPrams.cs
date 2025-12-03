using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;
using System.Collections.Generic;

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

public class EditUserPickUpRequestStatusBulkParams : ParamsBase
{
    /// <summary>
    /// Identifiers of the pick up requests.
    /// </summary>
    public List<int> UserPickUpRequestIds { get; set; } = [];

    /// <summary>
    /// Status of the pick up requests.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}