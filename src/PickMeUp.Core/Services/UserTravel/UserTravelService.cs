using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Extensions;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.UserPickUpRequest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.UserTravel;

internal class UserTravelService(PickMeUpDbContext dbContext) : IUserTravelService
{
    private readonly PickMeUpDbContext _dbContext = dbContext;

    /// <inheritdoc/>
    public async Task<Result<ListItemsResult<UserTravelList>>> ListUserTravelAsync(ListItemsParams requestParams)
    {
        // Validate parameters
        // if (requestParams.Skip < 0)
        // {
        //     return Result.InvalidArgument(nameof(requestParams.Skip));
        // }
        // if (requestParams.Take < 1)
        // {
        //     return Result.InvalidArgument(nameof(requestParams.Take));
        // }

        // Build query
        var query = _dbContext.UserTravels
            .AsNoTracking()
            .Where(travel => !travel.DeletionDateTime.HasValue)
            .OrderByDescending(travel => travel.DepartureDateTime);

        // Get total count
        var totalCount = await query.CountAsync();

        // Get paginated results
        var items = await query
            // .Skip(requestParams.Skip)
            // .Take(requestParams.Take)
            .Select(travel => new UserTravelList
            {
                UserTravelId = travel.UserTravelId,
                UserNominative = $"{travel.User.FirstName} {travel.User.LastName}",
                TotalPassengersSeatsCount = travel.TotalPassengersSeatsCount,
                OccupiedPassengerSeatsCount = travel.OccupiedPassengerSeatsCount,
                DepartureAddress = travel.DepartureAddress,
                DepartureDateTime = travel.DepartureDateTime,
                DestinationAddress = travel.DestinationAddress,
            })
            .ToArrayAsync();

        return Result.Success(new ListItemsResult<UserTravelList>
        {
            Items = items,
            TotalCount = totalCount
        });
    }

    /// <inheritdoc/>
    public async Task<Result<UserTravel>> GetUserTravelAsync(GetEntityParams<int> requestParams)
    {
        // Validate parameters
        if (requestParams.EntityId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.EntityId));
        }

        // Find travel
        var loadResult = await LoadUserTravelAsync(
            userTravelId: requestParams.EntityId,
            userId: null,
            useNoTracking: true);

        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }

        // Load associated pick-up requests
        var pickUpRequests = await _dbContext.UserPickUpRequests
            .AsNoTracking()
            .Where(request => request.UserTravelId == requestParams.EntityId
                           && !request.DeletionDateTime.HasValue)
            .Select(request => new UserPickUpRequestLookupItem
            {
                UserPickUpRequestId = request.UserPickUpRequestId,
                UserId = request.UserId,
                Status = request.Status,
                PickUpPointLatitude = request.PickUpPointLatitude,
                PickUpPointLongitude = request.PickUpPointLongitude,
                PickUpPointAddress = request.PickUpPointAddress,
            })
            .ToArrayAsync();

        // Load involved users nominatives
        var travel = loadResult.Data!;
        var involvedUserIds = pickUpRequests
            .Select(request => request.UserId)
            .Append(travel.UserId)
            .Distinct()
            .ToList();

        // Load involved users nominatives
        var involvedUsersStore = await _dbContext.Users
            .Where(user => involvedUserIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId, user => $"{user.FirstName} {user.LastName}");

        foreach (var request in pickUpRequests)
        {
            request.UserNominative = involvedUsersStore[request.UserId];
        }

        return Result.Success(
            new UserTravel
            {
                OwnerUserId = travel.UserId,
                UserTravelId = travel.UserTravelId,
                UserNominative = involvedUsersStore[travel.UserId],
                TotalPassengersSeatsCount = travel.TotalPassengersSeatsCount,
                OccupiedPassengerSeatsCount = travel.OccupiedPassengerSeatsCount,
                DepartureLatitude = travel.DepartureLatitude,
                DepartureLongitude = travel.DepartureLongitude,
                DepartureAddress = travel.DepartureAddress,
                DepartureDateTime = travel.DepartureDateTime,
                DestinationLatitude = travel.DestinationLatitude,
                DestinationLongitude = travel.DestinationLongitude,
                DestinationAddress = travel.DestinationAddress,
                TravelPickUpRequests = pickUpRequests
            });
    }

    /// <inheritdoc/>
    public async Task<Result<EditEntityResult<int>>> EditUserTravelAsync(EditEntityParams<UserTravel> requestParams)
    {
        // Validate parameters
        if (requestParams.Entity is null)
        {
            return Result.InvalidArgument(nameof(requestParams.Entity));
        }

        var receivedUserTravel = requestParams.Entity;

        // Validate required fields
        if (receivedUserTravel.TotalPassengersSeatsCount <= 0)
        {
            return Result.InvalidArgument(nameof(receivedUserTravel.TotalPassengersSeatsCount));
        }
        if (string.IsNullOrWhiteSpace(receivedUserTravel.DepartureAddress))
        {
            return Result.InvalidArgument(nameof(receivedUserTravel.DepartureAddress));
        }
        if (string.IsNullOrWhiteSpace(receivedUserTravel.DestinationAddress))
        {
            return Result.InvalidArgument(nameof(receivedUserTravel.DestinationAddress));
        }

        // Check if creating new or editing existing
        DatabaseModels.UserTravel travelEntity;
        if (receivedUserTravel.UserTravelId <= 0)
        {
            // Create new travel with minimal data
            if (requestParams.UserId <= 0)
            {
                return Result.InvalidArgument(nameof(requestParams.UserId));
            }

            travelEntity = new DatabaseModels.UserTravel
            {
                UserId = requestParams.UserId,
            };

            // Add to context
            _dbContext.UserTravels.Add(travelEntity);
        }
        else
        {
            // Load existing travel
            var loadResult = await LoadUserTravelAsync(receivedUserTravel.UserTravelId, requestParams.UserId);
            if (loadResult.HasNonSuccessStatusCode)
            {
                return Result.Error(loadResult.ErrorMessage);
            }

            travelEntity = loadResult.Data!;
        }

        // Update fields
        travelEntity.TotalPassengersSeatsCount = receivedUserTravel.TotalPassengersSeatsCount;
        travelEntity.DepartureLatitude = receivedUserTravel.DepartureLatitude;
        travelEntity.DepartureLongitude = receivedUserTravel.DepartureLongitude;
        travelEntity.DepartureAddress = receivedUserTravel.DepartureAddress.Trim();
        travelEntity.DepartureDateTime = new DateTime(receivedUserTravel.DepartureDateTime.Ticks, DateTimeKind.Utc);
        travelEntity.DestinationLatitude = receivedUserTravel.DestinationLatitude;
        travelEntity.DestinationLongitude = receivedUserTravel.DestinationLongitude;
        travelEntity.DestinationAddress = receivedUserTravel.DestinationAddress.Trim();

        // Save changes
        await _dbContext.SaveChangesAsync();

        return Result.Success(
            new EditEntityResult<int>
            {
                EntityId = travelEntity.UserTravelId
            });
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteUserTravelAsync(DeleteEntityParams<int> requestParams)
    {
        // Validate parameters
        if (requestParams.EntityId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.EntityId));
        }

        // Find travel
        var travel = await _dbContext.UserTravels
            .Where(travel => travel.UserTravelId == requestParams.EntityId
                          && travel.UserId == requestParams.UserId
                          && !travel.DeletionDateTime.HasValue)
            .FirstOrDefaultAsync();

        if (travel is null)
        {
            return Result.NotFound("UserTravel");
        }

        // Soft delete
        travel.DeletionDateTime = DateTime.UtcNow;

        // Save changes
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    /// <summary>
    /// Load user travel along with associated pick-up requests.
    /// </summary>
    private async Task<Result<DatabaseModels.UserTravel>> LoadUserTravelAsync(int userTravelId, int? userId, bool useNoTracking = false)
    {
        // Load user travel
        var userTravel = await _dbContext.UserTravels
            .AsNoTracking(useNoTracking)
            .Where(travel => travel.UserTravelId == userTravelId
                          && !travel.DeletionDateTime.HasValue
                          && (!userId.HasValue || travel.UserId == userId))
            .FirstOrDefaultAsync();

        // Check entity existence
        if (userTravel is null)
        {
            return Result.NotFound("UserTravel");
        }

        return Result.Success(userTravel);
    }
}
