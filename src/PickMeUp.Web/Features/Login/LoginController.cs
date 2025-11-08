using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using PickMeUp.Core.Services.Auth;
using PickMeUp.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PickMeUp.Web.Features.Login;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Alerts]
[ModelStateToTempData]
public partial class LoginController(
    IAuthService authService,
    IStringLocalizer<SharedResource> sharedLocalizer,
    IConfiguration configuration) : Controller
{
    public static string LOGIN_ERROR_MODEL_STATE_KEY = "LoginError";

    // Injected services
    private readonly IAuthService _authService = authService;
    private readonly IStringLocalizer<SharedResource> _sharedLocalizer = sharedLocalizer;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    public virtual IActionResult Login(string returnUrl)
    {
        if (HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) == false)
                return Redirect(returnUrl);

            return RedirectToAction(MVC.Example.Users.Index());
        }

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl,
        };

        ViewBag.GoogleClientId = _configuration["Authentication:Google:ClientId"];
        return View(model);
    }

    [HttpPost]
    public async virtual Task<ActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var loginResult = await _authService
                .LoginAsync(
                    new LoginParams
                    {
                        Email = model.Email,
                        Password = model.Password,
                    });

            if (!loginResult.HasNonSuccessStatusCode)
            {
                return LoginAndRedirect(loginResult.Data!, model.ReturnUrl, model.RememberMe);
            }

            ModelState.AddModelError(LOGIN_ERROR_MODEL_STATE_KEY, loginResult.ErrorMessage);
        }

        ViewBag.GoogleClientId = _configuration["Authentication:Google:ClientId"];
        return View(model);
    }

    [HttpGet]
    public virtual IActionResult SignUp(string returnUrl)
    {
        if (HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) == false)
                return Redirect(returnUrl);

            return RedirectToAction(MVC.Example.Users.Index());
        }

        var model = new SignUpViewModel
        {
            ReturnUrl = returnUrl,
        };

        ViewBag.GoogleClientId = _configuration["Authentication:Google:ClientId"];
        return View(model);
    }

    [HttpPost]
    public async virtual Task<ActionResult> SignUp(SignUpViewModel model)
    {
        if (ModelState.IsValid)
        {
            var signUpResult = await _authService
                .SignUpAsync(
                    new SignUpParams
                    {
                        Email = model.Email,
                        Password = model.Password,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                    });

            if (!signUpResult.HasNonSuccessStatusCode)
            {
                Alerts.AddSuccess(this, "Registrazione completata! Controlla la tua email per confermare il tuo account.");
                return RedirectToAction(MVC.Login.Login());
            }

            // If error message is about email confirmation, show it as info
            if (signUpResult.ErrorMessage.Contains("Controlla la tua email"))
            {
                Alerts.AddInfo(this, signUpResult.ErrorMessage);
                return RedirectToAction(MVC.Login.Login());
            }

            ModelState.AddModelError(string.Empty, signUpResult.ErrorMessage);
        }

        ViewBag.GoogleClientId = _configuration["Authentication:Google:ClientId"];
        return View(model);
    }

    [HttpGet]
    public async virtual Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(token))
        {
            Alerts.AddError(this, "Link di conferma non valido");
            return RedirectToAction(MVC.Login.Login());
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);

        if (!result.HasNonSuccessStatusCode)
        {
            Alerts.AddSuccess(this, "Email confermata con successo! Ora puoi effettuare l'accesso.");
            return RedirectToAction(MVC.Login.Login());
        }

        Alerts.AddError(this, result.ErrorMessage);
        return RedirectToAction(MVC.Login.Login());
    }

    [HttpGet]
    public virtual IActionResult ResendConfirmation()
    {
        return View(new ResendConfirmationViewModel());
    }

    [HttpPost]
    public async virtual Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.ResendConfirmationEmailAsync(model.Email);

            if (!result.HasNonSuccessStatusCode)
            {
                Alerts.AddSuccess(this, "Email di conferma inviata! Controlla la tua casella di posta.");
                return RedirectToAction(MVC.Login.Login());
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage);
        }

        return View(model);
    }

    [HttpPost]
    public async virtual Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            // Decode the Google JWT token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(request.Credential);

            // Extract user information from the token
            var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var firstName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var lastName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            var googleUserId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleUserId))
            {
                return Json(new { success = false, message = "Token Google non valido" });
            }

            // Authenticate or create user
            var result = await _authService.GoogleSignUpOrLoginAsync(new GoogleSignUpParams
            {
                GoogleIdToken = request.Credential,
                Email = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                GoogleUserId = googleUserId
            });

            if (!result.HasNonSuccessStatusCode)
            {
                // Sign in the user
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
                    new System.Security.Claims.Claim(ClaimTypes.Email, result.Data.UserEmail)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMonths(3)
                    });

                var redirectUrl = string.IsNullOrWhiteSpace(request.ReturnUrl)
                    ? Url.Action("Index", "Users", new { area = "Example" })
                    : request.ReturnUrl;

                return Json(new { success = true, redirectUrl });
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Errore durante l'autenticazione con Google: " + ex.Message });
        }
    }

    [HttpPost]
    public async virtual Task<IActionResult> GoogleSignUp([FromBody] GoogleLoginRequest request)
    {
        // For Google, login and signup are the same process
        return await GoogleLogin(request);
    }

    [HttpPost]
    public virtual IActionResult Logout()
    {
        HttpContext.SignOutAsync();

        Alerts.AddSuccess(this, "Utente scollegato correttamente");
        return RedirectToAction(MVC.Login.Login());
    }

    /// <summary>
    /// Logs in the user and redirects to the specified return URL.
    /// </summary>
    private ActionResult LoginAndRedirect(LoginResult loginResponse, string returnUrl, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
            new(ClaimTypes.Email, loginResponse.UserEmail)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
        {
            ExpiresUtc = (rememberMe) ? DateTimeOffset.UtcNow.AddMonths(3) : null,
            IsPersistent = rememberMe,
        });

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(MVC.Example.Users.Index());
    }
}

public class GoogleLoginRequest
{
    public string Credential { get; set; } = default!;
    public string? ReturnUrl { get; set; }
}