using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PickMeUp.Core.Services.Auth;
using PickMeUp.Web.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PickMeUp.Web.Controllers;

[Alerts]
[ModelStateToTempData]
public partial class HomeController : Controller
{
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public virtual IActionResult Landing()
    {
        // Landing page - accessible for both authenticated and unauthenticated users
        return View();
    }

    [HttpPost]
    public virtual IActionResult ChangeLanguageTo(string cultureName)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureName)),
            // Secure ensures the cookie is only sent over HTTPS
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
        );

        var referer = Request.GetTypedHeaders().Referer?.ToString();
        return Redirect(referer ?? "/");
    }

    [HttpGet]
    public async virtual Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(token))
        {
            AlertHelper.AddError(this, "Link di conferma non valido");
            return RedirectToAction(nameof(Landing));
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);

        if (!result.HasNonSuccessStatusCode)
        {
            AlertHelper.AddSuccess(this, "Email confermata con successo! Ora puoi effettuare l'accesso.");
            return RedirectToAction(nameof(Landing));
        }

        AlertHelper.AddError(this, result.ErrorMessage);
        return RedirectToAction(nameof(Landing));
    }
}
