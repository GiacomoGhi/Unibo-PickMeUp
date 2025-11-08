using PickMeUp.Core.Common.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.User;

public interface IUserService
{
    /// <summary>
    /// Get the user's profile information.
    /// </summary>
    Task<Result<User>> GetUserAsync(GetEntityParams<int> requestParams);

    /// <summary>
    /// Edit the user's profile information.
    /// </summary>
    Task<Result> EditUserAsync(EditEntityParams<User> requestParams);

    /// <summary>
    /// Delete the user's profile.
    /// </summary>
    Task<Result> DeleteUserAsync(DeleteEntityParams<int> requestParams);
}