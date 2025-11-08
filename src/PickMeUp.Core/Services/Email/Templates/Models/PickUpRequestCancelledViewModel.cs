using System;

namespace PickMeUp.Core.Services.Email.Templates;

internal class PickUpRequestCancelledViewModel
{
    public required string TravelOwnerFirstName { get; set; }
    public required string RequesterFirstName { get; set; }
    public required string RequesterLastName { get; set; }
    public required string DepartureAddress { get; set; }
    public required string DestinationAddress { get; set; }
    public required DateTime DepartureDateTime { get; set; }
}
