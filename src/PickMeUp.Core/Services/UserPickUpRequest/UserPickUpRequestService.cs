using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Extensions;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.Email;
using PickMeUp.Enums.UserPickUpRequest;
using System;
using System.Collections.Generic;
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
            .Where(request => !request.DeletionDateTime.HasValue);

        // Get total count
        var totalCount = await query.CountAsync();

        // Get paginated results
        var pickUpRequests = await query
            // .Skip(requestParams.Skip)
            // .Take(requestParams.Take)
            .Select(pickUpRequest => new
            {
                pickUpRequest.UserPickUpRequestId,
                pickUpRequest.Status,
                pickUpRequest.LocationId,
                UserNominative = $"{pickUpRequest.User.FirstName} {pickUpRequest.User.LastName}",
            })
            .ToListAsync();

        var involvedLocationIds = pickUpRequests
            .Select(r => r.LocationId)
            .Distinct();

        var locations = await _dbContext.Locations
            .AsNoTracking()
            .Where(location => involvedLocationIds.Contains(location.LocationId))
            .ToDictionaryAsync(
                location => location.LocationId,
                location => location.ReadableAddress);
    
        return new ListItemsResult<ListUserPickUpRequest>
        {
            Items = [.. pickUpRequests
                .Select(r => new ListUserPickUpRequest
                {
                    UserPickUpRequestId = r.UserPickUpRequestId,
                    Status = r.Status,
                    UserNominative = r.UserNominative,
                    PickUpPointAddress = locations.GetValueOrDefault(r.LocationId) ?? "Unknown Address",
                })],
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
        var loadResult = await this.LoadUserPickUpRequestAsync(
            userPickUpRequestId: requestParams.EntityId,
            userId: null,
            useNoTracking: true);

        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }

        var pickUpRequestData = loadResult.Data!;

        return Result.Success(new UserPickUpRequest
        {
            UserPickUpRequestId = pickUpRequestData.PickUpRequest.UserPickUpRequestId,
            UserId = pickUpRequestData.PickUpRequest.UserId,
            UserTravelId = pickUpRequestData.PickUpRequest.UserTravelId,
            Location = pickUpRequestData.Location.ToServiceModel(),
            Status = pickUpRequestData.PickUpRequest.Status
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
        if (string.IsNullOrWhiteSpace(receivedPickUpRequest.Location.ReadableAddress))
        {
            return Result.InvalidArgument(nameof(receivedPickUpRequest.Location.ReadableAddress));
        }

        // Check if creating new or editing existing
        LoadUserPickUpRequestData loadPickUpRequestData;
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
                .Include(travel => travel.DepartureLocation)
                .Include(travel => travel.DestinationLocation)
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
            var location = new DatabaseModels.Location();
            loadPickUpRequestData = new()
            {
                PickUpRequest = new DatabaseModels.UserPickUpRequest
                {
                    UserId = requestParams.UserId,
                    UserTravelId = receivedPickUpRequest.UserTravelId,
                    Status = UserPickUpRequestStatus.Pending,
                    Location = location,
                },
                Location = location
            };

            _dbContext.UserPickUpRequests.Add(loadPickUpRequestData.PickUpRequest);

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
                    travel.DepartureLocation.ReadableAddress,
                    travel.DestinationLocation.ReadableAddress,
                    receivedPickUpRequest.Location.ReadableAddress);
            }
        }
        else
        {
            // EDIT: Load existing request
            var loadResult = await this.LoadUserPickUpRequestAsync(receivedPickUpRequest.UserPickUpRequestId, requestParams.UserId);
            if (loadResult.HasNonSuccessStatusCode)
            {
                return Result.Error(loadResult.ErrorMessage);
            }

            loadPickUpRequestData = loadResult.Data!;

            // Cannot edit if already accepted or rejected
            if (loadPickUpRequestData.PickUpRequest.Status != UserPickUpRequestStatus.Pending)
            {
                return Result.Error("Cannot edit a request that has already been processed");
            }
        }
        var locationEntity = loadPickUpRequestData.Location;

        // Update fields
        locationEntity.ReadableAddress = receivedPickUpRequest.Location.ReadableAddress;
        locationEntity.Latitude = receivedPickUpRequest.Location.Coordinates.Latitude;
        locationEntity.Longitude = receivedPickUpRequest.Location.Coordinates.Longitude;
        locationEntity.Street = receivedPickUpRequest.Location.Street;
        locationEntity.Number = receivedPickUpRequest.Location.Number;
        locationEntity.City = receivedPickUpRequest.Location.City;
        locationEntity.PostalCode = receivedPickUpRequest.Location.PostalCode;
        locationEntity.Province = receivedPickUpRequest.Location.Province;
        locationEntity.Region = receivedPickUpRequest.Location.Region;
        locationEntity.Country = receivedPickUpRequest.Location.Country;
        locationEntity.Continent = receivedPickUpRequest.Location.Continent;

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
        var loadResult = await this.LoadUserPickUpRequestAsync(
            userPickUpRequestId: requestParams.UserPickUpRequestId,
            userId: null,
            useNoTracking: false);

        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }

        var pickupRequestData = loadResult.Data!;

        // Check that user is NOT the owner of the pickup request
        var pickupRequest = pickupRequestData.PickUpRequest;
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
                travel.DepartureLocation.ReadableAddress,
                travel.DestinationLocation.ReadableAddress,
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
        var loadResult = await this.LoadUserPickUpRequestAsync(requestParams.EntityId, requestParams.UserId);
        if (loadResult.HasNonSuccessStatusCode)
        {
            return Result.Error(loadResult.ErrorMessage);
        }
        
        var pickupRequest = loadResult.Data!.PickUpRequest;

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

            var travelLocations = await _dbContext.Locations
                .Where(location => location.LocationId == travel!.DepartureLocationId
                              || location.LocationId == travel.DestinationLocationId)
                .ToDictionaryAsync(
                    location => location.LocationId,
                    location => location.ReadableAddress);

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
                        travelLocations[travel.DepartureLocationId],
                        travelLocations[travel.DestinationLocationId],
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
    private async Task<Result<LoadUserPickUpRequestData>> LoadUserPickUpRequestAsync(
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

        var location = await _dbContext.Locations
            .AsNoTracking(useNoTracking)
            .Where(location => location.LocationId == pickupRequest.LocationId)
            .FirstOrDefaultAsync();

        if (location is null)
        {
            return Result.NotFound("UserTravel");
        }

        return Result.Success(new LoadUserPickUpRequestData
        {
            PickUpRequest = pickupRequest,
            Location = location
        });
    }

    private class LoadUserPickUpRequestData
    {
        /// <summary>
        /// The loaded pick up request.
        /// </summary>
        public DatabaseModels.UserPickUpRequest PickUpRequest { get; set; } = default!;

        /// <summary>
        /// The associated location.
        /// </summary>
        public DatabaseModels.Location Location { get; set; } = default!;
    }
}