using PickMeUp.Enums.UserPickUpRequest;
using System;

namespace PickMeUp.Core.Services.Email.Templates;

internal class PickUpRequestStatusChangedViewModel
{
    public required string RequesterFirstName { get; set; }
    public required string TravelOwnerFirstName { get; set; }
    public required UserPickUpRequestStatus Status { get; set; }
    public required string DepartureAddress { get; set; }
    public required string DestinationAddress { get; set; }
    public required DateTime DepartureDateTime { get; set; }
}
