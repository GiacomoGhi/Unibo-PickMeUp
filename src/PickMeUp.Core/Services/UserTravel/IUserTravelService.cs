using PickMeUp.Core.Common.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.UserTravel;

public interface IUserTravelService
{
    /// <summary>
    /// Provides a list of user travels according to the specified parameters.
    /// </summary>
    public Task<Result<ListItemsResult<UserTravelList>>> ListUserTravelAsync(ListItemsParams requestParams);

    /// <summary>
    /// Returns a user travel.
    /// </summary>
    public Task<Result<UserTravel>> GetUserTravelAsync(GetEntityParams<int> requestParams);

    /// <summary>
    /// Edits the requested user travel.
    /// </summary>
    public Task<Result<EditEntityResult<int>>> EditUserTravelAsync(EditEntityParams<UserTravel> requestParams);

    /// <summary>
    /// Deletes the requested user travel.
    /// </summary>
    public Task<Result> DeleteUserTravelAsync(DeleteEntityParams<int> requestParams);
}
