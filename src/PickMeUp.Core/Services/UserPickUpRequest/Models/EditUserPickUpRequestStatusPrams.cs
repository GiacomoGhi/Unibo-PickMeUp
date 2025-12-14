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
    /// List of pick up request status changes.
    /// </summary>
    public List<PickUpRequestStatusChange> StatusChanges { get; set; } = [];
}

/// <summary>
/// Represents a single pick up request status change.
/// </summary>
public class PickUpRequestStatusChange
{
    /// <summary>
    /// Identifier of the pick up request.
    /// </summary>
    public int UserPickUpRequestId { get; set; }

    /// <summary>
    /// New status for the pick up request.
    /// </summary>
    public UserPickUpRequestStatus Status { get; set; }
}