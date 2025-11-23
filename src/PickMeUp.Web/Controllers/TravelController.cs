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
using System;
using PickMeUp.Core.Services.UserPickUpRequest;
using System.ComponentModel.Design;

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
    public virtual async Task<IActionResult> Search(UserTravelViewModel model)
    {
        if (!ModelState.IsValid)
        {
            AlertHelper.AddError(this, "Dati di ricerca non validi");
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }

        var getGoogleRoutesResult = await _googleRoutesService.GetRouteAsync(
            new()
            {
                Origin = new()
                {
                    Latitude = model.DepartureLat,
                    Longitude = model.DepartureLng,
                },
                Destination = new()
                {
                    Latitude = model.DestinationLat,
                    Longitude = model.DestinationLng,
                },
            });

        // Check error
        if (getGoogleRoutesResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, getGoogleRoutesResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }
        var googleRoute = getGoogleRoutesResult.Data!;

        // Map to view model
        model.Route = new TravelRouteInfoViewModel
            {
                EncodedPolyline = googleRoute.EncodedPolyline,
                DistanceMeters = googleRoute.DistanceMeters,
                // TODO remove timestamp from duration, make it long
                DurationSeconds = (long)googleRoute.Duration.TotalSeconds
            };

        // Per ora passiamo semplicemente i dati alla view
        return View(model);
    }

    [HttpPost]
    public virtual async Task<ControllerResult> Edit([FromBody] UserTravelViewModel model)
    {
        if (!ModelState.IsValid
            || int.TryParse(User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId) == false)
        {
            return ControllerResult.Error("Dati di viaggio non validi");
        }

        // Edit request
        var editUserTravelResult = await _userTravelService
            .EditUserTravelAsync(
                new()
                {
                    UserId = userId,
                    Entity = new()
                    {
                        DepartureLatitude = model.DepartureLat,
                        DepartureLongitude = model.DepartureLng,
                        DepartureAddress = model.DepartureAddress,
                        DepartureDateTime = new System.DateTime(
                            model.DepartureDate.Year, model.DepartureDate.Month, model.DepartureDate.Day,
                            model.DepartureTime.Hour, model.DepartureTime.Minute, model.DepartureTime.Second),
                        DestinationLatitude = model.DestinationLat,
                        DestinationLongitude = model.DestinationLng,
                        DestinationAddress = model.DestinationAddress,
                        TotalPassengersSeatsCount = 4, // TODO set real seats count
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
        var getGoogleRoutesResult = await _googleRoutesService.GetRouteAsync(
            new()
            {
                Origin = new()
                {
                    Latitude = userTravel.DepartureLatitude,
                    Longitude = userTravel.DepartureLongitude,
                },
                Destination = new()
                {
                    Latitude = userTravel.DestinationLatitude,
                    Longitude = userTravel.DestinationLongitude,
                },
                Waypoints = [.. userTravel.TravelPickUpRequests
                    .Where(pickUpRequest => pickUpRequest.Status == UserPickUpRequestStatus.Accepted)
                    .Select(pickUpRequest => new Coordinates
                    {
                        Latitude = pickUpRequest.PickUpPointLatitude,
                        Longitude = pickUpRequest.PickUpPointLongitude,
                    })],
            });

        // Check error
        if (getGoogleRoutesResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, getGoogleRoutesResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }
        var googleRoute = getGoogleRoutesResult.Data!;

        return View(new UserTravelViewModel
        {
            UserId = userTravel.OwnerUserId,
            DepartureLat = userTravel.DepartureLatitude,
            DepartureLng = userTravel.DepartureLongitude,
            DepartureAddress = userTravel.DepartureAddress,
            DestinationLat = userTravel.DestinationLatitude,
            DestinationLng = userTravel.DestinationLongitude,
            DestinationAddress = userTravel.DestinationAddress,
            PickUpRequests = [.. userTravel.TravelPickUpRequests
                .Select(pickUpRequest => new PickUpRequestViewModel
                {
                    PickUpRequestId = pickUpRequest.UserPickUpRequestId,
                    UserNominative = pickUpRequest.UserNominative,
                    PickUpPointAddress = pickUpRequest.PickUpPointAddress,
                    Status = pickUpRequest.Status,
                    Location = new Coordinates
                    {
                        Latitude = pickUpRequest.PickUpPointLatitude,
                        Longitude = pickUpRequest.PickUpPointLongitude,
                    }
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
    public virtual async Task<IActionResult> EditPickUpRequestStatus([FromForm] int travelId, [FromForm] int pickUpRequestId, [FromForm] UserPickUpRequestStatus status)
    {
        var editPickUpRequestResult = await _userPickUpRequestService.EditUserPickUpRequestAsync(
            new()
            {
                UserId = this.GetUserId(),
                Entity = new()
                {
                    UserPickUpRequestId = pickUpRequestId,
                    Status = status,
                }
            });

        // Check error
        if (editPickUpRequestResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, editPickUpRequestResult.ErrorMessage);
        }        
        
        return RedirectToAction(nameof(Travel), new { travelId });
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditPickUpRequest([FromBody] PickUpRequestViewModel model)
    {
        var editPickUpRequestResult = await _userPickUpRequestService
            .EditUserPickUpRequestAsync(
                new()
                {
                    UserId = this.GetUserId(),
                    Entity = new()
                    {
                        UserTravelId = model.TravelId,
                        PickUpPointLatitude = model.Location.Latitude,
                        PickUpPointLongitude = model.Location.Longitude,
                        PickUpPointAddress = model.PickUpPointAddress,
                    }
                });
        
        // Check error
        if (editPickUpRequestResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, editPickUpRequestResult.ErrorMessage);
        }

        return RedirectToAction(nameof(Travel), new { travelId = model.TravelId });
    }
    
    [HttpGet]
    public virtual async Task<IActionResult> TravelList()
    {
        var listTravelsResult = await _userTravelService
            .ListUserTravelAsync(new());

        // Check error
        if (listTravelsResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, listTravelsResult.ErrorMessage);
            return RedirectToAction(nameof(HomeController.Landing), "Home");
        }

        return View(new TravelListViewModel
        {
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
                
    /// <summary>
    /// Get user identifier from claims.
    /// </summary>
    private int GetUserId()
        => int.Parse(this.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
