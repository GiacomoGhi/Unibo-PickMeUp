namespace PickMeUp.Core.Services.Auth;

public class GoogleSignUpParams
{
    /// <summary>
    /// Google ID token received from Google OAuth.
    /// </summary>
    public string GoogleIdToken { get; set; } = default!;

    /// <summary>
    /// Email address from Google account.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// First name from Google account.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name from Google account.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Google user ID.
    /// </summary>
    public string GoogleUserId { get; set; } = default!;
}
