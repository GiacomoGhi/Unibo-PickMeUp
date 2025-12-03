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
                // Logic for Include/Exclude based on the flag
                UserIdsToInclude = request.IsFromFindTravel ? [] : [currentUserId],
                UserIdsToExclude = request.IsFromFindTravel ? [currentUserId] : [],
                
                // Pass the mapped locations
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
            Travels = [.. listTravelsResult.Data!.Items
                .Select(travel => new TravelListItemViewModel
                {
                    UserTravelId = travel.UserTravelId,
                    UserNominative = travel.UserNominative,
                    TotalPassengersSeatsCount = travel.TotalPassengersSeatsCount,
                    OccupiedPassengerSeatsCount = travel.OccupiedPassengerSeatsCount,
                    DepartureAddress = travel.DepartureAddress,
                    DepartureDateTime = travel.DepartureDateTime,
                    DestinationAddress = travel.DestinationAddress,
                })],
        });
    }
     
    [HttpGet]
    public virtual async Task<IActionResult> Travel([FromQuery] int travelId)
    {
        var getUserTravelResult = await _userTravelService
            .GetUserTravelAsync(
                new()
                {
                    UserId = this.GetUserId(),
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
            DepartureLocation = userTravel.DepartureLocation.ToViewModel(),
            DestinationLocation = userTravel.DestinationLocation.ToViewModel(),
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
    public virtual async Task<IActionResult> EditPickUpRequestStatus([FromForm] int travelId, [FromForm] int[] pickUpRequestIds, [FromForm] UserPickUpRequestStatus status)
    {
        // Validate that at least one request is selected
        if (pickUpRequestIds == null || pickUpRequestIds.Length == 0)
        {
            AlertHelper.AddWarning(this, "Seleziona almeno una richiesta");
            return RedirectToAction(nameof(Travel), new { travelId });
        }

        var editPickUpRequestResult = await _userPickUpRequestService
            .EditUserPickUpRequestStatusBulkAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    UserPickUpRequestIds = [.. pickUpRequestIds],
                    Status = status,
                });

        // Check error
        if (editPickUpRequestResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, editPickUpRequestResult.ErrorMessage);
        }
        else
        {
            var action = status == UserPickUpRequestStatus.Accepted ? "accettate" : "rifiutate";
            AlertHelper.AddSuccess(this, $"{pickUpRequestIds.Length} richieste {action} con successo");
        }        
        
        return RedirectToAction(nameof(Travel), new { travelId });
    }
  
    /// <summary>
    /// Get user identifier from claims.
    /// </summary>
    private int GetUserId()
        => int.Parse(this.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
