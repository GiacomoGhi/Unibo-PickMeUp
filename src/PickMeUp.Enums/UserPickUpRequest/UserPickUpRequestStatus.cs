namespace PickMeUp.Enums.UserPickUpRequest;

public enum UserPickUpRequestStatus
{
    /// <summary>
    /// Request is created and awaiting a decision.
    /// </summary>
    Pending,

    /// <summary>
    /// Accepted request.
    /// </summary>
    Accepted,

    /// <summary>
    /// Rejected request.
    /// </summary>
    Rejected,
}
