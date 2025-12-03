using PickMeUp.Core.Common.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.UserPickUpRequest;

public interface IUserPickUpRequestService
{
    /// <summary>
    /// Provides a list of user pick-up requests based on the specified parameters.
    /// </summary>
    public Task<ListItemsResult<ListUserPickUpRequest>> ListUserPickUpRequestAsync(ListItemsParams requestParams);

    /// <summary>
    /// Provides the requested user pick-up request based on the specified parameters.
    /// </summary>
    public Task<Result<UserPickUpRequest>> GetUserPickUpRequest(GetEntityParams<int> requestParams);

    /// <summary>
    /// Edit the requested user pick-up request.
    /// </summary>
    public Task<Result> EditUserPickUpRequestAsync(EditEntityParams<UserPickUpRequest> requestParams);

    /// <summary>
    /// Edit the status of the requested user pick-up request.
    /// </summary>
    public Task<Result> EditUserPickUpRequestStatusAsync(EditUserPickUpRequestStatusParams requestParams);

    /// <summary>
    /// Edit the status of multiple user pick-up requests in bulk.
    /// </summary>
    public Task<Result> EditUserPickUpRequestStatusBulkAsync(EditUserPickUpRequestStatusBulkParams requestParams);

    /// <summary>
    /// Deletes the requested user pick-up request.
    /// </summary>
    public Task<Result> DeleteUserPickUpRequestAsync(DeleteEntityParams<int> requestParams);
}
