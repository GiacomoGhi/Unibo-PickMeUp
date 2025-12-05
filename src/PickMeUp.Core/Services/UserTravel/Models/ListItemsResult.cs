
using PickMeUp.Core.Common.Models;

namespace PickMeUp.Core.Services.UserTravel;

public class ListUserTravelResult : ListItemsResult<UserTravelList>
{
    /// <summary>
    /// Total number of user travels with pending pick-up requests.
    /// </summary>
    public int TotalTravelsWithPendingPickUpRequestsCount { get; set; }

    /// <summary>
    /// Total number of travels where the user is a driver.
    /// </summary>
    public int TotalTravelsAsDriver { get; set; }

    /// <summary>
    /// Total number of travels where the user is a guest.
    /// </summary>
    public int TotalTravelsAsGuest { get; set; }
}
