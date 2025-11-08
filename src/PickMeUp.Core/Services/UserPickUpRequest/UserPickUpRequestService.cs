using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Extensions;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.Email;
using PickMeUp.Enums.UserPickUpRequest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.UserPickUpRequest;

internal class UserPickUpRequestService(
    PickMeUpDbContext dbContext,
    IEmailService emailService) : IUserPickUpRequestService
{
    private readonly PickMeUpDbContext _dbContext = dbContext;
    private readonly IEmailService _emailService = emailService;

    /// <inheritdoc/>
    public async Task<ListItemsResult<ListUserPickUpRequest>> ListUserPickUpRequestAsync(ListItemsParams requestParams)
    {
        // Build query
        var query = _dbContext.UserPickUpRequests
            .AsNoTracking()
            .Where(request => !request.DeletionDateTime.HasValue)
            .OrderByDescending(request => request.PickUpDateTime);

        // Get total count
        var totalCount = await query.CountAsync();

        // Get paginated results
        var items = await query
            .Skip(requestParams.Skip)
            .Take(requestParams.Take)
            .Select(request => new ListUserPickUpRequest
            {
                UserPickUpRequestId = request.UserPickUpRequestId,
                UserNominative = $"{request.User.FirstName} {request.User.LastName}",
                PickUpPointAddress = request.PickUpPointAddress,
                Status = request.Status
            })
            .ToArrayAsync();

        return new ListItemsResult<ListUserPickUpRequest>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<Result<UserPickUpRequest>> GetUserPickUpRequest(GetEntityParams<int> requestParams)
    {
        // Validate parameters
        if (requestParams.EntityId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.EntityId));
        }

        // Load pickup request
        var loadResult = await LoadUserPickUpRequestAsync(
            userPickUpRequestId: requestParams.EntityId,
            userId: null,
            useNoTracking: true);

        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }

        var pickUpRequest = loadResult.Data!;

        return Result.Success(new UserPickUpRequest
        {
            UserPickUpRequestId = pickUpRequest.UserPickUpRequestId,
            UserId = pickUpRequest.UserId,
            UserTravelId = pickUpRequest.UserTravelId,
            PickUpDateTime = pickUpRequest.PickUpDateTime,
            PickUpPointLatitude = pickUpRequest.PickUpPointLatitude,
            PickUpPointLongitude = pickUpRequest.PickUpPointLongitude,
            PickUpPointAddress = pickUpRequest.PickUpPointAddress,
            PickUpPointToDestinationDistanceInKm = pickUpRequest.PickUpPointToDestinationDistanceInKm,
            CostsRefund = pickUpRequest.CostsRefund,
            Status = pickUpRequest.Status
        });
    }

    /// <inheritdoc/>
    public async Task<Result> EditUserPickUpRequestAsync(EditEntityParams<UserPickUpRequest> requestParams)
    {
        var receivedPickUpRequest = requestParams.Entity;

        // Validate required fields
        if (receivedPickUpRequest.UserTravelId <= 0)
        {
            return Result.InvalidArgument(nameof(receivedPickUpRequest.UserTravelId));
        }
        if (string.IsNullOrWhiteSpace(receivedPickUpRequest.PickUpPointAddress))
        {
            return Result.InvalidArgument(nameof(receivedPickUpRequest.PickUpPointAddress));
        }

        DatabaseModels.UserPickUpRequest pickUpRequestEntity;

        // Check if creating new or editing existing
        if (receivedPickUpRequest.UserPickUpRequestId <= 0)
        {
            // CREATE: Validate user
            if (requestParams.UserId <= 0)
            {
                return Result.InvalidArgument(nameof(requestParams.UserId));
            }

            // Verify travel exists and is not departed
            var travel = await _dbContext.UserTravels
                .AsNoTracking()
                .Where(travel => travel.UserTravelId == receivedPickUpRequest.UserTravelId
                              && !travel.DeletionDateTime.HasValue)
                .FirstOrDefaultAsync();

            if (travel is null)
            {
                return Result.NotFound("UserTravel");
            }

            if (travel.DepartureDateTime <= DateTime.UtcNow)
            {
                return Result.Error("Cannot create request for a travel that has already departed");
            }

            // User cannot request pickup for their own travel
            if (travel.UserId == requestParams.UserId)
            {
                return Result.Error("Cannot create pickup request for your own travel");
            }

            // Create new pickup request
            pickUpRequestEntity = new DatabaseModels.UserPickUpRequest
            {
                UserId = requestParams.UserId,
                UserTravelId = receivedPickUpRequest.UserTravelId,
                Status = UserPickUpRequestStatus.Pending
            };

            _dbContext.UserPickUpRequests.Add(pickUpRequestEntity);

            // Load both users in a single query
            var userIds = new[] { travel.UserId, requestParams.UserId };
            var users = await _dbContext.Users
                .Where(user => userIds.Contains(user.UserId))
                .ToDictionaryAsync(user => user.UserId);

            if (users.TryGetValue(travel.UserId, out var travelOwner) 
                && users.TryGetValue(requestParams.UserId, out var requester))
            {
                await _emailService.SendPickUpRequestReceivedAsync(
                    travelOwner.Email,
                    travelOwner.FirstName,
                    requester.FirstName,
                    requester.LastName,
                    travel.DepartureAddress,
                    travel.DestinationAddress,
                    receivedPickUpRequest.PickUpPointAddress);
            }
        }
        else
        {
            // EDIT: Load existing request
            var loadResult = await LoadUserPickUpRequestAsync(receivedPickUpRequest.UserPickUpRequestId, requestParams.UserId);
            if (loadResult.HasNonSuccessStatusCode)
            {
                return Result.Error(loadResult.ErrorMessage);
            }

            pickUpRequestEntity = loadResult.Data!;

            // Cannot edit if already accepted or rejected
            if (pickUpRequestEntity.Status != UserPickUpRequestStatus.Pending)
            {
                return Result.Error("Cannot edit a request that has already been processed");
            }
        }

        // Update fields
        pickUpRequestEntity.PickUpDateTime = receivedPickUpRequest.PickUpDateTime;
        pickUpRequestEntity.PickUpPointLatitude = receivedPickUpRequest.PickUpPointLatitude;
        pickUpRequestEntity.PickUpPointLongitude = receivedPickUpRequest.PickUpPointLongitude;
        pickUpRequestEntity.PickUpPointAddress = receivedPickUpRequest.PickUpPointAddress.Trim();
        pickUpRequestEntity.PickUpPointToDestinationDistanceInKm = receivedPickUpRequest.PickUpPointToDestinationDistanceInKm;
        pickUpRequestEntity.CostsRefund = receivedPickUpRequest.CostsRefund;

        // Save changes
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> EditUserPickUpRequestStatusAsync(EditUserPickUpRequestStatusParams requestParams)
    {
        // Validate parameters
        if (requestParams.UserPickUpRequestId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.UserPickUpRequestId));
        }
        if (requestParams.UserId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.UserId));
        }

        // Load pickup request
        var loadResult = await LoadUserPickUpRequestAsync(
            userPickUpRequestId: requestParams.UserPickUpRequestId,
            userId: null,
            useNoTracking: false);

        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }

        var pickupRequest = loadResult.Data!;

        // Check that user is NOT the owner of the pickup request
        if (pickupRequest.UserId == requestParams.UserId)
        {
            return Result.Unauthorized();
        }

        // Load associated travel
        var travel = await _dbContext.UserTravels
            .Where(travel => travel.UserTravelId == pickupRequest.UserTravelId
                          && !travel.DeletionDateTime.HasValue)
            .FirstOrDefaultAsync();

        if (travel is null)
        {
            return Result.NotFound("UserTravel");
        }

        // Check that user IS the owner of the travel
        if (travel.UserId != requestParams.UserId)
        {
            return Result.Unauthorized();
        }

        // Check that travel has not already departed
        if (travel.DepartureDateTime <= DateTime.UtcNow)
        {
            return Result.Error("Cannot modify request for a travel that has already departed");
        }

        // If accepting request, check available seats
        if (requestParams.Status == UserPickUpRequestStatus.Accepted)
        {
            var availableSeats = travel.TotalPassengersSeatsCount - travel.OccupiedPassengerSeatsCount;

            if (availableSeats <= 0)
            {
                return Result.Error("No available seats for this travel");
            }

            // Increment occupied seats
            travel.OccupiedPassengerSeatsCount++;
        }

        // If rejecting a previously accepted request, decrement occupied seats
        if (requestParams.Status == UserPickUpRequestStatus.Rejected 
            && pickupRequest.Status == UserPickUpRequestStatus.Accepted)
        {
            travel.OccupiedPassengerSeatsCount--;
        }

        // Update status
        var previousStatus = pickupRequest.Status;
        pickupRequest.Status = requestParams.Status;

        // Save changes
        await _dbContext.SaveChangesAsync();

        // Load both users in a single query
        var userIds = new[] { pickupRequest.UserId, travel.UserId };
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId);

        if (users.TryGetValue(pickupRequest.UserId, out var requester) 
            && users.TryGetValue(travel.UserId, out var travelOwner))
        {
            await _emailService.SendPickUpRequestStatusChangedAsync(
                requester.Email,
                requester.FirstName,
                travelOwner.FirstName,
                requestParams.Status,
                travel.DepartureAddress,
                travel.DestinationAddress,
                travel.DepartureDateTime);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteUserPickUpRequestAsync(DeleteEntityParams<int> requestParams)
    {
        // Validate parameters
        if (requestParams.EntityId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.EntityId));
        }
        if (requestParams.UserId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.UserId));
        }

        // Load pickup request
        var pickupRequest = await _dbContext.UserPickUpRequests
            .Where(request => request.UserPickUpRequestId == requestParams.EntityId
                           && request.UserId == requestParams.UserId
                           && !request.DeletionDateTime.HasValue)
            .FirstOrDefaultAsync();

        if (pickupRequest is null)
        {
            return Result.NotFound("UserPickUpRequest");
        }

        var wasAccepted = pickupRequest.Status == UserPickUpRequestStatus.Accepted;

        // If request was accepted, need to free up the seat
        if (wasAccepted)
        {
            var travel = await _dbContext.UserTravels
                .Where(travel => travel.UserTravelId == pickupRequest.UserTravelId
                              && !travel.DeletionDateTime.HasValue)
                .FirstOrDefaultAsync();

            if (travel is not null)
            {
                travel.OccupiedPassengerSeatsCount--;

                // Load both users in a single query
                var userIds = new[] { travel.UserId, pickupRequest.UserId };
                var users = await _dbContext.Users
                    .AsNoTracking()
                    .Where(user => userIds.Contains(user.UserId))
                    .ToDictionaryAsync(user => user.UserId);

                if (users.TryGetValue(travel.UserId, out var travelOwner) 
                    && users.TryGetValue(pickupRequest.UserId, out var requester))
                {
                    await _emailService.SendPickUpRequestCancelledAsync(
                        travelOwner.Email,
                        travelOwner.FirstName,
                        requester.FirstName,
                        requester.LastName,
                        travel.DepartureAddress,
                        travel.DestinationAddress,
                        travel.DepartureDateTime);
                }
            }
        }

        // Soft delete
        pickupRequest.DeletionDateTime = DateTime.UtcNow;

        // Save changes
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    /// <summary>
    /// Load user pickup request by ID with optional user validation.
    /// </summary>
    private async Task<Result<DatabaseModels.UserPickUpRequest>> LoadUserPickUpRequestAsync(
        int userPickUpRequestId,
        int? userId,
        bool useNoTracking = false)
    {
        // Build query
        var pickupRequest = await _dbContext.UserPickUpRequests
            .AsNoTracking(useNoTracking)
            .Where(request => request.UserPickUpRequestId == userPickUpRequestId
                           && !request.DeletionDateTime.HasValue
                           && (!userId.HasValue || request.UserId == userId))
            .FirstOrDefaultAsync();

        if (pickupRequest is null)
        {
            return Result.NotFound("UserPickUpRequest");
        }

        return Result.Success(pickupRequest);
    }
}