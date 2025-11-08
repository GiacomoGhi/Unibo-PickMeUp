using System;

namespace PickMeUp.Enums.UserTravel;

[Flags]
public enum UserTravelAdditionalEntityMask
{
    /// <summary>
    /// No mask to apply.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include user pick up requests associated with the travel.
    /// </summary>
    OnlyAcceptedUserPickUpRequests = 1 << 0,
}
