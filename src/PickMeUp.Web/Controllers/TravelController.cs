using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PickMeUp.Core.Services.GoogleRoutes;
using PickMeUp.Core.Services.UserTravel;
using PickMeUp.Web.Infrastructure;
using PickMeUp.Web.Models.Travel;
using System.Linq;
using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;
using PickMeUp.Core.Services.UserPickUpRequest;
using System;
using System.Collections.Generic;
using PickMeUp.Web.Extensions.Mappers;

namespace PickMeUp.Web.Controllers;

[Authorize]
[Alerts]
[ModelStateToTempData]
public partial class TravelController : Controller
{
    // Injected services
    private readonly IGoogleRoutesService _googleRoutesService;
    private readonly IUserTravelService _userTravelService;
    private readonly IUserPickUpRequestService _userPickUpRequestService;

    public TravelController(IGoogleRoutesService googleRoutesService, IUserTravelService userTravelService, IUserPickUpRequestService userPickUpRequestService)
    {
        _googleRoutesService = googleRoutesService;
        _userTravelService = userTravelService;
        _userPickUpRequestService = userPickUpRequestService;
    }
    
    [HttpGet]
    public virtual IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public virtual async Task<IActionResult> List([FromQuery] TravelListRequest request)
    {
        // Get Current User
        var currentUserId = this.GetUserId();

        // Call Service with Geographic Filters
        var listTravelsResult = await _userTravelService
            .ListUserTravelAsync(new()
            {
                UserId = currentUserId,
                IsFromFindTravel = request.IsFromFindTravel,
                ShowOnlyTravelsWithRole = request.ShowOnlyTravelsWithRole,
                ShowOnlyTravelsWithPendingPickUpRequests = request.ShowOnlyTravelsWithPendingPickUpRequests,
                DepartureLocation = request.Departure?.ToServiceModel(),
                DestinationLocation = request.Destination?.ToServiceModel(),
                DepartureDate = request.DepartureDate
            });

        // Check error
        if (listTravelsResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, listTravelsResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }

        return View(new TravelListViewModel
        {
            IsFromFindTravel = request.IsFromFindTravel,
            Filters = request,
            TotalTravelsWithPendingPickUpRequestsCount = listTravelsResult.Data!.TotalTravelsWithPendingPickUpRequestsCount,
            TotalTravelsAsDriver = listTravelsResult.Data!.TotalTravelsAsDriver,
            TotalTravelsAsGuest = listTravelsResult.Data!.TotalTravelsAsGuest,
            Travels = [.. listTravelsResult.Data!.Items
                .Select(travel => new TravelListItemViewModel
                {
                    UserTravelId = travel.UserTravelId,
                    UserId = travel.UserId,
                    UserNominative = travel.UserNominative,
                    TotalPassengersSeatsCount = travel.TotalPassengersSeatsCount,
                    OccupiedPassengerSeatsCount = travel.OccupiedPassengerSeatsCount,
                    DepartureAddress = travel.DepartureAddress,
                    DepartureDateTime = travel.DepartureDateTime,
                    DestinationAddress = travel.DestinationAddress,
                    AcceptedPickUpRequestUserIds = travel.AcceptedPickUpRequestUserIds,
                    PendingPickUpRequestUserIds = travel.PendingPickUpRequestUserIds,
                })],
        });
    }
     
    [HttpGet]
    public virtual async Task<IActionResult> Travel([FromQuery] int travelId)
    {
        var currentUserId = this.GetUserId();
        var getUserTravelResult = await _userTravelService
            .GetUserTravelAsync(
                new()
                {
                    UserId = currentUserId,
                    EntityId = travelId,
                });

        // Check error
        if (getUserTravelResult.HasNonSuccessStatusCode)
        {
            // TODO redirect to travels
            AlertHelper.AddError(this, getUserTravelResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }
        var userTravel = getUserTravelResult.Data!;

        // Prepare pick-up request for current user
        var currentUserPickUpRequestLookup = userTravel.TravelPickUpRequests
            .FirstOrDefault(pickUpRequest => pickUpRequest.UserId == currentUserId);
        PickUpRequestViewModel? currentUserPickUpRequest = null;
        if (currentUserPickUpRequestLookup != null)
        {
            var getPickUpRequestResult = await _userPickUpRequestService
                .GetUserPickUpRequest(
                    new()
                    {
                        UserId = currentUserId,
                        EntityId = currentUserPickUpRequestLookup.UserPickUpRequestId,
                    });

            // Check error
            if (getPickUpRequestResult.HasNonSuccessStatusCode)
            {
                AlertHelper.AddError(this, getPickUpRequestResult.ErrorMessage);
                return RedirectToAction(nameof(HomeController.Landing), "Home");
            }

            // Map to ViewModel
            currentUserPickUpRequest = new PickUpRequestViewModel
            {
                PickUpRequestId = getPickUpRequestResult.Data!.UserPickUpRequestId,
                TravelId = getPickUpRequestResult.Data.UserTravelId,
                Location = getPickUpRequestResult.Data.Location.ToViewModel(),
                Status = getPickUpRequestResult.Data.Status,
            };
        }
        

        // Get route using Google Routes
        var getGoogleRoutesResult = await _googleRoutesService
            .GetRouteAsync(
                new()
                {
                    Origin = new()
                    {
                        Latitude = userTravel.DepartureLocation.Coordinates.Latitude,
                        Longitude = userTravel.DepartureLocation.Coordinates.Longitude,
                    },
                    Destination = new()
                    {
                        Latitude = userTravel.DestinationLocation.Coordinates.Latitude,
                        Longitude = userTravel.DestinationLocation.Coordinates.Longitude,
                    },
                    Waypoints = [.. userTravel.TravelPickUpRequests
                        .Where(pickUpRequest => pickUpRequest.Status == UserPickUpRequestStatus.Accepted)
                        .Select(pickUpRequest => new Coordinates
                        {
                            Latitude = pickUpRequest.Location.Coordinates.Latitude,
                            Longitude = pickUpRequest.Location.Coordinates.Longitude,
                        })],
                });

        // Check error
        if (getGoogleRoutesResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, getGoogleRoutesResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }
        var googleRoute = getGoogleRoutesResult.Data!;

        return View(new TravelViewModel
        {
            UserId = userTravel.OwnerUserId,
            UserTravelId = userTravel.UserTravelId,
            UserNominative = userTravel.UserNominative,
            DepartureDate = DateOnly.FromDateTime(userTravel.DepartureDateTime), 
            DepartureTime = TimeOnly.FromDateTime(userTravel.DepartureDateTime),
            TotalPassengersSeatsCount = userTravel.TotalPassengersSeatsCount,
            OccupiedPassengerSeatsCount = userTravel.OccupiedPassengerSeatsCount,
            DepartureLocation = userTravel.DepartureLocation.ToViewModel(),
            DestinationLocation = userTravel.DestinationLocation.ToViewModel(),
            CurrentUserPickUpRequest = currentUserPickUpRequest,
            PickUpRequests = [.. userTravel.TravelPickUpRequests
                .Select(pickUpRequest => new PickUpRequestLookupViewModel
                {
                    PickUpRequestId = pickUpRequest.UserPickUpRequestId,
                    UserNominative = pickUpRequest.UserNominative,
                    Status = pickUpRequest.Status,
                    Location = pickUpRequest.Location.ToViewModel(),
                })],
            Route = new TravelRouteInfoViewModel
            {
                EncodedPolyline = googleRoute.EncodedPolyline,
                DistanceMeters = googleRoute.DistanceMeters,
                // TODO remove timestamp from duration, make it long
                DurationSeconds = (long)googleRoute.Duration.TotalSeconds
            },
        });
    }

    [HttpPost]
    public virtual async Task<ControllerResult> Edit([FromBody] TravelViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ControllerResult.Error("Dati di viaggio non validi");
        }

        // Edit request
        var editUserTravelResult = await _userTravelService
            .EditUserTravelAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    Entity = new()
                    {
                        UserTravelId = model.UserTravelId,
                        OwnerUserId = model.UserId,
                        UserNominative = model.UserNominative!,
                        DepartureDateTime = model.DepartureDate.ToDateTime(model.DepartureTime),
                        TotalPassengersSeatsCount = model.TotalPassengersSeatsCount,
                        OccupiedPassengerSeatsCount = model.OccupiedPassengerSeatsCount,
                        DepartureLocation = model.DepartureLocation.ToServiceModel(),
                        DestinationLocation = model.DestinationLocation.ToServiceModel(),
                        TravelPickUpRequests = [.. model.PickUpRequests
                            .Select(pickUpRequest => new UserPickUpRequestLookup
                            {
                                UserPickUpRequestId = pickUpRequest.PickUpRequestId,
                                UserNominative = pickUpRequest.UserNominative,
                                Status = pickUpRequest.Status,
                                Location = pickUpRequest.Location.ToServiceModel(),
                            })],
                    }
                });

        // Check error
        if (editUserTravelResult.HasNonSuccessStatusCode)
        {
            return ControllerResult.Error(editUserTravelResult.ErrorMessage);
        }
                
        AlertHelper.AddSuccess(this, "Viaggio creato con successo");
        return ControllerResult.Success(new { TravelId = editUserTravelResult.Data!.EntityId });
    }

    [HttpPost]
    public virtual async Task<ControllerResult> EditPickUpRequest([FromBody] PickUpRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ControllerResult.Error("Dati non validi. Seleziona un indirizzo dai suggerimenti.");
        }

        var editPickUpRequestResult = await _userPickUpRequestService
            .EditUserPickUpRequestAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    Entity = new()
                    {
                        UserPickUpRequestId = model.PickUpRequestId,
                        UserTravelId = model.TravelId,
                        Location = model.Location.ToServiceModel(),
                        Status = model.Status,
                    }
                });
        
        // Check error
        if (editPickUpRequestResult.HasNonSuccessStatusCode)
        {
            return ControllerResult.Error(editPickUpRequestResult.ErrorMessage);
        }

        AlertHelper.AddSuccess(this, "Richiesta di pick-up inviata con successo!");
        return ControllerResult.Success(new { TravelId = model.TravelId });
    }
         
    [HttpPost]
    public virtual async Task<IActionResult> EditPickUpRequestStatus([FromForm] int travelId, [FromForm] Dictionary<int, UserPickUpRequestStatus> decisions)
    {
        // Validate that at least one request is selected
        if (decisions == null || decisions.Count == 0)
        {
            AlertHelper.AddWarning(this, "Seleziona almeno una richiesta");
            return RedirectToAction(nameof(Travel), new { travelId });
        }

        // Build list of status changes from dictionary
        var statusChanges = decisions.Select(d => new PickUpRequestStatusChange
        {
            UserPickUpRequestId = d.Key,
            Status = d.Value
        }).ToList();

        var editPickUpRequestResult = await _userPickUpRequestService
            .EditUserPickUpRequestStatusBulkAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    StatusChanges = statusChanges,
                });

        // Check error
        if (editPickUpRequestResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, editPickUpRequestResult.ErrorMessage);
        }
        else
        {
            var acceptedCount = decisions.Count(d => d.Value == UserPickUpRequestStatus.Accepted);
            var rejectedCount = decisions.Count(d => d.Value == UserPickUpRequestStatus.Rejected);
            var message = $"{acceptedCount} richieste accettate, {rejectedCount} rifiutate";
            AlertHelper.AddSuccess(this, message);
        }        
        
        return RedirectToAction(nameof(Travel), new { travelId });
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeletePickUpRequest([FromForm] int travelId, [FromForm] int pickUpRequestId)
    {
        var deletePickUpRequestResult = await _userPickUpRequestService
            .DeleteUserPickUpRequestAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    EntityId = pickUpRequestId,
                });
        
        // Check error
        if (deletePickUpRequestResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, deletePickUpRequestResult.ErrorMessage);
        }
        else
        {
            AlertHelper.AddSuccess(this, "Richiesta di pick-up eliminata con successo!");
        }

        return RedirectToAction(nameof(Travel), new { travelId });
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteTravel([FromForm] int travelId)
    {
        var deleteTravelResult = await _userTravelService
            .DeleteUserTravelAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    EntityId = travelId,
                });
        
        // Check error
        if (deleteTravelResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, deleteTravelResult.ErrorMessage);
            return RedirectToAction(nameof(Travel), new { travelId });
        }
        
        AlertHelper.AddSuccess(this, "Viaggio annullato con successo!");
        return RedirectToAction(nameof(List), new { IsFromFindTravel = false });
    }
  
    /// <summary>
    /// Get user identifier from claims.
    /// </summary>
    private int GetUserId()
        => int.Parse(this.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
