using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PickMeUp.Core.Common.Helpers;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.Email;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.Auth;

internal class AuthService(
    PickMeUpDbContext dbContext, 
    IEmailService emailService, 
    IConfiguration configuration) : IAuthService
{
    private const int CONFERMATION_EMAIL_TOKEN_EXPIRY_HOURS = 24;
    private const int PASSWORD_RESET_TOKEN_EXPIRY_HOURS = 1;

    private readonly PickMeUpDbContext _dbContext = dbContext;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc/>
    public async Task<Result<LoginResult>> LoginAsync(LoginParams requestParams)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(requestParams.Email))
        {
            return Result.InvalidArgument(nameof(requestParams.Email));
        }
        if (string.IsNullOrWhiteSpace(requestParams.Password))
        {
            return Result.InvalidArgument(nameof(requestParams.Password));
        }

        // Normalize email
        var normalizedEmail = requestParams.Email.Trim().ToUpperInvariant();

        // Find user by email
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(user => !user.DeletionDateTime.HasValue
                        && user.IsEmailConfirmed
                        && user.NormalizedEmail == normalizedEmail)
            .FirstOrDefaultAsync();

        // Verify user
        if (user is null || user.PasswordHash != CryptographyHelper.Hash(requestParams.Password))
        {
            return Result.Error("Invalid email or password");
        }

        // Return login result
        return Result.Success(
            new LoginResult
            {
                UserId = user.UserId,
                UserFirstName = user.FirstName,
                UserEmail = user.Email
            });
    }

    /// <inheritdoc/>
    public async Task<Result> SignUpAsync(SignUpParams requestPrams)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(requestPrams.Email))
        {
            return Result.InvalidArgument(nameof(requestPrams.Email));
        }
        if (string.IsNullOrWhiteSpace(requestPrams.Password))
        {
            return Result.InvalidArgument(nameof(requestPrams.Password));
        }
        if (string.IsNullOrWhiteSpace(requestPrams.FirstName))
        {
            return Result.InvalidArgument(nameof(requestPrams.FirstName));
        }
        if (string.IsNullOrWhiteSpace(requestPrams.LastName))
        {
            return Result.InvalidArgument(nameof(requestPrams.LastName));
        }

        // TODO validate length and complexity
        // Validate password strength
        if (requestPrams.Password.Length < 8)
        {
            return Result.InvalidArgument("Password must be at least 8 characters long");
        }

        // Normalize email
        var normalizedEmail = requestPrams.Email.Trim().ToUpperInvariant();

        // Check if user already exists
        var existingUser = await _dbContext.Users
            .Where(user => user.NormalizedEmail == normalizedEmail 
                        && user.DeletionDateTime == null)
            .FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            // Don't reveal if user exists or not for security reasons
            // Return success anyway to prevent email enumeration
            
            // TODO Optionally: send a security notification to the existing user
            // Ignoring the result as it's not critical
            // _ = await _emailService.SendAccountExistsNotificationAsync(existingUser.Email, existingUser.FirstName);
            
            return Result.Success();
        }

        // Create new user
        var newUser = new DatabaseModels.User
        {
            Email = requestPrams.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = CryptographyHelper.Hash(requestPrams.Password),
            FirstName = requestPrams.FirstName.Trim(),
            LastName = requestPrams.LastName.Trim(),
            IsEmailConfirmed = false,
            EmailConfirmationToken = GenerateConfirmationToken(),
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(CONFERMATION_EMAIL_TOKEN_EXPIRY_HOURS),
            DeletionDateTime = null 
        };
        _dbContext.Users.Add(newUser);

        // Save changes
        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5178";
        var confirmationUrl = $"{baseUrl}/Home/ConfirmEmail?userId={newUser.UserId}&token={Uri.EscapeDataString(newUser.EmailConfirmationToken)}";

        return await _emailService.SendEmailConfirmationAsync(newUser.Email, newUser.EmailConfirmationToken, confirmationUrl);
    }

    /// <summary>
    /// Confirms a user's email address using the confirmation token.
    /// </summary>
    public async Task<Result> ConfirmEmailAsync(int userId, string token)
    {
        var user = await _dbContext.Users
            .Where(user => user.UserId == userId && user.DeletionDateTime == null)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return Result.NotFound("User");
        }

        if (user.IsEmailConfirmed)
        {
            return Result.Error("Email already confirmed");
        }

        if (user.EmailConfirmationToken != token)
        {
            return Result.Error("Invalid confirmation token");
        }

        if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
        {
            return Result.Error("Confirmation token has expired");
        }

        // Confirm email
        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;

        await _dbContext.SaveChangesAsync();

        // Ignoring send welcome email failure.
        // Its not critical to the flow
        _ = await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

        return Result.Success();
    }

    /// <summary>
    /// Resends the email confirmation link to the user.
    /// </summary>
    public async Task<Result> ResendConfirmationEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();

        var user = await _dbContext.Users
            .Where(user => !user.DeletionDateTime.HasValue
                        && !user.IsEmailConfirmed
                        && user.NormalizedEmail == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            // Don't reveal if user exists or not for security reasons
            // Return success anyway to prevent email enumeration
            return Result.Success();
        }

        // Generate new token
        user.EmailConfirmationToken = GenerateConfirmationToken();
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(CONFERMATION_EMAIL_TOKEN_EXPIRY_HOURS);

        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var confirmationUrl = $"{baseUrl}/Home/ConfirmEmail?userId={user.UserId}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";

        return await _emailService
            .SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken, confirmationUrl);
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResult>> GoogleSignUpOrLoginAsync(GoogleSignUpParams requestParams)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(requestParams.Email))
        {
            return Result.InvalidArgument(nameof(requestParams.Email));
        }
        if (string.IsNullOrWhiteSpace(requestParams.GoogleUserId))
        {
            return Result.InvalidArgument(nameof(requestParams.GoogleUserId));
        }
        if (string.IsNullOrWhiteSpace(requestParams.FirstName))
        {
            return Result.InvalidArgument(nameof(requestParams.FirstName));
        }
        if (string.IsNullOrWhiteSpace(requestParams.LastName))
        {
            return Result.InvalidArgument(nameof(requestParams.LastName));
        }

        // Normalize email
        var normalizedEmail = requestParams.Email.Trim().ToUpperInvariant();

        // Check if user already exists
        var existingUser = await _dbContext.Users
            .Where(user => !user.DeletionDateTime.HasValue
                        && user.NormalizedEmail == normalizedEmail)
            .FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            // User exists, log them in
            // Email is already confirmed via Google
            if (!existingUser.IsEmailConfirmed)
            {
                existingUser.IsEmailConfirmed = true;
                await _dbContext.SaveChangesAsync();
            }

            return Result.Success(new LoginResult
            {
                UserId = existingUser.UserId,
                UserEmail = existingUser.Email
            });
        }

        // Create new user with Google account
        var newUser = new DatabaseModels.User
        {
            Email = requestParams.Email,
            NormalizedEmail = normalizedEmail,
            // For Google users, we use a random hash since they don't have a password
            PasswordHash = CryptographyHelper.Hash(Guid.NewGuid().ToString()),
            FirstName = requestParams.FirstName.Trim(),
            LastName = requestParams.LastName.Trim(),
            IsEmailConfirmed = true, // Google accounts are pre-verified
            DeletionDateTime = null
        };
        _dbContext.Users.Add(newUser);

        // Save changes
        await _dbContext.SaveChangesAsync();

        // Return login result
        return Result.Success(new LoginResult
        {
            UserId = newUser.UserId,
            UserFirstName = newUser.FirstName,
            UserEmail = newUser.Email
        });
    }

    /// <summary>
    /// Generates a secure random confirmation token.
    /// </summary>
    private static string GenerateConfirmationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + 
               Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    /// <inheritdoc/>
    public async Task<Result> RequestPasswordResetAsync(string email)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();

        var user = await _dbContext.Users
            .Where(user => !user.DeletionDateTime.HasValue
                        && user.NormalizedEmail == normalizedEmail
                        && user.IsEmailConfirmed)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            // Don't reveal if user exists or not for security reasons
            // Return success anyway to prevent email enumeration
            return Result.Success();
        }

        // Generate reset token
        user.PasswordResetToken = GenerateConfirmationToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(PASSWORD_RESET_TOKEN_EXPIRY_HOURS);

        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5178";
        var resetUrl = $"{baseUrl}/Auth/ResetPassword?userId={user.UserId}&token={Uri.EscapeDataString(user.PasswordResetToken)}";

        return await _emailService.SendPasswordResetAsync(user.Email, resetUrl);
    }

    /// <inheritdoc/>
    public async Task<Result> ResetPasswordAsync(int userId, string token, string newPassword)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.InvalidArgument(nameof(token));
        }
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.InvalidArgument(nameof(newPassword));
        }

        // Validate password strength
        if (newPassword.Length < 8)
        {
            return Result.InvalidArgument("Password must be at least 8 characters long");
        }

        var user = await _dbContext.Users
            .Where(user => user.UserId == userId && user.DeletionDateTime == null)
            .FirstOrDefaultAsync();

        if (user is null
            || string.IsNullOrWhiteSpace(user.PasswordResetToken) 
            || user.PasswordResetToken != token
            || !user.PasswordResetTokenExpiry.HasValue 
            || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return Result.Error("Invalid or expired reset token");
        }

        // Update password and clear reset token
        user.PasswordHash = CryptographyHelper.Hash(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}