using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PickMeUp.Core.Common.Helpers;
using PickMeUp.Core.Common.Models;
using PickMeUp.Core.Services.Email.Templates;
using PickMeUp.Enums.UserPickUpRequest;
using RazorLight;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PickMeUp.Core.Services.Email;

internal class EmailService(
    IOptions<EmailSettings> emailSettings,
    IRazorLightEngine razorEngine,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly IRazorLightEngine _razorEngine = razorEngine;
    private readonly ILogger<EmailService> _logger = logger;

    /// <inheritdoc/>
    public async Task<Result> SendEmailConfirmationAsync(string email, string confirmationToken, string confirmationUrl)
        => await SendEmailAsync(new SendEmailParams<EmailConfirmationViewModel>
        {
            TemplateName = "EmailConfirmation",
            ToEmail = email,
            Subject = "Conferma il tuo indirizzo email - PickMeUp",
            Model = new EmailConfirmationViewModel
            {
                ConfirmationUrl = confirmationUrl,
                ExpirationHours = 24
            }
        });

    /// <inheritdoc/>
    public async Task<Result> SendPasswordResetAsync(string email, string resetUrl)
        => await SendEmailAsync(new SendEmailParams<PasswordResetViewModel>
        {
            TemplateName = "PasswordReset",
            ToEmail = email,
            Subject = "Reimpostazione Password - PickMeUp",
            Model = new PasswordResetViewModel
            {
                ResetUrl = resetUrl,
                ExpirationHours = 1
            }
        });

    /// <inheritdoc/>
    public async Task<Result> SendWelcomeEmailAsync(string email, string firstName)
        => await SendEmailAsync(new SendEmailParams<WelcomeEmailViewModel>
        {
            TemplateName = "WelcomeEmail",
            ToEmail = email,
            Subject = "Benvenuto su PickMeUp!",
            Model = new WelcomeEmailViewModel
            {
                FirstName = firstName
            }
        });

    /// <inheritdoc/>
    public async Task<Result> SendPickUpRequestReceivedAsync(
        string travelOwnerEmail,
        string travelOwnerFirstName,
        string requesterFirstName,
        string requesterLastName,
        string departureAddress,
        string destinationAddress,
        string pickUpPointAddress)
        => await SendEmailAsync(new SendEmailParams<PickUpRequestReceivedViewModel>
        {
            TemplateName = "PickUpRequestReceived",
            ToEmail = travelOwnerEmail,
            Subject = "Nuova Richiesta di Passaggio - PickMeUp",
            Model = new PickUpRequestReceivedViewModel
            {
                TravelOwnerFirstName = travelOwnerFirstName,
                RequesterFirstName = requesterFirstName,
                RequesterLastName = requesterLastName,
                DepartureAddress = departureAddress,
                DestinationAddress = destinationAddress,
                PickUpPointAddress = pickUpPointAddress
            }
        });

    /// <inheritdoc/>
    public async Task<Result> SendPickUpRequestStatusChangedAsync(
        string requesterEmail,
        string requesterFirstName,
        string travelOwnerFirstName,
        UserPickUpRequestStatus status,
        string departureAddress,
        string destinationAddress,
        DateTime departureDateTime)
        => await SendEmailAsync(new SendEmailParams<PickUpRequestStatusChangedViewModel>
        {
            TemplateName = "PickUpRequestStatusChanged",
            ToEmail = requesterEmail,
            Subject = status == UserPickUpRequestStatus.Accepted 
                ? "Richiesta Accettata - PickMeUp" 
                : "Richiesta Rifiutata - PickMeUp",
            Model = new PickUpRequestStatusChangedViewModel
            {
                RequesterFirstName = requesterFirstName,
                TravelOwnerFirstName = travelOwnerFirstName,
                Status = status,
                DepartureAddress = departureAddress,
                DestinationAddress = destinationAddress,
                DepartureDateTime = departureDateTime
            }
        });

    /// <inheritdoc/>
    public async Task<Result> SendPickUpRequestCancelledAsync(
        string travelOwnerEmail,
        string travelOwnerFirstName,
        string requesterFirstName,
        string requesterLastName,
        string departureAddress,
        string destinationAddress,
        DateTime departureDateTime)
        => await SendEmailAsync(new SendEmailParams<PickUpRequestCancelledViewModel>
        {
            TemplateName = "PickUpRequestCancelled",
            ToEmail = travelOwnerEmail,
            Subject = "Richiesta Annullata - PickMeUp",
            Model = new PickUpRequestCancelledViewModel
            {
                TravelOwnerFirstName = travelOwnerFirstName,
                RequesterFirstName = requesterFirstName,
                RequesterLastName = requesterLastName,
                DepartureAddress = departureAddress,
                DestinationAddress = destinationAddress,
                DepartureDateTime = departureDateTime
            }
        });

    /// <summary>
    /// Sends an email using SMTP configuration.
    /// </summary>
    private async Task<Result> SendEmailAsync<TModel>(SendEmailParams<TModel> requestParams)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);

            // Configure SMTP client
            client.EnableSsl = _emailSettings.EnableSsl;

            // Only use credentials if username is provided (Mailpit doesn't need auth)
            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            }

            // Create message
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = requestParams.Subject,
                Body = await RazorTemplateHelper
                    .RenderTemplateAsync(_razorEngine, requestParams.TemplateName, requestParams.Model),
                IsBodyHtml = true,
            };
            message.To.Add(requestParams.ToEmail);

            // Send email
            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", requestParams.ToEmail, requestParams.Subject);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }

        return Result.Success();
    }

    private class SendEmailParams<TModel>
    {
        /// <summary>
        ///  The name of the Razor template to use.
        /// </summary>
        public required string TemplateName { get; set; }

        /// <summary>
        /// The recipient email address.
        /// </summary>
        public required string ToEmail { get; set; }

        /// <summary>
        /// The email subject.
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// The model to pass to the Razor template.
        /// </summary>
        public required TModel Model { get; set; }
    }
}
