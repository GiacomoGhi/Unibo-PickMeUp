using PickMeUp.Core.Common.Models;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.Auth;

public interface IAuthService
{
    /// <summary>
    /// Logs in a user with the provided parameters.
    /// </summary>
    Task<Result<LoginResult>> LoginAsync(LoginParams parameters);

    /// <summary>
    /// Signs up a new user with email and password.
    /// Sends a confirmation email that must be verified before login.
    /// </summary>
    Task<Result> SignUpAsync(SignUpParams parameters);

    /// <summary>
    /// Signs up or logs in a user with Google OAuth.
    /// </summary>
    Task<Result<LoginResult>> GoogleSignUpOrLoginAsync(GoogleSignUpParams parameters);

    /// <summary>
    /// Confirms a user's email address using the confirmation token.
    /// </summary>
    Task<Result> ConfirmEmailAsync(int userId, string token);

    /// <summary>
    /// Resends the email confirmation link to the user.
    /// </summary>
    Task<Result> ResendConfirmationEmailAsync(string email);
}
