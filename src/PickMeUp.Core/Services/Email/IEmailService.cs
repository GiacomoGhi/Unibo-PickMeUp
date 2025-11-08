using System;
using System.Threading.Tasks;
using PickMeUp.Core.Common.Models;
using PickMeUp.Enums.UserPickUpRequest;

namespace PickMeUp.Core.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// Sends an email confirmation link to the user.
    /// </summary>
    Task<Result> SendEmailConfirmationAsync(string email, string confirmationToken, string confirmationUrl);

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    Task<Result> SendPasswordResetAsync(string email, string resetUrl);

    /// <summary>
    /// Sends a welcome email to a new user.
    /// </summary>
    Task<Result> SendWelcomeEmailAsync(string email, string firstName);

    /// <summary>
    /// Sends an email to the travel owner when a new pickup request is received.
    /// </summary>
    Task<Result> SendPickUpRequestReceivedAsync(
        string travelOwnerEmail,
        string travelOwnerFirstName,
        string requesterFirstName,
        string requesterLastName,
        string departureAddress,
        string destinationAddress,
        string pickUpPointAddress);

    /// <summary>
    /// Sends an email to the requester when their pickup request status changes (accepted or rejected).
    /// </summary>
    Task<Result> SendPickUpRequestStatusChangedAsync(
        string requesterEmail,
        string requesterFirstName,
        string travelOwnerFirstName,
        UserPickUpRequestStatus status,
        string departureAddress,
        string destinationAddress,
        DateTime departureDateTime);

    /// <summary>
    /// Sends an email to the travel owner when an accepted pickup request is cancelled.
    /// </summary>
    Task<Result> SendPickUpRequestCancelledAsync(
        string travelOwnerEmail,
        string travelOwnerFirstName,
        string requesterFirstName,
        string requesterLastName,
        string departureAddress,
        string destinationAddress,
        DateTime departureDateTime);
}
