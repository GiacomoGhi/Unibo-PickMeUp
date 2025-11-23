namespace PickMeUp.Core.Services.Auth;

public class LoginResult
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User first name.
    /// </summary>
    public string UserFirstName { get; set; } = default!;

    /// <summary>
    /// User email address.
    /// </summary>
    public string UserEmail { get; set; } = default!;
}