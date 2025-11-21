using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PickMeUp.Core.Services.GoogleRoutes;
using PickMeUp.Web.Infrastructure;
using PickMeUp.Web.Models.Travel;

namespace PickMeUp.Web.Controllers;

[Alerts]
[ModelStateToTempData]
public partial class TravelController : Controller
{
    // Injected services
    private readonly IGoogleRoutesService _googleRoutesService;

    public TravelController(IGoogleRoutesService googleRoutesService)
    {
        _googleRoutesService = googleRoutesService;
    }

    [HttpGet]
    public virtual async Task<IActionResult> Search(TravelViewModel model)
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
}
