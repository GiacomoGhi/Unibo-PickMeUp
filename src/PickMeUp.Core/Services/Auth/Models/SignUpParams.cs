namespace PickMeUp.Core.Services.Auth;

public class SignUpParams
{
    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; } = default!;
}