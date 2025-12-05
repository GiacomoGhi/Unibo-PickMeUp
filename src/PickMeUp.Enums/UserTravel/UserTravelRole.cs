namespace PickMeUp.Enums.UserTravel;

public enum UserTravelRole
{
    /// <summary>
    /// Any role in the travel.
    /// </summary>
    Any = 0,

    /// <summary>
    /// The user is the driver of the travel.
    /// </summary>
    Driver,

    /// <summary>
    /// The user is a guest in the travel.
    /// </summary>
    Guest,
}