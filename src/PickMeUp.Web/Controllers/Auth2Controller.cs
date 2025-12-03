using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PickMeUp.Core.Services.Auth;
using PickMeUp.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PickMeUp.Web.Models.Auth;

namespace PickMeUp.Web.Controllers;

[Alerts]
[ModelStateToTempData]
public partial class Auth2Controller : Controller
{
    private readonly IAuthService _authService;

    public Auth2Controller(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public virtual IActionResult Login([FromQuery] string? returnUrl = null, [FromQuery] bool useCredentials = false)
    {
        var model = new LoginViewModel { ReturnUrl = returnUrl };
        if (useCredentials)
        {
            return View("LoginCredentials", model);
        }
        return View("Login", model);
    }

    [HttpPost]
    public async virtual Task<IActionResult> Login([FromForm] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            AlertHelper.AddError(this, "Dati non validi");
            return View("LoginCredentials", model);
        }

        var loginResult = await _authService.LoginAsync(new LoginParams
        {
            Email = model.Email,
            Password = model.Password,
        });

        if (loginResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, loginResult.ErrorMessage);
            return View("LoginCredentials", model);
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginResult.Data!.UserFirstName),
            new(ClaimTypes.NameIdentifier, loginResult.Data.UserId.ToString()),
            new(ClaimTypes.Email, loginResult.Data.UserEmail)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddMonths(3) : null,
                IsPersistent = model.RememberMe,
            });

        return string.IsNullOrWhiteSpace(model.ReturnUrl) 
            ? RedirectToAction("Landing", "Home")
            : Redirect(model.ReturnUrl);
    }

    [HttpGet]
    public virtual IActionResult SignUp([FromQuery] SignUpViewModel? signUpModel = null)
    {
        return View(signUpModel ?? new SignUpViewModel());
    }

    [HttpPost]
    [ActionName("SignUp")]
    public async virtual Task<IActionResult> SignUpPost([FromForm] SignUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            AlertHelper.AddError(this, "Dati non validi");
            return View("SignUp", model);
        }

        var signUpResult = await _authService.SignUpAsync(new SignUpParams
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
        });

        if (signUpResult.HasNonSuccessStatusCode)
        {
            AlertHelper.AddError(this, signUpResult.ErrorMessage);
            return View("SignUp", model);
        }

        return string.IsNullOrWhiteSpace(model.ReturnUrl) 
            ? RedirectToAction("Landing", "Home") 
            : Redirect(model.ReturnUrl);
    }

    [HttpPost]
    public async virtual Task<ControllerResult> ResendConfirmation([FromBody] ResendConfirmationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ControllerResult.Error("Dati non validi");
        }

        var result = await _authService.ResendConfirmationEmailAsync(model.Email);

        if (result.HasNonSuccessStatusCode)
        {
            return ControllerResult.Error(result.ErrorMessage);
        }

            return ControllerResult.Success();
    }

    [HttpPost]
    public async virtual Task<ControllerResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(request.Credential);

        var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var firstName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
        var lastName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
        var googleUserId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleUserId))
        {
            return ControllerResult.Error("Token Google non valido");
        }

        var result = await _authService.GoogleSignUpOrLoginAsync(new GoogleSignUpParams
        {
            GoogleIdToken = request.Credential,
            Email = email,
            FirstName = firstName ?? "",
            LastName = lastName ?? "",
            GoogleUserId = googleUserId
        });

        if (result.HasNonSuccessStatusCode)
        {
            return ControllerResult.Error(result.ErrorMessage);
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
            new(ClaimTypes.Email, result.Data.UserEmail)
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

        return ControllerResult.Success();
    }

    [HttpPost]
    public virtual IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        AlertHelper.AddSuccess(this, "Utente scollegato correttamente");
        return RedirectToAction("Landing", "Home");
    }

    [HttpGet]
    public virtual IActionResult CurrentUser()
    {
        var isAuth = User?.Identity?.IsAuthenticated == true;
        var firstName = User?.FindFirst(ClaimTypes.Name)?.Value;

        return Json(new {
            isAuthenticated = isAuth,
            firstName = firstName ?? string.Empty
        });
    }

    [HttpPost]
    public async virtual Task<ControllerResult> RequestPasswordReset([FromBody] RequestPasswordResetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ControllerResult.Error("Dati non validi");
        }

        var result = await _authService.RequestPasswordResetAsync(model.Email);

        if (!result.HasNonSuccessStatusCode)
        {
            return ControllerResult.Success();
        }

        return ControllerResult.Error(result.ErrorMessage);
    }

    [HttpGet]
    public virtual IActionResult ResetPassword(int userId, string token)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(token))
        {
            AlertHelper.AddError(this, "Link non valido o scaduto");
            return RedirectToAction("Landing", "Home");
        }

        var model = new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    public async virtual Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);

        if (result.HasNonSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        AlertHelper.AddSuccess(this, "Password reimpostata con successo! Ora puoi effettuare il login.");
        return RedirectToAction("Landing", "Home");
    }
}
