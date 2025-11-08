namespace PickMeUp.Core.Services.Auth;

public class LoginParams
{
    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = default!;


    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = default!;
}