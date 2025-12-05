using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Extensions;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.UserPickUpRequest;
using PickMeUp.Enums.UserPickUpRequest;
using PickMeUp.Enums.UserTravel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.UserTravel;

internal class UserTravelService(PickMeUpDbContext dbContext) : IUserTravelService
{
    private readonly PickMeUpDbContext _dbContext = dbContext;

    /// <inheritdoc/>
    public async Task<Result<ListUserTravelResult>> ListUserTravelAsync(ListItemsParams requestParams)
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
        
        // Validate UserId
        if (requestParams.UserId <= 0)
        {
            return Result.InvalidArgument(nameof(requestParams.UserId));
        }

        // Build query
        var query = await CreateListQueryAsync(requestParams);

        // Get total count
        var totalCount = await query.CountAsync();

        // Project to lightweight anonymous objects first
        var travels = await query
            // .Skip(requestParams.Skip)
            // .Take(requestParams.Take)
            .Select(travel => new
            {
                travel.UserTravelId,
                travel.UserId,
                travel.TotalPassengersSeatsCount,
                travel.OccupiedPassengerSeatsCount,
                travel.DepartureLocationId,
                travel.DestinationLocationId,
                travel.DepartureDateTime,
                PickUpRequestsData = travel.UserPickUpRequests
                    .Where(request => !request.DeletionDateTime.HasValue)
                    .Select(request => 
                        new
                        {
                            request.UserId,
                            request.Status
                        })
            })
            .ToArrayAsync();

        // Load involved users' nominatives in a single query
        var involvedUserIds = travels.Select(t => t.UserId).Distinct();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(user => involvedUserIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId, user => $"{user.FirstName} {user.LastName}");

        // Load involved locations' readable addresses in a single query
        var involvedLocationIds = travels
            .SelectMany(t => new[] { t.DepartureLocationId, t.DestinationLocationId })
            .Distinct();

        var locations = await _dbContext.Locations
            .AsNoTracking()
            .Where(location => involvedLocationIds.Contains(location.LocationId))
            .ToDictionaryAsync(
                location => location.LocationId,
                location => location.ReadableAddress);

        // Get generic total count if needed
        var totalTravelsWithPendingPickUpRequestsCount = 0;
        var totalTravelsAsDriver = 0;
        var totalTravelsAsGuest = 0;
        if (!requestParams.IsFromFindTravel)
        {
            totalTravelsWithPendingPickUpRequestsCount = await _dbContext.UserTravels
                .AsNoTracking()
                .Where(travel => !travel.DeletionDateTime.HasValue
                              && travel.UserId == requestParams.UserId
                              && travel.UserPickUpRequests
                                  .Any(request => request.Status == UserPickUpRequestStatus.Pending
                                               && !request.DeletionDateTime.HasValue))
                .CountAsync();

            totalTravelsAsDriver = await _dbContext.UserTravels
                .AsNoTracking()
                .Where(travel => !travel.DeletionDateTime.HasValue
                              && travel.UserId == requestParams.UserId)
                .CountAsync();

            totalTravelsAsGuest = await _dbContext.UserTravels
                .AsNoTracking()
                .Where(travel => !travel.DeletionDateTime.HasValue
                              && travel.UserPickUpRequests
                                  .Any(request => request.UserId == requestParams.UserId
                                               && !request.DeletionDateTime.HasValue))
                .CountAsync();
        }

        // Map to service model
        var items = travels
            .Select(t => new UserTravelList
            {
                UserId = t.UserId,
                UserTravelId = t.UserTravelId,
                UserNominative = users.GetValueOrDefault(t.UserId) ?? "Unknown User",
                TotalPassengersSeatsCount = t.TotalPassengersSeatsCount,
                OccupiedPassengerSeatsCount = t.OccupiedPassengerSeatsCount,
                DepartureDateTime = t.DepartureDateTime,
                DepartureAddress = locations.GetValueOrDefault(t.DepartureLocationId) ?? "Unknown Location",
                DestinationAddress = locations.GetValueOrDefault(t.DestinationLocationId) ?? "Unknown Location",
                AcceptedPickUpRequestUserIds = [.. t.PickUpRequestsData
                    .Where(request => request.Status == UserPickUpRequestStatus.Accepted)
                    .Select(request => request.UserId)],
                PendingPickUpRequestUserIds = [.. t.PickUpRequestsData
                    .Where(request => request.Status == UserPickUpRequestStatus.Pending)
                    .Select(request => request.UserId)]
            })
            .ToArray();

        return Result.Success(new ListUserTravelResult
        {
            Items = items,
            TotalCount = totalCount,
            TotalTravelsWithPendingPickUpRequestsCount = totalTravelsWithPendingPickUpRequestsCount,
            TotalTravelsAsDriver = totalTravelsAsDriver,
            TotalTravelsAsGuest = totalTravelsAsGuest,
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

        // Load associated pick-up requests (project location as a lightweight lookup)
        var pickUpRequests = await _dbContext.UserPickUpRequests
            .AsNoTracking()
            .Where(request => request.UserTravelId == requestParams.EntityId
                           && !request.DeletionDateTime.HasValue)
            .Select(request => new UserPickUpRequestLookup
            {
                UserPickUpRequestId = request.UserPickUpRequestId,
                UserId = request.UserId,
                Status = request.Status,
                Location = new LocationLookup
                {
                    LocationId = request.Location.LocationId,
                    ReadableAddress = request.Location.ReadableAddress,
                    Coordinates = new Coordinates
                    {
                        Latitude = request.Location.Latitude,
                        Longitude = request.Location.Longitude
                    }
                }
            })
            .ToArrayAsync();

        // Load involved users nominatives
        var travelData = loadResult.Data!;
        var travel = travelData.Travel;
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
                DepartureLocation = travelData.DepartureLocation.ToServiceModel(),
                DepartureDateTime = travel.DepartureDateTime,
                DestinationLocation = travelData.DestinationLocation.ToServiceModel(),
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
        if (receivedUserTravel.DepartureLocation is null || string.IsNullOrWhiteSpace(receivedUserTravel.DepartureLocation.ReadableAddress))
        {
            return Result.InvalidArgument(nameof(receivedUserTravel.DepartureLocation));
        }
        if (receivedUserTravel.DestinationLocation is null || string.IsNullOrWhiteSpace(receivedUserTravel.DestinationLocation.ReadableAddress))
        {
            return Result.InvalidArgument(nameof(receivedUserTravel.DestinationLocation));
        }

        // Check if creating new or editing existing
        DatabaseModels.UserTravel travelEntity;
        DatabaseModels.Location departureLocationEntity;
        DatabaseModels.Location destinationLocationEntity;

        if (receivedUserTravel.UserTravelId <= 0)
        {
            // Create new travel with locations
            if (requestParams.UserId <= 0)
            {
                return Result.InvalidArgument(nameof(requestParams.UserId));
            }

            departureLocationEntity = new DatabaseModels.Location();
            destinationLocationEntity = new DatabaseModels.Location();

            travelEntity = new DatabaseModels.UserTravel
            {
                UserId = requestParams.UserId,
                DepartureLocation = departureLocationEntity,
                DestinationLocation = destinationLocationEntity
            };

            _dbContext.UserTravels.Add(travelEntity);
        }
        else
        {
            // Load existing travel (including locations)
            var loadResult = await LoadUserTravelAsync(receivedUserTravel.UserTravelId, requestParams.UserId);
            if (loadResult.HasNonSuccessStatusCode)
            {
                return Result.Error(loadResult.ErrorMessage);
            }

            var loaded = loadResult.Data!;
            travelEntity = loaded.Travel;
            departureLocationEntity = loaded.DepartureLocation;
            destinationLocationEntity = loaded.DestinationLocation;
        }

        // Update fields
        travelEntity.TotalPassengersSeatsCount = receivedUserTravel.TotalPassengersSeatsCount;
        travelEntity.DepartureDateTime = new DateTime(receivedUserTravel.DepartureDateTime.Ticks, DateTimeKind.Utc);

        // TODO move all this in a separate method, same for the UserPickUpRequestService
        // Update departure location fields
        departureLocationEntity.ReadableAddress = receivedUserTravel.DepartureLocation.ReadableAddress.Trim();
        departureLocationEntity.Latitude = receivedUserTravel.DepartureLocation.Coordinates.Latitude;
        departureLocationEntity.Longitude = receivedUserTravel.DepartureLocation.Coordinates.Longitude;
        departureLocationEntity.Street = receivedUserTravel.DepartureLocation.Street;
        departureLocationEntity.Number = receivedUserTravel.DepartureLocation.Number;
        departureLocationEntity.City = receivedUserTravel.DepartureLocation.City;
        departureLocationEntity.PostalCode = receivedUserTravel.DepartureLocation.PostalCode;
        departureLocationEntity.Province = receivedUserTravel.DepartureLocation.Province;
        departureLocationEntity.Region = receivedUserTravel.DepartureLocation.Region;
        departureLocationEntity.Country = receivedUserTravel.DepartureLocation.Country;
        departureLocationEntity.Continent = receivedUserTravel.DepartureLocation.Continent;

        // Update destination location fields
        destinationLocationEntity.ReadableAddress = receivedUserTravel.DestinationLocation.ReadableAddress.Trim();
        destinationLocationEntity.Latitude = receivedUserTravel.DestinationLocation.Coordinates.Latitude;
        destinationLocationEntity.Longitude = receivedUserTravel.DestinationLocation.Coordinates.Longitude;
        destinationLocationEntity.Street = receivedUserTravel.DestinationLocation.Street;
        destinationLocationEntity.Number = receivedUserTravel.DestinationLocation.Number;
        destinationLocationEntity.City = receivedUserTravel.DestinationLocation.City;
        destinationLocationEntity.PostalCode = receivedUserTravel.DestinationLocation.PostalCode;
        destinationLocationEntity.Province = receivedUserTravel.DestinationLocation.Province;
        destinationLocationEntity.Region = receivedUserTravel.DestinationLocation.Region;
        destinationLocationEntity.Country = receivedUserTravel.DestinationLocation.Country;
        destinationLocationEntity.Continent = receivedUserTravel.DestinationLocation.Continent;

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
    /// Load user travel along with its associated departure and destination locations (no includes).
    /// </summary>
    private async Task<Result<LoadUserTravelData>> LoadUserTravelAsync(int userTravelId, int? userId, bool useNoTracking = false)
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

        // Load departure location
        var departureLocation = await _dbContext.Locations
            .AsNoTracking(useNoTracking)
            .Where(l => l.LocationId == userTravel.DepartureLocationId)
            .FirstOrDefaultAsync();

        if (departureLocation is null)
        {
            return Result.NotFound("UserTravel");
        }

        // Load destination location
        var destinationLocation = await _dbContext.Locations
            .AsNoTracking(useNoTracking)
            .Where(l => l.LocationId == userTravel.DestinationLocationId)
            .FirstOrDefaultAsync();

        if (destinationLocation is null)
        {
            return Result.NotFound("UserTravel");
        }

        return Result.Success(new LoadUserTravelData
        {
            Travel = userTravel,
            DepartureLocation = departureLocation,
            DestinationLocation = destinationLocation
        });
    }
    
    /// <summary>
    /// Creates and configures the IQueryable for listing user travels based on the provided parameters.
    /// </summary>
    private async Task<IQueryable<DatabaseModels.UserTravel>> CreateListQueryAsync(ListItemsParams requestParams)
    {
        var query = _dbContext.UserTravels
            .AsNoTracking()
            .Where(travel => !travel.DeletionDateTime.HasValue);

        // Apply filters
        if (requestParams.IsFromFindTravel)
        {
            query = query
                .Where(travel => travel.UserId != requestParams.UserId);
        }
        else
        {
            // Filter only travels owned by the user and where there is a pending pick up request
            if (requestParams.ShowOnlyTravelsWithPendingPickUpRequests)
            {
                query = query
                    // Filter by user id
                    .Where(travel => travel.UserId == requestParams.UserId
                                    // Filter by pending pick up requests
                                  && travel.UserPickUpRequests
                                    .Any(request => request.Status == UserPickUpRequestStatus.Pending
                                                 && !request.DeletionDateTime.HasValue));

                return query;
            }
            
            // Filter by user role in the travel
            if (requestParams.ShowOnlyTravelsWithRole != UserTravelRole.Any)
            {
                // Driver role
                if (requestParams.ShowOnlyTravelsWithRole == UserTravelRole.Driver)
                {
                    query = query
                        .Where(travel => travel.UserId == requestParams.UserId);
                    return query;
                }

                // Guest role   
                query = query
                    .Where(travel => travel.UserPickUpRequests
                        .Any(request => request.UserId == requestParams.UserId
                                        && !request.DeletionDateTime.HasValue));
                return query;
            }
            
            // All travels where the user is involved both as driver or guest
            query = query
                .Where(travel => travel.UserId == requestParams.UserId
                                || travel.UserPickUpRequests
                                .Any(request => request.UserId == requestParams.UserId
                                                && !request.DeletionDateTime.HasValue));
        }

        // Apply Departure Date filter
        if (requestParams.DepartureDate.HasValue)
        {
            var targetDate = new DateTime(requestParams.DepartureDate.Value, TimeOnly.MinValue, DateTimeKind.Utc);
            query = query
                .Where(travel => travel.DepartureDateTime.Date >= targetDate);
        }
        
        // Apply Geographic Filters (Cascade)
        query = await ApplyGeographicFilterAsync(query, requestParams.DestinationLocation, isDeparture: false);
        query = await ApplyGeographicFilterAsync(query, requestParams.DepartureLocation, isDeparture: true);

        // Apply ordering
        query = query
            .OrderBy(travel => travel.DepartureDateTime);

        return query;
    }

    /// <summary>
    /// Applies a cascading geographic filter logic. It tries to match the exact address first, 
    /// then relaxes the constraints (Street -> City -> Province -> Region).
    /// </summary>
    private static async Task<IQueryable<DatabaseModels.UserTravel>> ApplyGeographicFilterAsync(
        IQueryable<DatabaseModels.UserTravel> query, 
        Location? filterLocation, 
        bool isDeparture)
    {
        // If no filter is provided, return the original query immediately.
        if (filterLocation == null)
        {
            return query;
        }

        var loc = filterLocation;
        var searchLevels = new List<Func<IQueryable<DatabaseModels.UserTravel>, IQueryable<DatabaseModels.UserTravel>>>();

        // Helper function to create the predicates based on the target column (Departure vs Destination)
        // We use this local function to avoid duplicating the logic for building the levels.
        void AddLevel(bool condition, System.Linq.Expressions.Expression<Func<DatabaseModels.UserTravel, bool>> predicate)
        {
            if (condition) searchLevels.Add(q => q.Where(predicate));
        }

        // Define Specificity Levels
        
        // Level 1: Exact Address (City + Street + Number)
        if (isDeparture)
        {
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City) && !string.IsNullOrWhiteSpace(loc.Street) && !string.IsNullOrWhiteSpace(loc.Number),
                t => t.DepartureLocation.City == loc.City && t.DepartureLocation.Street == loc.Street && t.DepartureLocation.Number == loc.Number
            );
            // Level 2: Street
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City) && !string.IsNullOrWhiteSpace(loc.Street),
                t => t.DepartureLocation.City == loc.City && t.DepartureLocation.Street == loc.Street
            );
            // Level 3: City
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City),
                t => t.DepartureLocation.City == loc.City
            );
            // Level 4: Province
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.Province),
                t => t.DepartureLocation.Province == loc.Province
            );
             // Level 5: Region
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.Region),
                t => t.DepartureLocation.Region == loc.Region
            );
        }
        else // Destination Logic
        {
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City) && !string.IsNullOrWhiteSpace(loc.Street) && !string.IsNullOrWhiteSpace(loc.Number),
                t => t.DestinationLocation.City == loc.City && t.DestinationLocation.Street == loc.Street && t.DestinationLocation.Number == loc.Number
            );
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City) && !string.IsNullOrWhiteSpace(loc.Street),
                t => t.DestinationLocation.City == loc.City && t.DestinationLocation.Street == loc.Street
            );
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.City),
                t => t.DestinationLocation.City == loc.City
            );
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.Province),
                t => t.DestinationLocation.Province == loc.Province
            );
            AddLevel(
                !string.IsNullOrWhiteSpace(loc.Region),
                t => t.DestinationLocation.Region == loc.Region
            );
        }

        // Execute Cascade
        foreach (var applyFilter in searchLevels)
        {
            var attemptQuery = applyFilter(query);

            // Check if results exist for this specificity level
            if (await attemptQuery.AnyAsync())
            {
                return attemptQuery; // Found match, return the filtered query
            }
        }

        // If no matches found at any level, force empty result
        return query.Where(t => false);
    }

    private class LoadUserTravelData
    {
        public DatabaseModels.UserTravel Travel { get; set; } = default!;
        public DatabaseModels.Location DepartureLocation { get; set; } = default!;
        public DatabaseModels.Location DestinationLocation { get; set; } = default!;
    }
}
