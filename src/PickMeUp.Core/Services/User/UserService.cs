using PickMeUp.Core.Common.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.User;

internal class UserService : IUserService
{
    /// <inheritdoc/>
    public Task<Result> DeleteUserAsync(DeleteEntityParams<int> requestParams)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result> EditUserAsync(EditEntityParams<User> requestParams)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<User>> GetUserAsync(GetEntityParams<int> requestParams)
    {
        throw new System.NotImplementedException();
    }
}