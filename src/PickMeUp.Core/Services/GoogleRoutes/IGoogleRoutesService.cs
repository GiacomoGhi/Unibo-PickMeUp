using System.Threading.Tasks;
using PickMeUp.Core.Common.Models;

namespace PickMeUp.Core.Services.GoogleRoutes;

public interface IGoogleRoutesService
{
    /// <summary>
    /// Returns a route based on the specified parameters.
    /// </summary>
    Task<Result<GoogleRoute>> GetRouteAsync(GetRouteParams requestParams);
}