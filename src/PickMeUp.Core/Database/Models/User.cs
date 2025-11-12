using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace PickMeUp.Core.Database.Models;

internal class User
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User email.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Normalized user email.
    /// </summary>
    public string NormalizedEmail { get; set; } = default!;

    /// <summary>
    /// Password hash.
    /// </summary>
    public string PasswordHash { get; set; } = default!;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Deletion date and time of the user.
    /// </summary>
    public DateTime? DeletionDateTime { get; set; }

    /// <summary>
    /// Indicates whether the user's email is confirmed.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Email confirmation token (null if email is confirmed).
    /// </summary>
    public string? EmailConfirmationToken { get; set; }

    /// <summary>
    /// Expiration date and time for the email confirmation token.
    /// </summary>
    public DateTime? EmailConfirmationTokenExpiry { get; set; }

    /// <summary>
    /// Password reset token (null if no reset is pending).
    /// </summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// Expiration date and time for the password reset token.
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }
}

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary key
        builder.HasKey(e => e.UserId);

        // Constraints
        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(100);

        builder.Property(e => e.FirstName)
            .HasMaxLength(50);  

        builder.Property(e => e.LastName)
            .HasMaxLength(50);
    }
}