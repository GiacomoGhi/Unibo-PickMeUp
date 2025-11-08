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

    private readonly PickMeUpDbContext _dbContext = dbContext;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc/>
    public async Task<Result<LoginResult>> LoginAsync(LoginParams parameters)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            return Result.InvalidArgument(nameof(parameters.Email));
        }
        if (string.IsNullOrWhiteSpace(parameters.Password))
        {
            return Result.InvalidArgument(nameof(parameters.Password));
        }

        // Normalize email
        var normalizedEmail = parameters.Email.Trim().ToUpperInvariant();

        // Find user by email
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(user => !user.DeletionDateTime.HasValue
                        && user.IsEmailConfirmed
                        && user.NormalizedEmail == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return Result.Error("Invalid email or password");
        }

        // Verify user
        if (user is null || user.PasswordHash != CryptographyHelper.Hash(parameters.Password))
        {
            return Result.Error("Invalid email or password");
        }

        // Return login result
        return Result.Success(
            new LoginResult
            {
                UserId = user.UserId,
                UserEmail = user.Email
            });
    }

    /// <inheritdoc/>
    public async Task<Result> SignUpAsync(SignUpParams parameters)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            return Result.InvalidArgument(nameof(parameters.Email));
        }
        if (string.IsNullOrWhiteSpace(parameters.Password))
        {
            return Result.InvalidArgument(nameof(parameters.Password));
        }
        if (string.IsNullOrWhiteSpace(parameters.FirstName))
        {
            return Result.InvalidArgument(nameof(parameters.FirstName));
        }
        if (string.IsNullOrWhiteSpace(parameters.LastName))
        {
            return Result.InvalidArgument(nameof(parameters.LastName));
        }

        // TODO validate length and complexity
        // Validate password strength
        if (parameters.Password.Length < 8)
        {
            return Result.InvalidArgument("Password must be at least 8 characters long");
        }

        // Normalize email
        var normalizedEmail = parameters.Email.Trim().ToUpperInvariant();

        // Check if user already exists
        if (await _dbContext.Users
                .Where(user => user.NormalizedEmail == normalizedEmail 
                            && user.DeletionDateTime == null)
                .AnyAsync())
        {
            return Result.Error("Invalid email");
        }

        // Create new user
        var newUser = new DatabaseModels.User
        {
            Email = normalizedEmail,
            PasswordHash = CryptographyHelper.Hash(parameters.Password),
            FirstName = parameters.FirstName.Trim(),
            LastName = parameters.LastName.Trim(),
            IsEmailConfirmed = false,
            EmailConfirmationToken = GenerateConfirmationToken(),
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(CONFERMATION_EMAIL_TOKEN_EXPIRY_HOURS),
            DeletionDateTime = null 
        };
        _dbContext.Users.Add(newUser);

        // Save changes
        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var confirmationUrl = $"{baseUrl}/Login/ConfirmEmail?userId={newUser.UserId}&token={Uri.EscapeDataString(newUser.EmailConfirmationToken)}";

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
                        && user.NormalizedEmail == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return Result.NotFound("User");
        }

        if (user.IsEmailConfirmed)
        {
            return Result.Error("Email already confirmed");
        }

        // Generate new token

        user.EmailConfirmationToken = GenerateConfirmationToken();
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(CONFERMATION_EMAIL_TOKEN_EXPIRY_HOURS);

        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var confirmationUrl = $"{baseUrl}/Login/ConfirmEmail?userId={user.UserId}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";

        return await _emailService
            .SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken, confirmationUrl);
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResult>> GoogleSignUpOrLoginAsync(GoogleSignUpParams parameters)
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(parameters.Email))
        {
            return Result.InvalidArgument(nameof(parameters.Email));
        }
        if (string.IsNullOrWhiteSpace(parameters.GoogleUserId))
        {
            return Result.InvalidArgument(nameof(parameters.GoogleUserId));
        }
        if (string.IsNullOrWhiteSpace(parameters.FirstName))
        {
            return Result.InvalidArgument(nameof(parameters.FirstName));
        }
        if (string.IsNullOrWhiteSpace(parameters.LastName))
        {
            return Result.InvalidArgument(nameof(parameters.LastName));
        }

        // Normalize email
        var normalizedEmail = parameters.Email.Trim().ToUpperInvariant();

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
            Email = normalizedEmail,
            // For Google users, we use a random hash since they don't have a password
            PasswordHash = CryptographyHelper.Hash(Guid.NewGuid().ToString()),
            FirstName = parameters.FirstName.Trim(),
            LastName = parameters.LastName.Trim(),
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
}